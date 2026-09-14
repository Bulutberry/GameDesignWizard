using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameDesignWizard.Core.Ai;

namespace GameDesignWizard.Ai;

public sealed class LlamaServerLocalAiService : ILocalAiService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<LocalAiGenerationResult> ImproveGddSectionAsync(
        LocalAiConfiguration configuration,
        GddAssistRequest request,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        LocalAiConfigurationValidator.ThrowIfInvalid(configuration);
        if (string.IsNullOrWhiteSpace(request.ExistingContent))
        {
            throw new InvalidOperationException("Write some section notes before asking the local model to improve them.");
        }

        var port = ReserveLoopbackPort();
        var apiKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
        var logs = new BoundedLogBuffer(30);
        using var process = StartServer(configuration, port, apiKey, logs);
        using var requestTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(configuration.RequestTimeoutSeconds));
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            requestTimeout.Token);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            progress?.Report("Loading the local model in an isolated process...");
            await WaitUntilReadyAsync(
                process,
                port,
                TimeSpan.FromSeconds(configuration.StartupTimeoutSeconds),
                logs,
                linkedCancellation.Token);

            progress?.Report("Improving the section locally...");
            using var httpClient = CreateClient(port, apiKey);
            using var response = await httpClient.PostAsync(
                "/v1/chat/completions",
                CreateChatRequest(configuration, request),
                linkedCancellation.Token);
            var responseText = await response.Content.ReadAsStringAsync(linkedCancellation.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"The local model returned HTTP {(int)response.StatusCode}: {ExtractServerError(responseText)}");
            }

            var parsedResponse = JsonSerializer.Deserialize<ChatCompletionResponse>(responseText, JsonOptions);
            var content = parsedResponse?.Choices.FirstOrDefault()?.Message.Content?.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("The local model returned an empty response.");
            }

            stopwatch.Stop();
            progress?.Report("A local proposal is ready for review.");
            return new LocalAiGenerationResult(
                content,
                stopwatch.Elapsed,
                parsedResponse?.Usage?.PromptTokens,
                parsedResponse?.Usage?.CompletionTokens);
        }
        catch (OperationCanceledException) when (requestTimeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"The local AI request exceeded {configuration.RequestTimeoutSeconds} seconds. " +
                "Try a smaller model or increase the timeout in Local AI Setup.");
        }
        finally
        {
            StopProcess(process);
        }
    }

    private static Process StartServer(
        LocalAiConfiguration configuration,
        int port,
        string apiKey,
        BoundedLogBuffer logs)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.GetFullPath(configuration.RuntimePath),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(Path.GetFullPath(configuration.RuntimePath))!
        };
        startInfo.ArgumentList.Add("-m");
        startInfo.ArgumentList.Add(Path.GetFullPath(configuration.ModelPath));
        startInfo.ArgumentList.Add("--host");
        startInfo.ArgumentList.Add(IPAddress.Loopback.ToString());
        startInfo.ArgumentList.Add("--port");
        startInfo.ArgumentList.Add(port.ToString(System.Globalization.CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("-c");
        startInfo.ArgumentList.Add(configuration.ContextSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--n-gpu-layers");
        startInfo.ArgumentList.Add(configuration.GpuLayers.ToString(System.Globalization.CultureInfo.InvariantCulture));
        startInfo.ArgumentList.Add("--api-key");
        startInfo.ArgumentList.Add(apiKey);
        startInfo.ArgumentList.Add("--no-webui");
        startInfo.ArgumentList.Add("--jinja");
        startInfo.ArgumentList.Add("--reasoning");
        startInfo.ArgumentList.Add("off");

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, eventArgs) => logs.Add(eventArgs.Data);
        process.ErrorDataReceived += (_, eventArgs) => logs.Add(eventArgs.Data);
        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("The llama.cpp server process could not start.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            return process;
        }
        catch
        {
            process.Dispose();
            throw;
        }
    }

    private static async Task WaitUntilReadyAsync(
        Process process,
        int port,
        TimeSpan timeout,
        BoundedLogBuffer logs,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        using var httpClient = new HttpClient
        {
            BaseAddress = new Uri($"http://{IPAddress.Loopback}:{port}"),
            Timeout = TimeSpan.FromSeconds(2)
        };

        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (process.HasExited)
            {
                throw new InvalidOperationException(
                    "The llama.cpp server stopped while loading the model." + logs.CreateSuffix());
            }

            try
            {
                using var response = await httpClient.GetAsync("/health", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // The server has not opened its local port yet.
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // A single health request timed out while the model was loading.
            }

            await Task.Delay(350, cancellationToken);
        }

        throw new TimeoutException(
            $"The local model did not finish loading within {timeout.TotalSeconds:0} seconds." + logs.CreateSuffix());
    }

    private static HttpClient CreateClient(int port, string apiKey)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri($"http://{IPAddress.Loopback}:{port}"),
            Timeout = Timeout.InfiniteTimeSpan
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        return client;
    }

    private static StringContent CreateChatRequest(
        LocalAiConfiguration configuration,
        GddAssistRequest request)
    {
        var payload = new
        {
            model = "local-gdd-model",
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "You are an editor for English game design documents. Improve clarity, structure, and practical detail without changing the designer's decisions. Never invent mechanics, features, scope, or facts. Preserve useful constraints. Return only the revised section text in plain text. Do not add a heading, preamble, commentary, markdown fence, or explanation. /no_think"
                },
                new
                {
                    role = "user",
                    content = BuildUserPrompt(request)
                }
            },
            temperature = 0.7,
            top_p = 0.8,
            top_k = 20,
            min_p = 0.0,
            presence_penalty = 1.5,
            max_tokens = configuration.MaxOutputTokens,
            stream = false
        };
        return new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");
    }

    private static string BuildUserPrompt(GddAssistRequest request)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Improve the following GDD section using only the supplied project context.");
        builder.AppendLine();
        AppendValue(builder, "Section", request.SectionTitle);
        AppendValue(builder, "Section purpose", request.SectionGuidance);
        AppendValue(builder, "Game", request.GameName);
        AppendValue(builder, "Overview", request.Overview);
        AppendValue(builder, "Platform", request.Platform);
        AppendValue(builder, "Genre", request.Genre);
        AppendValue(builder, "Subgenre", request.Subgenre);
        AppendList(builder, "Topics", request.Topics);
        AppendList(builder, "Mechanics", request.Mechanics);
        AppendList(builder, "Features", request.Features);
        AppendList(builder, "Art styles", request.ArtStyles);
        AppendValue(builder, "Development duration", request.DevelopmentDuration);
        AppendValue(builder, "Team size", request.TeamSize);
        builder.AppendLine();
        builder.AppendLine("Original section text:");
        builder.AppendLine("<section>");
        builder.AppendLine(request.ExistingContent.Trim());
        builder.AppendLine("</section>");
        return builder.ToString();
    }

    private static void AppendValue(StringBuilder builder, string label, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.Append(label).Append(": ").AppendLine(value.Trim());
        }
    }

    private static void AppendList(StringBuilder builder, string label, IReadOnlyList<string> values)
    {
        if (values.Count > 0)
        {
            builder.Append(label).Append(": ").AppendLine(string.Join(", ", values));
        }
    }

    private static int ReserveLoopbackPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string ExtractServerError(string responseText)
    {
        try
        {
            using var document = JsonDocument.Parse(responseText);
            if (document.RootElement.TryGetProperty("error", out var error))
            {
                if (error.ValueKind == JsonValueKind.Object
                    && error.TryGetProperty("message", out var message))
                {
                    return message.GetString() ?? "Unknown server error";
                }

                return error.ToString();
            }
        }
        catch (JsonException)
        {
            // Fall back to a short plain-text server response.
        }

        return responseText.Length <= 500 ? responseText : responseText[..500];
    }

    private static void StopProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit(5000);
            }
        }
        catch (InvalidOperationException)
        {
            // The process already exited between the checks.
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // Windows already released the process while cleanup was running.
        }
    }

    private sealed record ChatCompletionResponse(
        IReadOnlyList<ChatChoice> Choices,
        ChatUsage? Usage);

    private sealed record ChatChoice(ChatMessage Message);

    private sealed record ChatMessage(string Content);

    private sealed record ChatUsage(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);

    private sealed class BoundedLogBuffer(int capacity)
    {
        private readonly Queue<string> _lines = new(capacity);
        private readonly object _sync = new();

        public void Add(string? line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            lock (_sync)
            {
                _lines.Enqueue(line.Trim());
                while (_lines.Count > capacity)
                {
                    _lines.Dequeue();
                }
            }
        }

        public string CreateSuffix()
        {
            lock (_sync)
            {
                return _lines.Count == 0
                    ? string.Empty
                    : Environment.NewLine + "llama.cpp: " + string.Join(" | ", _lines.TakeLast(4));
            }
        }
    }
}
