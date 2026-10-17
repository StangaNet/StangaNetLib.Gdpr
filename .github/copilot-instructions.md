# StangaNetLib.Gdpr — Copilot instructions

- GDPR toolkit: consent, DSAR, objection/restriction, audit/RoPA/breach, AES-256-GCM pseudonymization.
- Prefer `Result` / `Result<T>` and `Guard` from `StangaNetLib.Core` for expected failures / validation.
- Keep public APIs documented (`///`) and domain-centric (Consent / Dsar / Objection / Audit / Pseudonymization).
- DI: `AddStangaNetLibGdpr`; pipeline: `UseStangaNetLibGdpr` / `UseStangaNetLibGdprRestrictions`.
- Consumers must register persistence contracts (`IConsentRepository`, DSAR services, etc.).
- Match existing naming, file-scoped namespaces, and EditorConfig / Roslyn conventions.
- Tests use xUnit v3 + FluentAssertions; cover both `net8.0` and `net9.0` when changing shared behavior.
