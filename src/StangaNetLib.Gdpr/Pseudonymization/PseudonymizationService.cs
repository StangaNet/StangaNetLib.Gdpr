using Microsoft.Extensions.Options;
using StangaNetLib.Core.Guards;
using StangaNetLib.Gdpr.Configuration;
using StangaNetLib.Gdpr.Pseudonymization.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace StangaNetLib.Gdpr.Pseudonymization;

/// <summary>
/// AES-256-GCM pseudonymization and SHA-256 anonymization service.
/// </summary>
/// <remarks>
/// Encryption format stored as Base64: <c>nonce (12 B) | tag (16 B) | ciphertext</c>.
/// AES-GCM provides both confidentiality and authenticity — any tampering with the
/// ciphertext causes <see cref="Decrypt"/> to throw <see cref="CryptographicException"/>.
/// </remarks>
public sealed class PseudonymizationService : IPseudonymizationService
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes   = 16;

    private readonly byte[] _key;

    /// <summary>
    /// Initialises the service, decoding and validating the AES key from <see cref="GdprSettings.PseudonymizationKey"/>.
    /// </summary>
    /// <param name="options">Bound <see cref="GdprSettings"/>; <see cref="GdprSettings.PseudonymizationKey"/> must be a Base64-encoded 32-byte key.</param>
    /// <exception cref="InvalidOperationException">Thrown when the key is missing or not exactly 32 bytes.</exception>
    public PseudonymizationService(IOptions<GdprSettings> options)
    {
        var base64Key = options.Value.PseudonymizationKey;

        if (string.IsNullOrWhiteSpace(base64Key))
            throw new InvalidOperationException(
                $"{nameof(GdprSettings.PseudonymizationKey)} is not configured. " +
                "Generate one with: Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))");

        _key = Convert.FromBase64String(base64Key);

        if (_key.Length != 32)
            throw new InvalidOperationException(
                $"{nameof(GdprSettings.PseudonymizationKey)} must decode to exactly 32 bytes (256 bits). " +
                $"Got {_key.Length} bytes.");
    }

    /// <inheritdoc/>
    public string Encrypt(string plaintext)
    {
        Guard.Against.Null(plaintext, nameof(plaintext));

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce          = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var ciphertext     = new byte[plaintextBytes.Length];
        var tag            = new byte[TagSizeBytes];

        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        // Pack: nonce | tag | ciphertext
        var combined = new byte[NonceSizeBytes + TagSizeBytes + ciphertext.Length];
        Buffer.BlockCopy(nonce,      0, combined, 0,                              NonceSizeBytes);
        Buffer.BlockCopy(tag,        0, combined, NonceSizeBytes,                 TagSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, combined, NonceSizeBytes + TagSizeBytes,  ciphertext.Length);

        return Convert.ToBase64String(combined);
    }

    /// <inheritdoc/>
    public string Decrypt(string ciphertext)
    {
        Guard.Against.Null(ciphertext, nameof(ciphertext));

        var combined = Convert.FromBase64String(ciphertext);

        if (combined.Length < NonceSizeBytes + TagSizeBytes)
            throw new CryptographicException("Ciphertext is too short to be valid.");

        var nonce     = combined[..NonceSizeBytes];
        var tag       = combined[NonceSizeBytes..(NonceSizeBytes + TagSizeBytes)];
        var data      = combined[(NonceSizeBytes + TagSizeBytes)..];
        var plaintext = new byte[data.Length];

        using var aes = new AesGcm(_key, TagSizeBytes);
        aes.Decrypt(nonce, data, tag, plaintext);

        return Encoding.UTF8.GetString(plaintext);
    }

    /// <inheritdoc/>
    public string Anonymize(string value)
    {
        Guard.Against.Null(value, nameof(value));

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
