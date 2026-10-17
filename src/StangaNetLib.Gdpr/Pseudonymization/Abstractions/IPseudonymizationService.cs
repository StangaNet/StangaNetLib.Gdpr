namespace StangaNetLib.Gdpr.Pseudonymization.Abstractions;

/// <summary>
/// Cryptographic service for GDPR pseudonymization (reversible) and anonymization (irreversible).
/// </summary>
public interface IPseudonymizationService
{
    /// <summary>
    /// Encrypts a plaintext value using AES-256-GCM.
    /// Each call produces a different ciphertext (random nonce) even for the same input.
    /// Returns a Base64 string in the format: nonce (12 B) | tag (16 B) | ciphertext.
    /// </summary>
    /// <param name="plaintext">The UTF-8 value to encrypt.</param>
    /// <returns>Base64-encoded string containing the nonce, authentication tag, and ciphertext.</returns>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts a value previously encrypted with <see cref="Encrypt"/>.
    /// Throws <see cref="System.Security.Cryptography.CryptographicException"/>
    /// if the key is wrong or the data is tampered.
    /// </summary>
    /// <param name="ciphertext">Base64 string produced by <see cref="Encrypt"/>.</param>
    /// <returns>The original plaintext value.</returns>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Produces a deterministic SHA-256 hex digest of the value.
    /// Use for identifiers that must remain consistent but must not be reversible
    /// (e.g., replacing an email with a stable anonymized token in analytics data).
    /// </summary>
    /// <param name="value">The value to anonymize.</param>
    /// <returns>Lowercase hex-encoded SHA-256 hash of the UTF-8 encoded value.</returns>
    string Anonymize(string value);
}
