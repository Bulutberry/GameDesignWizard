# Local AI Setup

GameDesignWizard 0.19 adds the first optional local-AI editing slice. It improves an existing English GDD section while keeping the original text unchanged until the user accepts the proposal.

The app does not bundle or automatically download a runtime or model. These files are large, have independent release cycles, and retain their publishers' licenses.

## Required files

1. Obtain a Windows llama.cpp build containing `llama-server.exe` from the [official llama.cpp releases](https://github.com/ggml-org/llama.cpp/releases). Keep the executable and all DLL files from its archive together.
2. Obtain a compatible GGUF instruction model. The initial small test candidate is the publisher-hosted [`Qwen3-1.7B-Q8_0.gguf`](https://huggingface.co/Qwen/Qwen3-1.7B-GGUF/tree/main), approximately 1.83 GB and published under Apache 2.0.
3. Start GameDesignWizard, open **Settings**, and choose **Local AI Setup**.
4. Select `llama-server.exe` and the `.gguf` file, keep the initial 4,096-token context and 384-token output limit, and choose **Validate & Save**.
5. In wizard step 6, enter text in a GDD section and choose **Improve with Local AI**.
6. Review the separate proposal. Choose **Accept proposal** to replace the editor text, **Discard** to keep the original, or **Cancel** while generation is running. An accepted change has one-level **Undo AI change** until that section view model is replaced.

Use `GPU layers = 0` for the CPU baseline. A value such as `99` asks llama.cpp to offload as many layers as possible and requires a compatible accelerated build, such as a CUDA build for supported NVIDIA hardware. GPU support remains experimental until the runtime/model benchmark is recorded.

## Privacy and process boundary

For each request, GameDesignWizard:

- starts `llama-server.exe` as a hidden child process;
- binds it to a randomly selected port on `127.0.0.1`;
- creates a random API key for that process;
- disables the llama.cpp web interface;
- enables the model's embedded Jinja chat template and requests non-thinking output;
- sends only the active GDD section and compact wizard selections;
- terminates the process after completion, cancellation, timeout, or failure.

GameDesignWizard does not pass a database path or any save operation through the inference API. A server response cannot accept itself; the application applies text only after the user chooses **Accept proposal**. Manual editing, saving, and all exports remain available when local AI is not configured or fails.

## Current prototype limits

- Only **Improve with Local AI** is implemented. Drafting a blank section from notes is planned next.
- Responses appear when generation finishes; token streaming is planned.
- Runtime and model download management, checksums, model profiles, and automatic compatibility testing are planned.
- The initial prompt and model candidates have not passed the 30-case quality gate yet.
- Loading the model for every request favors failure isolation and predictable cleanup over speed. A measured warm-worker policy may follow.

The integration uses llama.cpp's documented [loopback server, health check, API-key option, and OpenAI-compatible chat endpoint](https://github.com/ggml-org/llama.cpp/blob/master/tools/server/README.md). Pin the exact runtime build and model revision before distributing an AI-enabled release.
