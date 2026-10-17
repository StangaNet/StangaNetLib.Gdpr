# StangaNetLib.Gdpr

[![.NET CI](https://github.com/StangaNet/StangaNetLib.Gdpr/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.Gdpr/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)

GDPR compliance toolkit for ASP.NET Core: consent management, DSAR workflows, objection/restriction, audit/RoPA/breach models, and AES-256-GCM pseudonymization. Built on [`StangaNetLib.Core`](https://github.com/StangaNet/StangaNetLib.Core) (`Result`, `Guard`, audit abstractions).

## Design Philosophy

- **Domain-centric**: Consent, DSAR, Objection, Audit, Pseudonymization as capabilities.
- **Functional failures**: domain outcomes map to `Result` / `Result<T>` where appropriate.
- **Regulatory integrity**: typed models aligned to Art. 6/7/12/15/17/18/21/30/33/34.
- **Modern .NET**: `net8.0` / `net9.0` with ASP.NET Core middleware hooks.

## Installation

The package is hosted on **GitHub Packages** (pulls Core from the same feed).

```xml
<!-- NuGet.config — source key must match your packageSourceCredentials -->
<configuration>
  <packageSources>
    <add key="StangaNet" value="https://nuget.pkg.github.com/StangaNet/index.json" />
  </packageSources>
</configuration>
```

```xml
<!-- .csproj -->
<PackageReference Include="StangaNetLib.Gdpr" Version="1.0.0" />
```

See [CHANGELOG.md](./CHANGELOG.md) for release history.

## Core Components

| Namespace | Components | Purpose |
|---|---|---|
| `StangaNetLib.Gdpr.Consent` | `IConsentRepository`, middleware, attributes | Consent / Art. 6–7 |
| `StangaNetLib.Gdpr.Dsar` | Export / deletion / DSAR contracts | Art. 12/15/17/20 |
| `StangaNetLib.Gdpr.Objection` | Restriction / objection / rectification | Art. 16/18/21 |
| `StangaNetLib.Gdpr.Audit` | Breach + RoPA contracts | Art. 30/33/34 |
| `StangaNetLib.Gdpr.Pseudonymization` | AES-GCM + SHA-256 | Pseudonymization |
| `StangaNetLib.Gdpr.Infrastructure.Extensions` | `AddStangaNetLibGdpr`, `UseStangaNetLibGdpr` | DI / pipeline |

## Project Structure

```
StangaNetLib.Gdpr/
├── assets/
│   └── package-icon.png
├── build/
│   └── StangaNetLib.Gdpr.props
├── src/
│   └── StangaNetLib.Gdpr/
│       ├── Audit/
│       ├── Configuration/
│       ├── Consent/
│       ├── Dsar/
│       ├── Infrastructure/
│       ├── Objection/
│       └── Pseudonymization/
└── tests/
    └── StangaNetLib.Gdpr.Tests/
```

## Quick Start

```csharp
builder.Services.AddStangaNetLibGdpr(builder.Configuration);
// Register IConsentRepository (+ DSAR/restriction services) in the host app

app.UseStangaNetLibGdpr();
app.UseStangaNetLibGdprRestrictions(); // optional
```

## CI/CD

On push/PR to `main` and `workflow_dispatch`, the shared StangaNet workflow:

1. Restores, builds, and tests on .NET 8 and .NET 9
2. Packs the NuGet package (with symbols)
3. Publishes to GitHub Packages outside pull requests
4. Uploads the `.nupkg` as a build artifact
