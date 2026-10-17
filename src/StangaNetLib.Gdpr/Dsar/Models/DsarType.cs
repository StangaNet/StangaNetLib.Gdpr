namespace StangaNetLib.Gdpr.Dsar.Models;

/// <summary>Categories of Data Subject Access Requests recognised by the GDPR.</summary>
public enum DsarType
{
    /// <summary>Art. 15 — Right of access: the data subject requests a copy of their personal data.</summary>
    Access = 1,

    /// <summary>Art. 16 — Right to rectification of inaccurate or incomplete personal data.</summary>
    Rectification = 2,

    /// <summary>Art. 17 — Right to erasure ("right to be forgotten").</summary>
    Erasure = 3,

    /// <summary>Art. 18 — Right to restriction of processing.</summary>
    RestrictionOfProcessing = 4,

    /// <summary>Art. 20 — Right to data portability in a structured, machine-readable format.</summary>
    DataPortability = 5,

    /// <summary>Art. 21 — Right to object to processing based on legitimate interests or direct marketing.</summary>
    Objection = 6,

    /// <summary>Art. 22 — Rights related to automated decision-making and profiling.</summary>
    AutomatedDecision = 7,
}
