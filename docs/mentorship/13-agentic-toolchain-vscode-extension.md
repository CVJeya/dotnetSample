# 13. Toolchain & a Deployable VS Code Agent Plug-in

**Part A** is the install list: every tool you need on the machine before you write a line of agentic code. It has two layers — **§1.1–1.5** is the toolchain *this plug-in itself* needs, and **§1.6–1.8** is the general **AI-agent development stack** (model runtime, orchestration framework, retrieval, evals, tracing) plus the track table that tells you which of it to actually install.
**Part B** is the plug-in: how you take the engine from `12-build-your-own-agent-tool.md` and turn it into a **packaged, versioned, installable VS Code extension** — the deployable form of the tool you are using right now.

Companion to `12-build-your-own-agent-tool.md` (the engine: agent loop, tools, verification), `06-recent-tech.md` (AI/LLM), `04-architecture-system-design.md` (ADRs), and the W20 "AI in dev + RAG" week in `02-roadmap.md`.

> **Framing:** doc 12 built the *engine*. This doc builds the *product*. An engine that only runs on your laptop in a `node` script is not deployable — "deployable" means: a `.vsix` file you can hand to a colleague, install on a clean machine, that behaves identically, is versioned, has a changelog, and can be rolled out or rolled back.

## 0. Acceptance criteria — what "done" means here

You are done with this doc when all of these are true:

- [ ] Every tool in Part A is installed and its **verify command** passes.
- [ ] `F5` launches an Extension Development Host with your extension active.
- [ ] A command in the Command Palette starts a run against a real work item.
- [ ] The extension edits files in a `git worktree`, runs `dotnet build`, and shows build errors in the **Problems** panel.
- [ ] `npx @vscode/vsce package` produces a `.vsix` **under ~1 MB**.
- [ ] The `.vsix` installs on a second machine (or a WSL/Remote-SSH host) and runs without you present.
- [ ] The extension carries a version number, a `CHANGELOG.md`, and a rollback path.
- [ ] `ai-stack-check.ps1` (§1.8) passes for every tool your chosen track (§1.7) actually requires — and you can name the tools you *deliberately* skipped.

---

# Part A — The toolchain to install

## 1. What you install, and why (the tables)

### 1.1 Core runtimes

| Tool | Why an agent needs it | Version floor | Install (Windows) | Verify |
|---|---|---|---|---|
| **VS Code (stable)** | The host. Your extension is a guest in its process model. | **≥ 1.90** for chat + language-model APIs | `winget install Microsoft.VisualStudioCode` | `code --version` |
| **Node.js LTS** | Extension host runs on Node; esbuild and vsce are Node tools. | **≥ 20 LTS** (22/24 fine) | `winget install OpenJS.NodeJS.LTS` | `node --version` |
| **npm** (bundled) | Bundling, testing, packaging, publishing. | ≥ 10 | ships with Node | `npm --version` |
| **Git** | Worktrees, branches, diffs — the isolation primitive. | **≥ 2.30** (worktree maturity) | `winget install Git.Git` | `git --version` |
| **.NET SDK** | Runs the verification ladder (`dotnet build` / `dotnet test`). | matching your target TFM | `winget install Microsoft.DotNet.SDK.8` | `dotnet --list-sdks` |

> **Real finding from this machine:** VS Code **1.141.2**, Node **v24.18.0**, npm **11.16.0**, Git **2.55.0.windows.5**. But `dotnet --list-sdks` shows **only SDK 10.0.401**, while the solution targets **net8.0** (runtimes 8.0.31 and 10.0.12 are both present). That works — SDK 10 can build net8.0 — but your agent must never *assume* the SDK, because rebuilding happens inside the tool. Have the agent run `dotnet --list-sdks` and fail loudly rather than guessing.

### 1.2 Extension-development toolchain

| Tool | Why | Install | Verify |
|---|---|---|---|
| **Yeoman + generator-code** | Scaffolds the extension (only needed once). | `npm i -g yo generator-code` | `yo --version` |
| **`@vscode/vsce`** | Packages and publishes the `.vsix`. The actual deployment tool. | `npm i -g @vscode/vsce` | `vsce --version` |
| **`ovsx`** *(optional)* | Publishes to Open VSX — the route when your org blocks the VS Marketplace. | `npm i -g ovsx` | `ovsx --version` |
| **TypeScript** | The extension language. | *per-project* `npm i -D typescript @types/vscode @types/node` | `npx tsc --version` |
| **esbuild** | Bundles `src/**` + deps into one `dist/extension.js`. | *per-project* `npm i -D esbuild` | `npx esbuild --version` |
| **ESLint + Prettier** | Same formatting discipline as the repo (`npm run format`). | *per-project* | `npx eslint -v` |
| **`@vscode/test-cli` + `@vscode/test-electron`** | Integration tests that boot a real VS Code. | *per-project* `npm i -D @vscode/test-cli @vscode/test-electron mocha` | `npx vscode-test --help` |

Install globals once; everything else is a **`devDependency`**. That is deliberate — see §16.1: anything you ship in `dependencies` ends up inside the `.vsix`.

### 1.3 Model access

| Option | When to choose it | Install / setup | Verify |
|---|---|---|---|
| **GitHub Copilot Chat** (feeds `vscode.lm`) | Fastest start; zero auth code. **Requires a Copilot licence.** | Install the *GitHub Copilot Chat* extension, sign in | `vscode.lm.selectChatModels({})` returns ≥ 1 model |
| **Azure OpenAI** (in-tenant) | **Required for LHDN/production code** — data residency, no public model. | Azure resource + deployment in your tenant; `npm i @azure/identity` (Entra ID) | a 1-token prompt returns |
| **Any OpenAI-compatible endpoint** | Vendor-neutral fallback. | plain `fetch`, no SDK | `curl` the `/models` route |
| **Ollama** (local) | Offline work, evals, no data leaves the laptop. | `winget install Ollama.Ollama` then `ollama pull qwen2.5-coder:7b` | `ollama list` |

> **The trap nobody warns you about:** `vscode.lm.selectChatModels()` returns an **empty array** when the user has no Copilot subscription. That is not an error — it is silent. Your first-run experience must detect it and either explain it or fall back to your own Azure endpoint. This is the #1 "it works on my machine" bug in agent extensions.

### 1.4 Agentic extras (the "agentic" part)

| Tool | Why | Install |
|---|---|---|
| **ripgrep (`rg`)** | Fast code search — *if* you shell out. | `winget install BurntSushi.ripgrep.MSVC` |
| **MCP SDK** *(optional)* | Let your agent consume MCP servers instead of writing every tool. | `npm i @modelcontextprotocol/sdk` |
| **Docker / Podman** *(optional)* | Sandbox MCP servers and run the agent against a disposable environment. | `winget install Docker.DockerDesktop` |
| **`@azure/identity`** | Entra ID auth instead of API keys — the auditable choice. | `npm i @azure/identity` |

> **Real finding:** `where rg` returns **nothing** on this machine — ripgrep is not on PATH, and I could not find a bundled copy at the classic VS Code path either. **Do not build your search tool on a binary you hope is on the PATH.** Use the editor's own search API (`workspace.findTextInFiles`) or vendor the binary as a dependency. See §12.2.

### 1.5 Ops / reproducibility

| Tool | Why | Install |
|---|---|---|
| **Dev Containers extension** | Reproducible toolchain for every contributor + CI parity. | `code --install-extension ms-vscode-remote.remote-containers` |
| **`nvm-windows` / `fnm`** | Pin the Node major so CI and laptop agree. | `winget install CoreyButler.NVMforWindows` |
| **`glab`** *(optional)* | Inspect the GitLab pipeline that will build your `.vsix`. | `winget install glab.glab` |

### 1.6 The AI-agent development stack — models, frameworks, data, evals, tracing

> §1.1–1.5 installs exactly what *this* tool needs. This section is the layer underneath **any** AI agent you build: where tokens come from, what runs the loop, where retrieved context lives, and how you know it works. Treat it as a **menu, not a checklist** — §1.7 tells you which column to take.

#### 1.6.1 Language and package manager

| Tool | Why | Install | Verify |
|---|---|---|---|
| **Python 3.12.x** | The lingua franca of agent frameworks, evals and model tooling — research lands here months before anywhere else. Needed for every Python row below. | `winget install Python.Python.3.12` — **then disable the Store alias, see the trap** | `python -V` → `Python 3.12.x` |
| **`uv`** | Replaces pip + venv + poetry/pipenv in one binary: 10–100× faster, writes a real `uv.lock`, and `uvx` runs one-off eval CLIs without polluting your environment. | `winget install astral-sh.uv` | `uv --version` |
| **`pipx`** *(optional)* | Only if a tutorial or a tool insists on the classic pip route while keeping CLIs isolated. | `uv tool install pipx` | `pipx --version` |
| **Node 22 LTS + npm** | Already required by Part B — the in-editor agent loop and the JS eval tooling (`promptfoo`) are npm packages. | see §1.1 | `npm --version` |
| **.NET 8 SDK + `Microsoft.Extensions.AI`** | This is a net8.0 repo. `Microsoft.Extensions.AI` is the .NET-native abstraction for chat clients, embeddings and tool calling; Semantic Kernel composes on top of it. **Full ASP.NET Core walkthrough: `14-microsoft-extensions-ai.md`.** | see §1.1; `dotnet add package Microsoft.Extensions.AI` | `dotnet --list-sdks` shows 8.x |

> **Real finding (this machine): `python` "exists" but is a decoy.** `Get-Command python` resolves to `C:\Users\Jeya Work\AppData\Local\Microsoft\WindowsApps\python.exe` — that is the Microsoft Store **App Execution Alias stub**, not an interpreter. `python -V` prints nothing, and `pip`, `py` and `uv` are all absent. Every "install Python then `pip install langchain`" guide dies here with a misleading error. Fix: install 3.12 via winget, then *Settings → Apps → Advanced app settings → App execution aliases* → turn **off** `python.exe` / `python3.exe`, then confirm `where.exe python` points at a real `…\Python312\python.exe`. Same species of trap as the missing `rg` in §1.4.

#### 1.6.2 Model runtime / inference — where the tokens come from

Two separate decisions people wrongly merge: **where the dev loop points** and **where production points**. They are not the same endpoint.

| Option | When to choose it | Install | Verify |
|---|---|---|---|
| **Azure OpenAI** (in-tenant) | **Default for anything touching LHDN code** — residency, no egress, Entra ID auth, forecastable quota. | Azure resource + model deployment; `winget install Microsoft.AzureCLI` | `az login` then `az account show` |
| **GitHub Copilot** (via `vscode.lm`) | Zero-auth dev loop *inside* the extension. Fastest path to "does my loop work". | see §1.3 / §10 | `selectChatModels({})` returns ≥ 1 |
| **Ollama** | Offline prompt smoke tests and local embeddings; no budget, no data leaving the machine. CPU-only is fine for this. | `winget install Ollama.Ollama` | `ollama run llama3.2:3b "hi"` |
| **LM Studio** | GUI model management; best for comparing quantisations without CLI fiddling. Exposes an OpenAI-compatible server on `:1234`. | `winget install ElementLabs.LMStudio` | `http://localhost:1234/v1/models` responds |
| **Foundry Local** | Microsoft's Windows-native ONNX runtime — no Python, no GPU required, auto hardware acceleration. Best fit if you want local inference **inside a .NET/Windows product**. | `winget install Microsoft.FoundryLocal` | `foundry model list` |
| **`llama.cpp`** | Lowest-level control: GGUF quantisations, CPU thread tuning. Install only when debugging inference itself. | `winget install llama.cpp` (or build) | `llama-cli --version` |
| **vLLM** | High-throughput serving for real GPU traffic. | **Linux only** → WSL2 or a container | `vllm serve <model>` |
| **ONNX Runtime GenAI / DirectML** | Embedding a small model inside a shipped Windows app with no service dependency. | NuGet `Microsoft.ML.OnnxRuntimeGenAI` | model loads in a unit test |

> **Real finding (this machine): hardware forbids a local dev model.** GPU is an **AMD Radeon 610M integrated** adapter (~2 GB reported VRAM, shared with system RAM), CPU is **8 logical cores**, RAM is **13.8 GB**, and `nvidia-smi` is absent (no CUDA). A 7B Q4 model needs ~5 GB of weights *plus* KV cache: it will technically load, but on CPU it generates a few tokens/second — and an agent loop makes dozens of calls per task, each carrying a large context. **Do not build the loop against a local model here.** Use Ollama for single-shot prompt checks and embeddings; point the agent at Azure OpenAI (§4). A genuinely local *product* feature is a hardware purchase (16 GB+ VRAM), not a software choice.

#### 1.6.3 Orchestration frameworks — pick ONE

The framework is maybe 200 lines of what you would write anyway; you adopt it for the ecosystem, not the loop. Installing three is how people spend a quarter learning none.

| Framework | Language | Choose it when | Install |
|---|---|---|---|
| **Microsoft.Extensions.AI** | C#/.NET | You want the *abstractions only* (`IChatClient`, `IEmbeddingGenerator`, tool calling) with no framework opinion. **Start here in .NET.** | `dotnet add package Microsoft.Extensions.AI` |
| **Semantic Kernel** | C#/.NET (+Python) | The .NET-native agent framework: plugins, planners, memory, filters, Azure AI integration. **Best fit for this repo's stack.** | `dotnet add package Microsoft.SemanticKernel` |
| **LangGraph** | Python (+JS) | You need an explicit **state machine**: nodes, edges, checkpoints, human-in-the-loop interrupts, durable resume. Closest match to the loop in doc 12 §4. | `uv add langgraph` |
| **LangChain** | Python (+JS) | Breadth of loaders/integrations. Use as an adapter library, never as your architecture. | `uv add langchain` |
| **LlamaIndex** | Python (+TS) | Retrieval-first agents: ingestion, chunking, hybrid search, re-ranking, query engines. | `uv add llama-index` |
| **Pydantic AI** | Python | Type-safe agents with validated structured output and DI-style dependencies. The best "just enough framework". | `uv add pydantic-ai` |
| **OpenAI Agents SDK** | Python (+JS) | Thin, vendor-aligned primitives — agents, handoffs, guardrails, tracing. Good for learning the shapes. | `uv add openai-agents` |
| **CrewAI** / **AutoGen** | Python | Role-based multi-agent demos. Seductive; rarely what production needs. | `uv add crewai` |
| **OpenHands** / **SWE-agent** | Python | You want to study a *reference implementation* of a coding agent end-to-end. | clone; run in a container |

> **Recommendation for your situation:** **Semantic Kernel + `Microsoft.Extensions.AI`** in C# for anything that must live in this repo, **`vscode.lm`** in TypeScript for the editor plug-in (§10), and **Python only as a sidecar** for evals and spikes. One language owns production; Python is allowed to be disposable. (MCP tool servers are already covered in §1.4 — don't re-install them here.)

#### 1.6.4 Retrieval, embeddings and run state

| Tool | Why | Install |
|---|---|---|
| **PostgreSQL + `pgvector`** | Vector search inside a database you already know how to back up, secure and operate. Best default when data must stay in-tenant. | `winget install PostgreSQL.PostgreSQL`, then `CREATE EXTENSION vector;` |
| **Qdrant** | Purpose-built vector DB with metadata filtering and a clean local Docker story. | `docker run -p 6333:6333 qdrant/qdrant` |
| **Chroma** | Embedded, zero-ops vector store for prototypes. | `uv add chromadb` |
| **FAISS** | In-process similarity search, vectors in RAM, no server. | `uv add faiss-cpu` |
| **`sqlite-vec`** | Vectors in a single file: no server, no Docker. **The realistic choice on this machine** (§1.6.6). | `uv add sqlite-vec` |
| **Azure AI Search** | The in-tenant enterprise path: hybrid BM25 + vector, semantic re-ranking, RBAC, private endpoints. | Azure portal + SDK |
| **Embedding model** | `text-embedding-3-small` (Azure OpenAI — cheap, in-tenant, same auth as §1.6.2) is the sane default; `nomic-embed-text` / `bge-m3` via Ollama for offline work. | same auth path as §1.6.2 |
| **Tokenizer** | You must *count* tokens to set budgets, not estimate them. | `uv add tiktoken` / `npm i tiktoken` |
| **SQLite / Redis** | Checkpoint + run state so a run survives a reload (Part B step 5). SQLite single-machine, Redis shared. | `uv add aiosqlite` / `docker run -p 6379:6379 redis` |

#### 1.6.5 Evaluation and observability — the part that separates an agent from a demo

You cannot improve an agent you cannot measure. Install one eval runner and one tracer **before** you start tuning prompts, or you will tune by vibes.

| Tool | Why | Install |
|---|---|---|
| **promptfoo** | Zero-config eval/test runner for prompts and agents: YAML cases, assertions, red-team probes, comparison UI. **Highest value-per-minute row in this whole section.** | `npm i -g promptfoo` → `promptfoo --version` |
| **Ragas** | Retrieval-specific metrics: faithfulness, answer relevancy, context precision/recall. Use when RAG is the point. | `uv add ragas` |
| **DeepEval** | Pytest-native LLM assertions with CI-friendly thresholds. | `uv add deepeval` |
| **OpenAI Evals** | The reference harness the industry compares against. | `uv add openai-evals` |
| **Langfuse** | Self-hostable tracing: every LLM call, tool call, token count, cost and latency in one trace tree. Open source, stays in-tenant. | Docker Compose (§1.6.6) or Langfuse Cloud |
| **Arize Phoenix** | OpenTelemetry-native tracing + evals, runs locally via pip, good trace UI. | `uv add arize-phoenix` |
| **OpenTelemetry GenAI conventions** | Vendor-neutral `gen_ai.*` spans so traces survive a vendor swap. Pairs with the Aspire dashboard your roadmap already plans for. | `dotnet add package OpenTelemetry.Extensions.Hosting` |
| **JSONL run transcript** | Cheapest observability that exists: append every turn to a file. It is already specified in **§12.4** — do this on day one and add the tools later. | no install |

#### 1.6.6 Serving / ops — only once the agent stops being a plug-in

| Tool | Why | Install |
|---|---|---|
| **Docker Desktop / Podman** | Prerequisite for Langfuse, Qdrant, Redis and vLLM locally. **Requires WSL2.** Listed as optional in §1.4; it becomes mandatory the moment you want traces or a vector server. | `winget install Docker.DockerDesktop` (reboot, enable WSL2) |
| **FastAPI + Uvicorn** | The standard Python surface when the agent must become a service. | `uv add "fastapi[standard]"` |
| **Azure Container Apps / AKS** | Host the agent service with a managed identity to Azure OpenAI — no keys baked into images. | `az containerapp up` |
| **Bicep / Terraform** | Reproducible provisioning of Azure OpenAI + AI Search + Key Vault. | `winget install Microsoft.Bicep` / `Hashicorp.Terraform` |
| **GitLab CI job** | Your eval suite must gate merges, not run by hand. | see §16.4 |
| **Rate limiter + cost governor** | A tool-using loop can burn a month's quota in an afternoon. Cap tokens and turns in code (doc 12 §4). | in code |

> **Real finding (this machine): the container tier is absent.** `docker`, `podman` and `wsl --status` all return empty, so Langfuse, Qdrant, Redis and vLLM cannot run locally as their docs assume. Before you design around them, either install Docker Desktop + WSL2, or take the no-container path in §1.6.7. Do not discover this on the afternoon you need traces.

#### 1.6.7 The no-GPU, no-Docker starter set — what to install first

Six rows get you a working agent dev loop **this weekend** on this machine:

| Need | Install | Replaces |
|---|---|---|
| Interpreter | `winget install Python.Python.3.12` + `astral-sh.uv` (+ disable the Store alias) | the rest of the Python ecosystem |
| Model | Azure OpenAI in-tenant (§4) | local GPU inference |
| Loop | Semantic Kernel **or** `vscode.lm` (§10) | LangChain / LangGraph |
| Retrieval | `sqlite-vec` + `tiktoken` | Qdrant / FAISS / Azure AI Search |
| Evals | `npm i -g promptfoo` | Ragas / DeepEval / hosted eval SaaS |
| Traces | JSONL transcript (§12.4) + `getDiagnostics` evidence (§13) | Langfuse / Phoenix |

Everything else in §1.6 is a **deliberate upgrade bought with a specific pain**: add a vector DB when keyword search demonstrably fails, add Langfuse when you cannot explain a latency regression, add vLLM when you outgrow a hosted endpoint. Upgrades you cannot justify are just dependencies.

### 1.7 Pick a track — install one column, not three

| | **Track A — .NET / product** | **Track B — TypeScript / editor** | **Track C — Python / research** |
|---|---|---|---|
| **Deliverable** | An agent feature inside LHDN_STSDS (net8.0) | The VS Code plug-in (Part B) | Evals, spikes, anything > 1 day of flux |
| **Runtime** | .NET 8 SDK | Node 22 LTS | Python 3.12 + uv |
| **Model client** | `Microsoft.Extensions.AI` → Azure OpenAI via `DefaultAzureCredential` (§4) | `vscode.lm` with Azure OpenAI fallback (§10) | `openai` / `azure-ai-inference` SDK |
| **Framework** | Semantic Kernel | none by default — the loop is yours | LangGraph **or** Pydantic AI |
| **Retrieval** | Azure AI Search or pgvector | `workspace.findTextInFiles` (+ `sqlite-vec` if needed) | Chroma / FAISS |
| **Evals** | xUnit + a fixture of 20 golden tasks | `@vscode/test-cli` (§14) | promptfoo / Ragas |
| **Traces** | OpenTelemetry → Aspire dashboard | JSONL transcript (§12.4) | Langfuse / Phoenix |
| **Extra install** | `dotnet add package Microsoft.SemanticKernel` | `npm i -D @vscode/test-cli` | `npm i -g promptfoo` |

**Deliberately not installed, and why:** a local 70B-class model (no GPU — §1.6.2); Kubernetes locally (one machine, no container runtime); three orchestration frameworks instead of one; Weights & Biases (no team to share with yet); a vector database before you have vectors worth storing; and any hosted observability SaaS that would receive LHDN source code.

### 1.8 Verify the AI stack in one shot

```powershell
# ai-stack-check.ps1 — the AI-layer companion to tools-check.ps1 (§5)
$fail = 0
function Check($name, $block, $hint) {
  $out = & $block 2>&1 | Select-Object -First 1
  if ($LASTEXITCODE -eq 0 -and $out) { Write-Host "[ok]   $name -> $out" }
  else { Write-Host "[FAIL] $name  ($hint)" -ForegroundColor Red; $script:fail = 1 }
}

Check "python"    { python -V }                    "Install 3.12, then DISABLE the Store alias (§1.6.1)"
Check "uv"        { uv --version }                 "winget install astral-sh.uv"
Check "tiktoken"  { uv run python -c "import tiktoken; print('ok')" }  "uv add tiktoken - venv active?"
Check "promptfoo" { promptfoo --version }          "npm i -g promptfoo"
Check "az"        { az account show --query name } "winget install Microsoft.AzureCLI; az login - REQUIRED for keyless Azure OpenAI (§4)"
Check "ollama"    { ollama --version }             "OPTIONAL here - no GPU (§1.6.2)"
Check "docker"    { docker --version }             "OPTIONAL - only for Langfuse/Qdrant/vLLM (§1.6.6)"
Check "rg"        { rg --version }                 "OPTIONAL - prefer workspace.findTextInFiles (§1.4)"

Write-Host ""
Write-Host "GPU / RAM (decides whether a local model is viable at all):"
Get-CimInstance Win32_VideoController | ForEach-Object { "  GPU: $($_.Name)  (~$([math]::Round($_.AdapterRAM/1MB)) MB reported)" }
"  RAM: $([math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory/1GB,1)) GB"

if ($fail -eq 0) { Write-Host "`nAI stack OK. Start with Track A or B (§1.7)." -ForegroundColor Green }
else { Write-Host "`nFix the [FAIL] rows before writing agent code." -ForegroundColor Yellow }
exit $fail
```

On a fresh Windows machine expect `python` and `az` to fail first, with `docker` and `ollama` legitimately missing — and note that **that is still a working configuration.** You can build and ship everything in Part B with Azure OpenAI + `vscode.lm` alone. A red `ollama` row is not a blocker; a red `az` row is, because §4's keyless in-tenant path depends on it.

## 2. Windows-specific setup gotchas

These bite every agent tool, in this order:

```powershell
# 1. Long paths — worktrees + node_modules + deep namespaces WILL exceed 260 chars.
git config --system core.longpaths true

# 2. Developer Mode ON (Settings → System → For developers)
#    Some toolchains create symlinks for node_modules; without it, npm falls back to copying.

# 3. Keep worktrees on the SAME drive as the repo.
#    git worktree across drives (D: repo, E: worktree) breaks relative paths in build output (bin/obj).

# 4. Exclude source + node_modules from Defender real-time scanning (big speed win):
#    Add-MpPreference -ExclusionPath 'D:\RTS\STSDS'

# 5. PowerShell npm.ps1 may be blocked:
#    Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
```

Also decide, and write it down, **where the extension is allowed to touch the disk**. An agent that spawns `git worktree add ../wt-4712` is creating directories outside your repo — that needs to be a documented, configurable path, not a magic string.

## 3. Pin the toolchain (so CI can reproduce your `.vsix`)

Three lines of discipline that save you a weekend later:

```jsonc
// package.json
"engines": { "vscode": "^1.95.0", "node": ">=20" }
```
```bash
# .nvmrc
20
```
```yaml
# devcontainer.json (keeps laptop == CI)
{ "image": "mcr.microsoft.com/devcontainers/typescript-node:20",
  "features": { "ghcr.io/devcontainers/features/dotnet:2": { "version": "8.0" },
                "ghcr.io/devcontainers/features/git:1": {} },
  "postCreateCommand": "npm ci" }
```

> `engines.vscode` is a **contract, not a comment**. `vsce` will refuse to package if `@types/vscode` is newer than your `engines.vscode` floor — and worse, if it is *older-but-mismatched* you get runtime `undefined is not a function` in someone else's editor. Pin both to the same minor.

## 4. Model access and secrets

1. **Decide the model route before coding** — it is an architecture decision, not a config value. For LHDN data it is **in-tenant Azure OpenAI**, and the decision belongs in an ADR (`04-architecture-system-design.md`).
2. **Prefer Entra ID over API keys.** `DefaultAzureCredential` + `getBearerTokenProvider` means no key to leak, no key to rotate, and an audit trail.
3. **Never put a key in `settings.json`.** VS Code `settings.json` is often synced and committed. Use **SecretStorage**:

```ts
await context.secrets.store("myAgent.adoPat", pat);        // write
const pat = await context.secrets.get("myAgent.adoPat");   // read
context.secrets.onDidChange((e) => { /* rotate */ });      // lifecycle
```

4. **Set a cost ceiling on day one.** Tokens are a bill. Track input/output tokens per run, cap `maxTurns`, and surface "this run cost ≈ X". An agent with no budget is an outage waiting for a prompt loop.
5. **Redact before you log.** The transcript (see §13.4) is the debugging tool *and* the leak vector. Strip anything that looks like a PAT, connection string, or `Bearer`.

## 5. Verify the whole toolchain in one shot

```powershell
# tools-check.ps1 — run this before you start, and in CI before packaging
$fail = 0
function Check($name, $cmd, $expected) {
  $out = & $cmd 2>&1 | Select-Object -First 1
  if ($LASTEXITCODE -eq 0) { Write-Host "[ok]   $name -> $out" }
  else { Write-Host "[FAIL] $name (expected $expected)" -ForegroundColor Red; $script:fail = 1 }
}
Check "vscode"    { code --version }                 "1.9x+"
Check "node"      { node --version }                 ">=20"
Check "npm"       { npm --version }                  ">=10"
Check "git"       { git --version }                  ">=2.30"
Check "vsce"      { vsce --version }                 "any"
Check "dotnet"    { dotnet --list-sdks }             ">=8"
if (-not (Get-Command rg -ErrorAction SilentlyContinue)) { Write-Host "[warn] ripgrep not on PATH -> use workspace.findTextInFiles" -ForegroundColor Yellow }
exit $fail
```

---

# Part B — Build the deployable plug-in, step by step

## 6. The mental model: engine → library → shell

```
node script (doc 12)          VS Code extension (this doc)
┌────────────────────┐        ┌──────────────────────────────────────┐
│ CLI loop           │        │ Extension host (Node, in VS Code)    │
│ tools (fs/git/ado) │  ==>   │  ├── contributes (commands, views)   │
│ verification       │        │  ├── chat surface (participant|webview)│
│ worktrees          │        │  ├── SAME engine, as a library       │
└────────────────────┘        │  ├── tools via VS Code APIs          │
                              │  └── secrets/state via ExtensionContext│
                              └──────────────────────────────────────┘
```

Only the **shell** changes. If you wrote doc 12's loop with the tools behind an interface (`readFile`, `search`, `edit`, `run`, `git`, `ado`), porting is a day. If you hard-coded `fs.readFileSync`, you rewrite.

### 6.1 What the tool you are using right now actually ships

From the installed manifest of `steelburn.ado-code` **0.7.4** (`~/.vscode/extensions/steelburn.ado-code-0.7.4/package.json`) — a very instructive target shape:

| Fact | Value | What it teaches |
|---|---|---|
| `engines.vscode` | `^1.85.0` | Pin the floor *below* your dev VS Code (this machine runs 1.141.2) so colleagues aren't locked out. |
| `main` | `./dist/extension.js` | Ship **one bundled file**, not `out/**.js`. |
| `contributes.commands` | **75** | Lots of small commands beat one mega-command. Each is independently keybindable. |
| `contributes.configuration` | **49 properties** | Everything tunable is config, not code. |
| `contributes.viewsContainers` | `activitybar`, `secondaryBar` | A run panel in the activity bar **and** a sidebar surface. |
| `activationEvents` | explicit `onCommand:` / `onView:` / `onWebviewPanel:` | It predates auto-generated events (§7.3) — both styles are valid. |
| `dependencies` | **none (0)** | Every runtime dep is bundled by esbuild and kept in `devDependencies` → tiny `.vsix`. |
| `devDependencies` | esbuild, typescript, eslint, mocha, `@vscode/test-electron`, glob, zod, husky, lint-staged | Bundler + tests + validation schema + git hooks. |
| ships `webview-ui-dist/` | | Its **chat UI is a webview**, not the native chat participant (§10). |
| ships `CHANGELOG.md` (57 KB), `docs/`, `AGENTS.md` | | The docs travel *inside* the extension. |
| scripts include `publish:open-vsx` | | Enterprise-friendly distribution, not only the Marketplace. |

Read that manifest once. It is the answer key to Part B.

## 7. Step 0 — Scaffold the extension

**Goal:** a project that builds, activates, and packages. Nothing more.

```bash
npm i -g yo generator-code
yo code          # → "New Extension (TypeScript)" → enable esbuild bundling
```

Choose the **esbuild** option when offered. If you scaffold by hand, the minimum is:

```jsonc
// package.json — the parts that matter
{
  "name": "my-agent",
  "displayName": "My Agent",
  "publisher": "your-publisher-id",
  "version": "0.1.0",
  "engines": { "vscode": "^1.95.0" },
  "main": "./dist/extension.js",
  "extensionKind": ["workspace"],
  "capabilities": { "untrustedWorkspaces": { "supported": "limited" }, "virtualWorkspaces": false },
  "categories": ["AI", "Other"],
  "activationEvents": [],
  "contributes": {
    "commands": [{ "command": "myAgent.start", "title": "My Agent: Start on Work Item" }],
    "viewsContainers": { "activitybar": [{ "id": "myAgent", "title": "My Agent", "icon": "resources/icon.svg" }] },
    "views": { "myAgent": [{ "id": "myAgent.runs", "name": "Runs" }] },
    "configuration": {
      "title": "My Agent",
      "properties": {
        "myAgent.adoBaseUrl": { "type": "string", "description": "https://dev.azure.com/<org>" },
        "myAgent.branchPrefix": { "type": "string", "default": "agent/" },
        "myAgent.maxTurns": { "type": "number", "default": 25 }
      }
    }
  }
}
```

```jsonc
// scripts — build, watch, package, test
"scripts": {
  "vscode:prepublish": "npm run bundle -- --minify",
  "bundle": "esbuild src/extension.ts --bundle --platform=node --format=cjs --external:vscode --outfile=dist/extension.js",
  "watch": "npm run bundle -- --sourcemap --watch",
  "lint": "eslint src",
  "test": "vscode-test",
  "package": "vsce package --out out/"
}
```

**Why `extensionKind: ["workspace"]`:** your agent spawns `git`, `dotnet` and ripgrep **next to the code**. Over Remote-SSH/Dev Containers the code lives on the remote, so the extension must run there too. Get this wrong and it "works on desktop, silently fails over Remote".

**`.vscodeignore`** — what keeps the `.vsix` small:

```
src/**
test/**
**/*.map
**/*.ts
node_modules/**
!dist/extension.js
```

**Trap:** if you bundle with esbuild, `node_modules/**` can be ignored — *but only because everything is a devDependency*. Add one real `dependency` and it stops being bundled unless esbuild resolves it. Zero-runtime-deps (like ADO Code) is a feature, not a coincidence.

**Done when:** `npm run bundle` writes `dist/extension.js`, and `vsce ls` prints a file list you'd be happy to hand to a security reviewer.

## 8. Step 1 — Activate, first command, output channel

```ts
// src/extension.ts
import * as vscode from "vscode";

export function activate(context: vscode.ExtensionContext) {
  const log = vscode.window.createOutputChannel("My Agent");
  context.subscriptions.push(log);

  context.subscriptions.push(
    vscode.commands.registerCommand("myAgent.start", async () => {
      const ws = vscode.workspace.workspaceFolders?.[0];
      if (!ws) { vscode.window.showErrorMessage("Open a folder first."); return; }
      log.appendLine(`run started in ${ws.uri.fsPath}`);
      await vscode.window.withProgress(
        { location: vscode.ProgressLocation.Notification, title: "My Agent", cancellable: true },
        async (progress, token) => {
          progress.report({ message: "reading work item…" });
          if (token.isCancellationRequested) { log.appendLine("cancelled"); return; }
          // engine call goes here (Step 4)
        }
      );
    })
  );
}

export function deactivate() {}
```

Press **F5** → an Extension Development Host opens. `Ctrl+Shift+P` → *My Agent: Start on Work Item*.

**Trap:** `activate` runs on the UI thread's host process. Never do blocking work in it — return immediately and do the work inside your command. A 10-second `activate` makes the whole editor look broken.

## 9. Step 2 — Choose your chat surface

Two legitimate choices. Pick one deliberately:

| | **A. Native chat participant** | **B. Webview panel** |
|---|---|---|
| How | `contributes.chatParticipants` + `vscode.chat.createChatParticipant` | `contributes.viewsContainers` + `window.registerWebviewViewProvider` |
| Cost | hours | days (CSP, message passing, bundling a UI) |
| You get free | streaming, history, `@`-mentions, follow-ups, references | total layout control: run cards, diff lists, buttons |
| You fight | the host's layout | CSP, `asWebviewUri`, nonce, state restore |
| Worth it when | the agent is a *conversation* | the agent is a *dashboard* of runs |

ADO Code chose **B** (it ships `webview-ui-dist/` and activates on `onWebviewPanel:adoCode.chatEditor`). For a first agent, choose **A** — you'll ship a working surface this weekend instead of next month.

```jsonc
// package.json — option A
"contributes": {
  "chatParticipants": [{
    "id": "myAgent.chat",
    "name": "myagent",
    "description": "Work item → verified PR",
    "isSticky": true,
    "commands": [{ "name": "task", "description": "Run a work item end to end" }]
  }]
}
```

```ts
const handler: vscode.ChatRequestHandler = async (request, context, stream, token) => {
  stream.progress("Fetching work item…");
  stream.markdown(`**Item ${request.prompt}** — starting.\n\n`);
  // engine call; stream progress as each phase finishes
  stream.markdown("| phase | result |\n|---|---|\n| context | 8 files |\n| build | passed |\n");
  stream.button({ command: "myAgent.showDiff", title: "Review changes" });
  stream.followup; // offer suggested next prompts
  return {};
};
context.subscriptions.push(vscode.chat.createChatParticipant("myAgent.chat", handler));
```

**Trap:** `stream.markdown()` takes **Markdown**, not HTML, and the host sanitises it. Emit tables and code fences; don't try to style.

## 10. Step 3 — Model access inside the extension

**Prefer `vscode.lm`** (it inherits the user's Copilot licence, and you write no auth code) — **with a fallback**, because of the silent-empty-array trap from §1.3.

```ts
async function complete(prompt: string, token: vscode.CancellationToken): Promise<string> {
  const [model] = await vscode.lm.selectChatModels({ vendor: "copilot", family: "gpt-4o" });
  if (model) {
    const res = await model.sendRequest([vscode.LanguageModelChatMessage.User(prompt)], {}, token);
    let text = "";
    for await (const fragment of res.text) text += fragment;
    return text;
  }
  return completeViaAzureOpenAI(prompt, token); // your in-tenant route (§4)
}
```

```ts
// The in-tenant fallback — no SDK required, and no key in settings.json
const token = await new DefaultAzureCredential().getToken("https://cognitiveservices.azure.com/.default");
const r = await fetch(`${endpoint}/openai/deployments/${deployment}/chat/completions?api-version=2024-10-21`, {
  method: "POST",
  headers: { Authorization: `Bearer ${token.token}`, "Content-Type": "application/json" },
  body: JSON.stringify({ messages, temperature: 0.1, max_tokens: 4096 }),
});
```

**Notes that matter:**
- APIs in this area move fast. Treat tool *registration* (`contributes.languageModelTools` / `lm.registerTool` — so Copilot's agent mode can call *your* tools) as the fastest-moving surface: check the current API docs before you pin `engines.vscode`.
- Pass the `CancellationToken` into **every** `await`. Pressing the stop button must actually stop the HTTP request, not just the UI spinner.
- `temperature: 0.1` for code. You want determinism, not creativity.
- Count tokens as you go and stop at your ceiling (§4.4).

## 11. Step 4 — The tool layer, implemented with VS Code APIs

This is where you replace doc 12's `child_process` + `fs` tools with editor-native equivalents. Same interface, better citizenship.

| Tool (doc 12) | Use this in the extension | Why it's better |
|---|---|---|
| `list_workspace` | `vscode.workspace.findFiles(include, exclude, max)` | Respects `files.exclude`, respects the workspace, no path juggling |
| `read_file` (ranged) | `vscode.workspace.fs.readFile(uri)` + your own line windowing | Works with virtual/remote file systems |
| `search` | **`vscode.workspace.findTextInFiles`** | **No binary needed** — sidesteps the missing-`rg` problem entirely |
| `edit_file` | `vscode.WorkspaceEdit` + preview | Native undo/redo, native diff, works with SCM |
| `run_command` | `vscode.tasks.executeTask` or `child_process.spawn` | Tasks give you **problem matchers** → errors land in Problems |
| `git` | `vscode.extensions.getExtension("vscode.git")!.exports.getAPI(1)` | Gets the *user's* repo, branch and remotes — no guessing |
| `ado_*` | `fetch` + `context.secrets` | Same REST calls as doc 12 |
| `memory` | `context.workspaceState` / `globalState` (+ `.ado-code/memory/*` files) | Survives reloads; scoped per workspace |

```ts
// search without ripgrep — verified available as an editor API
async function search(pattern: string, token: vscode.CancellationToken) {
  const hits: string[] = [];
  await vscode.workspace.findTextInFiles(
    { pattern },
    { include: "**/*.{cs,ts,md}", maxResults: 200, previewOptions: { matchLines: 2 } },
    (r) => {
      const rel = vscode.workspace.asRelativePath(r.uri);
      r.matches.forEach((m) => hits.push(`${rel}:${m.range.start.line + 1}: ${m.preview.text.trim()}`));
    },
    token
  );
  return hits;
}
```

**Trap:** in a **restricted (untrusted) workspace** the read APIs work but writes/terminal do not. That is why §7 set `capabilities.untrustedWorkspaces.supported: "limited"` — it tells the editor your extension degrades instead of crashing. Never silently no-op an edit; fail with a message the user can act on.

## 12. Step 5 — Move the agent loop into the extension host

The loop is doc 12 §3's loop. Three things change inside a host that can be reloaded at any moment:

```ts
async function runAgent(workItemId: number, ctx: vscode.ExtensionContext, log: vscode.OutputChannel,
                        progress: vscode.Progress<{ message?: string }>, token: vscode.CancellationToken) {
  const transcript: unknown[] = [];
  let turns = 0;
  const maxTurns = vscode.workspace.getConfiguration("myAgent").get<number>("maxTurns", 25);

  while (turns++ < maxTurns) {
    if (token.isCancellationRequested) { log.appendLine("stopped by user"); break; }
    progress.report({ message: `turn ${turns}: planning…` });

    const step = await plan(...);            // model call
    transcript.push({ turn: turns, step });  // 1. audit every turn
    if (step.kind === "done") break;
    await executeTool(step, token);          // 2. every tool takes the token
  }
  await ctx.workspaceState.update("myAgent.lastTranscript", transcript);
  return transcript;
}
```

**12.1 The host can die.** Reload the window mid-run and everything in memory is gone. Persist the transcript (and the run's state) to `context.workspaceState` after every turn, or to `.ado-code/todos/`-style JSON. On activation, offer "resume the last run".

**12.2 Paths differ from your terminal.** `PATH` inside the extension host is not your shell's `PATH`. Resolve `dotnet`/`git` explicitly and fail with a *configurable* override rather than `ENOENT`. This is also why `findTextInFiles` (§11) beats shelling out to `rg`.

**12.3 Show the run, don't hide it.** Implement a `TreeDataProvider` for `myAgent.runs`: one node per phase (context → plan → edit → build → test → PR), with the verification results attached. The user must be able to answer "what is it doing?" without reading logs.

**12.4 Keep a raw transcript** in the output channel as JSONL (one object per turn) — it doubles as your eval corpus and your bug report.

## 13. Step 6 — Review, safety and the verification ladder

Verification is *the* feature (§ doc 12 §3). Doing it in-editor is strictly better than parsing stdout, because the editor already knows how to read compiler output:

```ts
const task = new vscode.Task(
  { type: "shell", task: "dotnet build" },
  vscode.TaskScope.Workspace,
  "dotnet build",
  "myAgent",
  new vscode.ShellExecution("dotnet build LHDN_STSDS.sln --nologo -v q", { cwd: repoRoot }),
  ["$msCompile"]                      // problem matcher → errors appear in the Problems panel
);
await vscode.tasks.executeTask(task);
```

Then either read the editor's view of the result…

```ts
const errors = vscode.languages.getDiagnostics(uri)   // populated by C# Dev Kit / Roslyn
  .filter((d) => d.severity === vscode.DiagnosticSeverity.Error);
```

…or, if no language server is running, parse the task output. **Prefer diagnostics when present** — they are already structured. Feed the errors back to the model as the next turn's input: that feedback loop *is* the agent; everything else is plumbing.

**Safety rules that belong in the code, not the prompt:**

| Rule | Implementation |
|---|---|
| Never edit the user's working tree directly | `git worktree add` first (§ doc 12 §3 step 6) |
| Never push to or commit on a protected branch | Check `git.branches` / the repo API; refuse with a clear message |
| Show the diff before applying | `vscode.commands.executeCommand("vscode.diff", leftUri, rightUri, title)` |
| Respect undo | Apply changes via `WorkspaceEdit` so `Ctrl+Z` works |
| One run = one branch | `agent/<itemId>-<slug>`; delete the worktree on success |
| Global kill switch | Setting `myAgent.enabled: false` → refuse to start, not "start quietly" |

## 14. Step 7 — Test the extension

Two layers, both cheap:

```js
// .vscode-test.mjs — integration: boots a real VS Code
import { defineConfig } from "@vscode/test-cli";
export default defineConfig({ files: "out/test/**/*.test.js", version: "stable", workspaceFolder: "./test-fixture" });
```

```ts
// test/integration/extension.test.ts — the only assertions that really matter
import * as assert from "node:assert";
import * as vscode from "vscode";

suite("activation", () => {
  test("command is registered", async () => {
    const all = await vscode.commands.getCommands(true);
    assert.ok(all.includes("myAgent.start"));
  });
});
```

Plus **unit tests for the pure parts** — prompt building, diff parsing, HTML-to-text stripping of work item descriptions, token budgeting. Those are where the bugs are, and they need no editor to test.

**Trap:** don't test the model. Test the *plumbing around* it, and evaluate the model with the eval set from doc 12 §3 step 13.

## 15. Step 8 — Package the `.vsix`

```bash
npm run lint && npm test
npx @vscode/vsce ls                       # inspect exactly what ships
npx @vscode/vsce package --out out/       # → out/my-agent-0.1.0.vsix
```

Checklist before you call it packaged:

- [ ] `engines.vscode` ≥ the oldest VS Code you intend to support, and `@types/vscode` matches it.
- [ ] `.vsix` under ~1 MB (bundled, no `node_modules`, no `src`, no sourcemaps, no test fixtures).
- [ ] `README.md` shows the first-run experience (what the user sees before it works).
- [ ] `CHANGELOG.md` has a `0.1.0` entry (the real ADO Code's is 57 KB — yours starts small, but starts).
- [ ] `LICENSE` present; `repository` field set.
- [ ] No secrets, no internal hostnames you wouldn't share, no sample PATs in fixtures.
- [ ] `vsce package --pre-release` if you need a channel for testers.

**Trap:** native dependencies (a vendored `rg.exe`, `node-gyp` output) are platform-specific. If you ship one, you ship **per-target** `.vsix` (`--target win32-x64`, `linux-x64`) — another reason to prefer the editor's own APIs.

## 16. Step 9 — Install, distribute, roll out

### 16.1 Sideload (your own machine, and your first "real" test)

```powershell
code --install-extension out\my-agent-0.1.0.vsix --force
code --list-extensions --show-versions | Select-String my-agent
```

Then test it on a **different repository** than the one you built it against. That single step exposes more assumptions than a week of reading.

### 16.2 Over Remote-SSH / Dev Containers / WSL

The extension runs **where the code is**. Install per target:

```powershell
code --remote ssh-remote+devbox --install-extension out\my-agent-0.1.0.vsix
```

If `extensionKind` is wrong (§7), this is the moment it breaks. Add this to your README, because it is the first thing a colleague will hit.

### 16.3 Choose your channel, deliberately

| Channel | When | How |
|---|---|---|
| **VSIX file / UNC share** | Internal tool, few users, fastest. | `\\company\dfs\tools\vscode\my-agent-0.1.0.vsix` + a `rollout.ps1` |
| **Open VSX** | Org blocks the VS Marketplace (common in regulated shops). | `ovsx publish -p $OVSX_TOKEN` — the real ADO Code ships a `publish:open-vsx` script |
| **VS Marketplace** | Public tool. Needs a publisher + PAT. | `vsce publish` |
| **Private gallery** | Enterprise with a mirror of the Marketplace. | Point `product.json`/gallery settings at your mirror, publish there |

For fleet rollout, install it the same way you already provision machines — a line in the DSC/Ansible stanza from `CICD-DEVOPS.md` §8.1 that also sets the IIS slots:

```powershell
# inside the existing provisioning stanza
& code --install-extension "\\dfs\vscode\my-agent-0.1.0.vsix" --force
```

Policy knobs worth knowing: `extensions.allowed` (allowlist what can be installed), `extensions.autoUpdate`, and the workspace **trust** prompt — an agent that can run terminal commands should never be auto-trusted in an untrusted folder.

### 16.4 Build it in CI so the artefact is reproducible

Add a job next to the existing `build`/`deploy` stages in `.gitlab-ci.yml` (that pipeline already runs on a Windows runner with `C:\` paths):

```yaml
package-vsix:
  stage: build
  tags: [windows]
  rules:
    - if: $CI_COMMIT_TAG
  script:
    - npm ci
    - npm run lint
    - npm test
    - npx @vscode/vsce package --out "$env:CI_PROJECT_DIR\out"
  artifacts:
    name: "my-agent-$env:CI_COMMIT_TAG"
    paths:
      - out/*.vsix
    expire_in: 1 year
```

Now the `.vsix` is an artefact of a tag, built from a clean checkout — not something you hand-built on your laptop on a Friday. That is the difference between a demo and a deployable tool.

## 17. Step 10 — Keep it alive

| Practice | Why |
|---|---|
| **SemVer + `CHANGELOG.md`** | You are distributing software to colleagues; "which version do you have?" must have an answer. |
| **Version-consistency check in CI** | The real extension has a `check:version` script. Tag ≠ `package.json` version is the classic release bug. |
| **Rollback path** | Keep the previous `.vsix`; downgrade is `code --install-extension <old>.vsix --force`. |
| **Eval set** (doc 12 §3 step 13) | 20 tasks, fixed; re-run each release. Track success rate and cost per accepted PR. |
| **Opt-in telemetry, redacted** | What you log is your liability. Tokens, timings, tool names — never prompts or file contents without consent. |
| **Keep the API surface pinned** | Deprecate config keys with a migration, don't delete them silently. |

## 18. Troubleshooting — the table you'll actually use

| Symptom | Likely cause | Fix |
|---|---|---|
| "No language model available" | No Copilot subscription → `selectChatModels` returns `[]` silently | Install/sign in to Copilot Chat, or fall back to Azure OpenAI (§10) |
| Command never appears in the palette | `contributes.commands` id ≠ `registerCommand` id | Match the ids; reload the window |
| Works on desktop, silent no-op over Remote-SSH | `extensionKind` wrong | `"extensionKind": ["workspace"]` |
| `spawn dotnet ENOENT` inside the agent, but fine in the terminal | Extension host `PATH` ≠ shell `PATH` | Resolve explicitly + configurable override (§12.2) |
| `spawn rg ENOENT` | ripgrep genuinely not installed (verified on this machine) | Use `workspace.findTextInFiles` (§11) |
| `vsce` refuses to package | `@types/vscode` newer than `engines.vscode` | Align both to the same minor |
| `.vsix` is 40 MB | `node_modules`/`src` shipped | Bundle with esbuild, tighten `.vscodeignore`, run `vsce ls` |
| Edits don't stick | Untrusted workspace, or file outside the workspace folder | Declare `untrustedWorkspaces`, validate every `Uri` against `workspaceFolders` |
| Webview is blank | CSP / missing nonce / wrong `asWebviewUri` | Use the generator's webview scaffold rather than writing CSP by hand |
| Model ignores your tools | Too many tools, or a bloated schema | Cap the active tool set at ~8, shorten descriptions |
| Stop button doesn't stop the run | Token not threaded through every `await` | Pass `token` everywhere; check `isCancellationRequested` each turn |
| Run lost after a window reload | In-memory state | Persist per turn to `workspaceState` (§12.1) |
| Build passes locally, agent says it failed | Parsing stdout instead of using diagnostics | Use the problem matcher + `languages.getDiagnostics` (§13) |
| Worktree weirdness / missing `bin/obj` | Worktree on a different drive, or long paths | Same drive, `core.longpaths true` (§2) |

## 19. Definition of done, effort, and how to talk about it

**Effort** (8 hrs/week, consistent with doc 12 §5):

| Slice | Effort |
|---|---|
| Part A install + verify | 1 evening |
| Steps 0–2 (scaffold, activate, chat surface) | 1 evening |
| Steps 3–6 (model, tools, loop, review + verification) | weekend 1 |
| Steps 7–10 (test, package, deploy, publish) | weekend 2 |
| **Total** | **~2 weekends + 2 evenings** |

**Done when:** you can hand a `.vsix` to a colleague, they install it on a clean machine, point it at a work item in a repository you have never touched, and get a verified PR — without you in the room.

**How to talk about it in an interview** (this is a design question, not a tooling question):

> "How would you introduce AI code generation into a regulated .NET codebase?"

Answer with the shape of what you built: an in-tenant model (residency, no data egress), tools behind an interface, isolation in a `git worktree`, a verification ladder that gates on a real `dotnet build` + tests, write-back to the work item as evidence, and a human review step before any PR. Then name the constraint you refused to violate — e.g. *never auto-merge, never send source to a public endpoint*. That answer is Senior→Lead; the extension is just the proof.

**Cross-links:** `12-build-your-own-agent-tool.md` (the engine — read first), `04-architecture-system-design.md` (the model-residency ADR), `docs/architecture/CICD-DEVOPS.md` §7.4 and §8.1 (blue/green deploy + the DSC provisioning stanza for fleet rollout), `06-recent-tech.md` (AI/RAG), `02-roadmap.md` (W20), `11-resources.md` (learning list).

## 20. Resources

- **VS Code Extension API** — official docs (your primary reference; the Chat and Language Model API chapters change fastest).
- **`microsoft/vscode-extension-samples`** — runnable samples for chat participants, LM tools, webviews, tree views. Copy from here, not from blog posts.
- **`generator-code`** — the scaffolding you already installed; read the generated `esbuild.js`.
- **`@vscode/vsce` + `ovsx`** — packaging and the non-Marketplace publishing route.
- **`@vscode/test-cli`** — integration testing against a real VS Code.
- **Model Context Protocol docs** — before you hand-write the tenth tool, check whether a server already exists.
- **Azure OpenAI + `DefaultAzureCredential`** docs — the in-tenant, keyless path.
- **Agent stack (install tables):** §1.6–1.8 of this doc — model runtimes, orchestration frameworks, retrieval, evals and tracing, with install + verify commands and the track table.
- **Orchestration:** Semantic Kernel docs, `Microsoft.Extensions.AI` overview, LangGraph concepts, Pydantic AI, OpenAI Agents SDK.
- **In-product (.NET):** → [`14-microsoft-extensions-ai.md`](14-microsoft-extensions-ai.md) — `Microsoft.Extensions.AI` explained + 10 steps to ship AI inside an ASP.NET Core app: DI + middleware pipeline, chat endpoint, JSON-schema extraction into existing DTOs, embeddings/RAG, tool calling, streaming (SSE vs Hangfire + SignalR), fake-`IChatClient` tests, redaction/budget/audit middleware, IIS + Key Vault deployment.
- **And make it behave:** `15-prompting-an-ai-agent.md` — the system prompt this extension should load (`prompts/system.md`), the `AGENTS.md` generator, git governance, and the comprehension protocol. §8.1 lists exactly where each product loads it (this plug-in, `copilot-instructions.md`, Cursor rules).
- **And make it portable:** `16-any-repo-agent-prompt.md` — the stack-agnostic bootstrap prompt. Point it at any repository so this extension's `AGENTS.md` and `REPO-BRIEF.md` artefacts are produced by **discovery** (STACK CARD + evidence) rather than by assuming a Node/.NET layout. Pairs with §6.1: the manifest is what a shippable extension declares, this prompt is what makes its output trustworthy.
- **Then learn it for free:** `17-free-learning-path-certifications.md` — the sequenced learning track for everything above: 6 tiers (LLM literacy → structured output → agentic patterns → evals → ship), one free primary source per tier, which certificates are genuinely free vs paid, a 12-week schedule, and the 5 artefacts that beat a certificate.
- **Evals:** promptfoo docs (start with the `promptfoo eval` tutorial), Ragas metrics, DeepEval assertions — plus your own 20-task golden set (doc 12 §8).
- **Tracing:** Langfuse (self-hosted) and Arize Phoenix quickstarts; OpenTelemetry GenAI semantic conventions for the vendor-neutral span names.
- **Local inference:** Ollama model library, Foundry Local docs, and the llama.cpp GGUF quantisation guide — read the quant guide *before* downloading 40 GB.
