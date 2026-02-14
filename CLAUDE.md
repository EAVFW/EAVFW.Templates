# CLAUDE.md — eavfw-templates

This file provides guidance to Claude Code when working with the EAVFW template system.

## Overview

This repo contains `dotnet new` templates for scaffolding new EAVFW (Entity-Attribute-Value Framework) projects. It is a submodule of the main EAVFW repo at `external/eavfw-templates/`.

The templates generate a complete solution structure including .NET backend, Next.js frontend, models, business logic, database scripts, and npm tooling.

## Template Installation

```bash
# Install from local source (from this repo root)
dotnet new install ./templates/EAVFW/
dotnet new install ./templates/EAVFW.NextJS/

# Uninstall (to reinstall after changes)
dotnet new uninstall ./templates/EAVFW/
dotnet new uninstall ./templates/EAVFW.NextJS/

# Rebuild cache after template changes
dotnet new --debug:rebuildcache
```

## Templates

### 1. `eavfw` (Main Template)

**Short name:** `eavfw`
**Identity:** `EAVFW`
**Config:** `templates/EAVFW/.template.config/template.json`

Scaffolds the complete solution structure: C# projects, manifest, models, business logic, npm scripts, database tooling.

#### Parameters

| Parameter | Replaces | Default | Required | Description |
|-----------|----------|---------|----------|-------------|
| `--namespace` | `__EAVFW__` | `EAVFW` | Yes | Namespace for solution/projects. Also used for file renames. |
| `--appName` | `__MainApp__` | `MainApp` | Yes | App name (e.g., "Portal"). Creates `{namespace}.{appName}` |
| `--databaseName` | `__databaseName__` | `databaseName` | Yes | Database name for scripts and connection strings |
| `--schemaName` | `__databaseSchema__` | Falls back to namespace | No | Database schema name (default: coalesce of schemaName or namespace) |
| `--yourUserEmail` | `__userEmail__` | `pks@delegate.dk` | No | Initial admin email for DB seeding |
| `--yourUserName` | `__userName__` | `Poul Kjeldager` | No | Initial admin username |
| `--yourUserPrincipalName` | `__userPrincipalName__` | `PoulKjeldagerSørensen` | No | Admin UPN |
| `--sendgrid_api_token` | `__sendgrid_api_token__` | (none) | No | SendGrid API token for email |
| `--dotnetSDK` | `__DOTNET__SDK__VERSION__` | `10.0.100` | No | .NET SDK version |
| `--targetFramework` | `__TARGET_FRAMEWORK__` | `net10.0` | No | Target framework |
| `--withSecurityModel` | (conditional) | `true` | No | Install EAVFW.Extensions.SecurityModel |
| `--withDocuments` | (conditional) | `true` | No | Install EAVFW.Extensions.Documents |
| `--withConfiguration` | (conditional) | `true` | No | Install EAVFW.Extensions.Configuration |
| `--skipRestore` | (conditional) | `false` | No | Skip NuGet restore |
| `--skipPortal` | (conditional) | `false` | No | Skip UI portal setup |
| `--skipGitCommit` | (conditional) | `false` | No | Skip git init + commit |

#### Auto-generated Symbols

- `usersecretsid` — Random GUID for `UserSecretsId`
- `schemaNameComputed` — Coalesces `schemaName` with `namespace` fallback
- `HttpsPortReplacer` — Random port 44300-44399 (or user-specified)
- `userGuid` — Random GUID for initial admin user

#### Generated Solution Structure

```
{namespace}.sln
├── src/
│   ├── {namespace}.Models/           — EF Core models + manifest.json
│   ├── {namespace}.BusinessLogic/    — Plugin configuration
│   ├── {namespace}.Common/           — Shared services + constants
│   └── {namespace}.ServiceDefaults/  — Aspire shared service defaults (OpenTelemetry, health checks)
├── apps/
│   ├── {namespace}.{appName}/        — ASP.NET Core app (Startup.cs, Startup.g.cs)
│   │   └── Properties/launchSettings.json
│   └── {namespace}.AppHost/          — .NET Aspire AppHost (orchestrator)
│       ├── AppHost.cs               — Aspire setup: SQL Server, MailPit, EAV model
│       └── {namespace}.AppHost.csproj
├── tests/
│   └── {namespace}.AppHost.Tests/    — E2E Playwright integration tests
├── scripts/
│   └── {namespace}.HelperScripts/    — Setup and utility scripts
├── package.json                      — npm workspace root + build scripts
├── global.json                       — .NET SDK version pin
└── .config/dotnet-tools.json         — eavfw-manifest CLI tool
```

#### Aspire Integration

The template includes a .NET Aspire AppHost that orchestrates the full development environment:

- **SQL Server** with persistent data volume, DbGate web UI, and BACPAC restore support
- **MailPit** for local SMTP email testing
- **EAV Model** setup with database publishing and initial admin user seeding
- **OpenTelemetry** observability via ServiceDefaults

The AppHost references `EAVFramework.Extensions.Aspire.Hosting` — via NuGet when `UseEAVFromNuget=true` (default) or via local ProjectReference when developing against a local EAVFramework clone.

**Aspire type naming:** Aspire generates `Projects.{name}` types from project references, replacing dots with underscores. In templates, `__EAVFW_____MainApp__` (5 underscores) produces `{namespace}_{appName}` after placeholder replacement (2 closing `__EAVFW__` + 1 literal `_` + 2 opening `__MainApp__`).

#### Post-Actions (in order)

1. `git init && git checkout -b main && git add . && git commit`
2. `dotnet restore` on solution
3. `dotnet tool restore --no-cache`
4. Install EAVFW extensions (SecurityModel, Documents, Configuration)
5. Set SMTP password via script
6. Run portal setup script (`scripts/setup.cmd`)
7. Final git commit

**Note:** Post-actions currently use `cmd.exe` — they are Windows-oriented. This is an area for improvement (cross-platform support).

### 2. `eavfw-nextjs` (NextJS Frontend Template)

**Short name:** `eavfw-nextjs`
**Identity:** `EAVFW-NextJS`
**Config:** `templates/EAVFW.NextJS/.template.config/template.json`

Adds the Next.js frontend layer to an existing scaffolded project.

#### Parameters

| Parameter | Replaces | Default | Required |
|-----------|----------|---------|----------|
| `--namespace` | `__EAVFW__` | `EAVFW` | Yes |
| `--appName` | `__MainApp__` | `MainApp` | Yes |
| `--skipGitCommit` | (conditional) | `false` | No |
| `--skipCertGen` | (conditional) | `false` | No |

#### Generated Files

```
apps/{namespace}.{appName}/
├── src/
│   ├── pages/          — Next.js pages (dynamic routes for EAVFW)
│   ├── components/     — Custom components + feature registration
│   └── themes/         — Fluent UI theme definitions
├── next.config.js      — Webpack config with transpile modules
├── package.json        — Next.js app package
├── tsconfig.json       — TypeScript config
└── .env                — Environment variables (NEXT_TRANSPILE_MODULES, etc.)
```

### 3. `eavfw-ado` (Azure DevOps Template)

**Short name:** `eavfw-ado`
**Config:** `templates/EAVFW.AureDevOps/.template.config/template.json`

Adds Azure DevOps CI/CD pipeline configuration.

### 4. EAVFW.Blazor

Blazor integration template (in `templates/EAVFW.Blazor/`).

## Template Mechanics

### Placeholder Replacement

The `dotnet new` engine replaces these placeholders in all files:

- `__EAVFW__` → `--namespace` value (also renames files/directories)
- `__MainApp__` → `--appName` value (also renames files/directories)
- `__databaseName__` → `--databaseName` value
- `__databaseSchema__` → computed from `--schemaName` or `--namespace`
- `__UserSecretsId__` → auto-generated GUID
- `__userEmail__` → `--yourUserEmail` value
- `__userName__` → `--yourUserName` value
- `__userPrincipalName__` → `--yourUserPrincipalName` value
- `__userGuid__` → auto-generated GUID
- `__sendgrid_api_token__` → `--sendgrid_api_token` value
- `__DOTNET__SDK__VERSION__` → `--dotnetSDK` value
- `__TARGET_FRAMEWORK__` → `--targetFramework` value

### Conditional Sections

C# files use `#if` preprocessor directives:
```csharp
#if (withSecurityModel)
using EAVFW.Extensions.SecurityModel;
#endif
```

## NuGet Package

Published as `EAVFW.Templates` on NuGet. The `eavfw.csproj` in the root is the package project.

## Typical Usage

```bash
# Full scaffold
dotnet new eavfw --namespace GjellerVand --appName Portal --databaseName GjellerVand \
  --schemaName dbo --yourUserEmail poul@kjeldager.com --allow-scripts yes

# Then inside the scaffolded project, add NextJS
dotnet new eavfw-nextjs --namespace GjellerVand --appName Portal

# Install npm dependencies
npm install --force

# Build
npm run build
```
