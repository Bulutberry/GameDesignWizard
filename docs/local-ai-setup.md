# Local AI Setup

GameDesignWizard 0.20 tests the first optional local-AI editing slice with Qwen3.5. It improves an existing English GDD section while keeping the original text unchanged until the user accepts the proposal.

The app does not bundle or automatically download a runtime or model. These files are large, have independent release cycles, and retain their publishers' licenses.

## Required files

1. Obtain a Windows llama.cpp build containing `llama-server.exe` from the [official llama.cpp releases](https://github.com/ggml-org/llama.cpp/releases). Keep the executable and all DLL files from its archive together. The current test uses build `b10964` and its Windows CUDA 13.3 x64 binary and CUDA runtime archives.
2. Obtain [`Qwen3.5-9B-Q3_K_M.gguf`](https://huggingface.co/unsloth/Qwen3.5-9B-GGUF/blob/3885219b6810b007914f3a7950a8d1b469d598a5/Qwen3.5-9B-Q3_K_M.gguf), approximately 4.67 GB. Qwen publishes the underlying [Apache 2.0 model](https://huggingface.co/Qwen/Qwen3.5-9B); Unsloth publishes this GGUF conversion. The test pins GGUF repository revision `3885219b6810b007914f3a7950a8d1b469d598a5` and expected file SHA-256 `8fed90306e4f019e2bf35f3766470b7bc59ea1a9dae00f5ceb20b43cb5514393`.
3. Start GameDesignWizard, open **Settings**, and choose **Local AI Setup**.
4. Select `llama-server.exe` and the `.gguf` file, keep the initial 4,096-token context and 384-token output limit, and choose **Validate & Save**.
5. In wizard step 6, enter text in a GDD section and choose **Improve with Local AI**.
6. Review the separate proposal. Choose **Accept proposal** to replace the editor text, **Discard** to keep the original, or **Cancel** while generation is running. An accepted change has one-level **Undo AI change** until that section view model is replaced.

For the current Windows CUDA test, the [b10964 release](https://github.com/ggml-org/llama.cpp/releases/tag/b10964) provides two archives that must be extracted into the same folder:

| Archive | Published SHA-256 |
| --- | --- |
| `llama-b10964-bin-win-cuda-13.3-x64.zip` | `cd63ae76ad78a1540aa0f30f6c6284bab14c146d99a58f70c3f0a38cb9c62351` |
| `cudart-llama-bin-win-cuda-13.3-x64.zip` | `1462a050eb4c684921ba51dcc4cc488a036674c3e73e9945ee705b854808d03e` |

These hashes matched the downloaded test copies. The GGUF file and runtime archives remain outside normal Git history and the base application download.

Use `GPU layers = 0` for the CPU baseline. A value such as `99` asks llama.cpp to offload as many layers as possible and requires a compatible accelerated build, such as a CUDA build for supported NVIDIA hardware. Confirm actual memory use with the benchmark before treating an 8 GB GPU as sufficient.

## Privacy and process boundary

For each request, GameDesignWizard:

- starts `llama-server.exe` as a hidden child process;
- binds it to a randomly selected port on `127.0.0.1`;
- creates a random API key for that process;
- disables the llama.cpp web interface;
- enables the model's embedded Jinja chat template and requests non-thinking output through `chat_template_kwargs.enable_thinking=false`;
- sends only the active GDD section and compact wizard selections;
- terminates the process after completion, cancellation, timeout, or failure.

GameDesignWizard does not pass a database path or any save operation through the inference API. A server response cannot accept itself; the application applies text only after the user chooses **Accept proposal**. Manual editing, saving, and all exports remain available when local AI is not configured or fails.

## Current prototype limits

- Only **Improve with Local AI** is implemented. Drafting a blank section from notes is planned next.
- Responses appear when generation finishes; token streaming is planned.
- Runtime and model download management, checksums, model profiles, and automatic compatibility testing are planned.
- The first Qwen3.5-9B [device benchmark](local-ai-benchmark-qwen35.md) covered three CUDA edits and one CPU case; the 30-case quality gate has not been completed.
- Loading the model for every request favors failure isolation and predictable cleanup over speed. A measured warm-worker policy may follow.

The integration uses llama.cpp's documented [loopback server, health check, API-key option, and OpenAI-compatible chat endpoint](https://github.com/ggml-org/llama.cpp/blob/master/tools/server/README.md). Pin the exact runtime build and model revision before distributing an AI-enabled release.
