# Qwen3.5-9B Local AI Smoke Benchmark

This report records the first real GameDesignWizard inference test on September 15, 2026. It is a three-case smoke benchmark of **Improve with Local AI**, not the planned 30-case quality gate or a minimum hardware specification.

## Tested artifacts and machine

| Item | Recorded value |
| --- | --- |
| Model | `Qwen3.5-9B-Q3_K_M.gguf`, Unsloth conversion of Qwen's Apache 2.0 model |
| GGUF repository revision | `3885219b6810b007914f3a7950a8d1b469d598a5` |
| GGUF size | 4,673,643,744 bytes (4.67 GB decimal) |
| GGUF SHA-256 | `8fed90306e4f019e2bf35f3766470b7bc59ea1a9dae00f5ceb20b43cb5514393` (matched) |
| Runtime | llama.cpp `b10964`, commit `b29c606e2`, Windows x64 CUDA 13.3 build |
| GPU | NVIDIA GeForce RTX 4060 Laptop GPU, 8,188 MiB reported VRAM, driver 610.47 |
| Context and output cap | 4,096 context tokens, 384 maximum output tokens |
| Generation | Non-thinking Jinja template; temperature 0.7, top-p 0.8, top-k 20, min-p 0, presence penalty 1.5 |

The [setup guide](local-ai-setup.md) lists the matching runtime archive hashes. The app starts and stops a new private loopback server for every edit, so the elapsed time below includes model loading and generation. GPU memory was sampled about once per second with `nvidia-smi`; Windows process working set was sampled separately. These are approximate peaks, not a combined system-memory requirement.

## Results after prompt revision

| Case | CUDA 13.3, 99 GPU layers | Output tokens | Peak total GPU memory | Peak server working set |
| --- | ---: | ---: | ---: | ---: |
| Sparse gameplay loop | 4.8 s | 25 | 6,023 MiB | 4.87 GB |
| Constraint preservation | 5.0 s | 37 | 6,023 MiB | 4.87 GB |
| Production scope | 5.4 s | 51 | 6,025 MiB | 4.87 GB |

Baseline total GPU memory before the cases was 1,462 MiB, making the observed increase about 4,561–4,563 MiB. The tested 8 GB GPU completed all three cases without an out-of-memory error. Background GPU use and sampling intervals mean this difference should not be treated as an exact model-only allocation.

The CPU baseline ran the sparse-loop case with zero GPU layers in 10.2 seconds for 37 output tokens. Its sampled server working set peaked at 5.17 GB. The CPU test is one case, so it does not establish sustained throughput or performance on lower-memory computers.

The initial prompt asked for practical detail and produced unsupported specifics in the sparse case, including electrical storms disrupting station power and a data-synthesis step. The revised prompt asks the model to reorganize explicit facts only. With that prompt, the same case returned: “The player explores old stations, scans for clues, and returns to the ship while avoiding storms; there is no combat.” The mechanics case retained one-clue-at-a-time scanning, battery drain, ship-only recharging, and exclusions of crafting, dialogue trees, and combat. The production case retained the solo six-month scope, one station, one ship interior, no multiplayer, mostly reusable stylized art, and an undecided clue count.

These three outputs were useful as conservative edits and did not visibly introduce a new mechanic or contradict a stated constraint. They do not establish that the model will behave reliably across genres, long notes, contradictory selections, or repeated generations. Complete the planned 30-case review and cancellation/long-input tests before naming this an approved default model profile. Keep model weights outside Git history and the base application download.
