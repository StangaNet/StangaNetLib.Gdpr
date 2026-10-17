# StangaNetLib.Gdpr

[![.NET CI](https://github.com/StangaNet/StangaNetLib.Gdpr/actions/workflows/main.yml/badge.svg)](https://github.com/StangaNet/StangaNetLib.Gdpr/actions/workflows/main.yml)
![NuGet](https://img.shields.io/badge/nuget-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-512BD4)

StangaNetLib.Gdpr is a domain-driven toolkit for managing GDPR compliance in ASP.NET Core applications. It provides structured implementations for core regulatory requirements, including explicit user consent management, automated Data Subject Access Request (DSAR) workflows, objection handling, audit logging for processing activities, and AES-256-GCM pseudonymization.

## Design Philosophy

- **Domain-Centric**: Organized around regulatory subdomains (Consent, DSAR, Objection, Audit, Pseudonymization) rather than technical layers.
- **Functional Error Handling**: Employs the `Result<T>` pattern for managing domain compliance failures as data, ensuring predictable and type-safe control flows.
- **Regulatory Integrity**: Built to support legal requirements (e.g., Art. 6, 12, 15, 17, 18, 21, 30, 33, 34) through highly typed domain models and abstractions.
- **Modern Standards**: Fully optimized for .NET 8.0 and 9.0, utilizing modern C# features like `record` types and `IReadOnlyCollection<T>`.

## Installation

The package is hosted on **GitHub Packages**.

```xml
<!-- NuGet.config — add the GitHub Packages source -->
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/StangaNet/index.json" />
  </packageSources>
</configuration>
```

```xml
<!-- .csproj -->
<PackageReference Include="StangaNetLib.Gdpr" Version="1.0.0" />
```

---

## Core Components

*The library is organized into functional namespaces. For implementation details, refer directly to the source code.*

| Namespace | Core Components | Purpose |
| :--- | :--- | :--- |
| `StangaNetLib.Gdpr.Consent` | `IConsentRepository`, `ConsentRecord`, `RequireConsentAttribute` | Managing user consent and legal basis (Art. 6/7). |
| `StangaNetLib.Gdpr.Dsar` | `IDsarService`, `DsarRecord`, `DataExportResult` | Automated Data Subject Access Request (DSAR) workflows (Art. 12/15/17/20). |
| `StangaNetLib.Gdpr.Objection` | `IDataProcessingObjectionService`, `ProcessingRestrictionMiddleware` | Handling right to object and restriction of processing (Art. 18/21). |
| `StangaNetLib.Gdpr.Audit` | `IDataBreachService`, `IProcessingActivityRegistry`, `ProcessingActivity` | Compliance logging and breach notification (Art. 30/33/34). |
| `StangaNetLib.Gdpr.Pseudonymization` | `IPseudonymizationService`, `PseudonymizationService` | Secure AES-256-GCM pseudonymization. |
| `StangaNetLib.Gdpr.Configuration` | `GdprSettings`, `DataRetentionSettings` | Compliance configuration management. |

---

## Project Structure

```
StangaNetLib.Gdpr/
├── build/
│   └── StangaNetLib.Gdpr.props  # MSBuild auto-import
├── src/
│   └── StangaNetLib.Gdpr/
│       ├── Audit/          # IDataBreachService, IProcessingActivityRegistry, ProcessingActivity
│       ├── Configuration/  # GdprSettings, DataRetentionSettings
│       ├── Consent/        # IConsentRepository, ConsentRecord, RequireConsentAttribute
│       ├── Dsar/           # IDsarService, DsarRecord, DataExportResult
│       ├── Objection/      # IDataProcessingObjectionService, ProcessingRestrictionMiddleware
│       ├── Pseudonymization/ # IPseudonymizationService, PseudonymizationService
│       └── Infrastructure/  # ServiceCollectionExtensions, ApplicationBuilderExtensions
└── tests/
    └── StangaNetLib.Gdpr.Tests/
```

---

## Quick Start

### Configuration

Define your settings in `appsettings.json`:

```json
{
  "GdprSettings": {
    "DataControllerName": "Acme Corp",
    "DpoEmail": "dpo@acme.com",
    "PseudonymizationKey": "YOUR_BASE64_32BYTE_KEY_HERE"
  }
}
```

### Registration

Register the services in your `Program.cs`:

```csharp
builder.Services.AddStangaNetLibGdpr(builder.Configuration);

// Middleware order is critical:
// app.UseRouting();
// app.UseAuthentication();
// app.UseAuthorization();
// app.UseStangaNetLibGdpr(); // Adds ConsentValidationMiddleware
// app.UseStangaNetLibGdprRestrictions(); // Adds ProcessingRestrictionMiddleware
```

### Usage Example: Enforcing Consent

```csharp
[RequireConsent("Marketing")]
public async Task<Result> SendNewsletter(string userId)
{
    // This endpoint will automatically return HTTP 403 
    // if the user has not granted 'Marketing' consent.
    return await _newsletterService.SendAsync(userId);
}
```
