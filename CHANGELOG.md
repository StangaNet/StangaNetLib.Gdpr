# Changelog

All notable changes to **StangaNetLib.Gdpr** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2026-10-17

### Added
- Consent management contracts and validation middleware (`RequireConsent`).
- DSAR contracts for access/erasure/export workflows (Art. 12/15/17/20).
- Objection and processing-restriction middleware (Art. 18/21).
- Audit models for RoPA and data-breach tracking (Art. 30/33/34).
- AES-256-GCM pseudonymization and SHA-256 anonymization (`IPseudonymizationService`).
- DI (`AddStangaNetLibGdpr`) and pipeline registration helpers.
- Dual targeting `net8.0` / `net9.0`, XML docs, symbols (`snupkg`), package icon, and MSBuild consumer metadata stamp.
- Depends on `StangaNetLib.Core` 1.0.0; Microsoft.Extensions.* pinned to 8.x.
- CI via shared StangaNet reusable workflow (push / PR / manual; publish outside PRs).

[Unreleased]: https://github.com/StangaNet/StangaNetLib.Gdpr/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/StangaNet/StangaNetLib.Gdpr/releases/tag/v1.0.0
