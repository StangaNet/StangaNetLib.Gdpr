using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using StangaNetLib.Gdpr.Configuration;
using StangaNetLib.Gdpr.Pseudonymization;

namespace StangaNetLib.Gdpr.Tests;

public sealed class PseudonymizationServiceTests
{
    private static PseudonymizationService CreateService(byte[]? key = null)
    {
        key ??= RandomNumberGenerator.GetBytes(32);
        var settings = new GdprSettings { PseudonymizationKey = Convert.ToBase64String(key) };
        return new PseudonymizationService(Options.Create(settings));
    }

    // ── Encrypt / Decrypt ──────────────────────────────────────────────────

    [Fact]
    public void Encrypt_ThenDecrypt_ReturnOriginalPlaintext()
    {
        var sut       = CreateService();
        const string plaintext = "user@example.com";

        var ciphertext = sut.Encrypt(plaintext);
        var decrypted  = sut.Decrypt(ciphertext);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_SamePlaintext_ProducesDifferentCiphertexts()
    {
        var sut = CreateService();
        const string plaintext = "same-input";

        var cipher1 = sut.Encrypt(plaintext);
        var cipher2 = sut.Encrypt(plaintext);

        cipher1.Should().NotBe(cipher2, "each call uses a fresh random nonce");
    }

    [Fact]
    public void Decrypt_TamperedData_ThrowsCryptographicException()
    {
        var sut        = CreateService();
        var ciphertext = sut.Encrypt("secret");

        // Flip the last byte in the base64-decoded content
        var bytes  = Convert.FromBase64String(ciphertext);
        bytes[^1] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        var act = () => sut.Decrypt(tampered);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_WrongKey_ThrowsCryptographicException()
    {
        var key1 = RandomNumberGenerator.GetBytes(32);
        var key2 = RandomNumberGenerator.GetBytes(32);

        var sut1       = CreateService(key1);
        var sut2       = CreateService(key2);
        var ciphertext = sut1.Encrypt("secret");

        var act = () => sut2.Decrypt(ciphertext);
        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Decrypt_TooShortInput_ThrowsCryptographicException()
    {
        var sut     = CreateService();
        var tooShort = Convert.ToBase64String(new byte[10]); // < 12 + 16 bytes

        var act = () => sut.Decrypt(tooShort);
        act.Should().Throw<CryptographicException>();
    }

    // ── Anonymize ──────────────────────────────────────────────────────────

    [Fact]
    public void Anonymize_SameValue_ProducesConsistentHash()
    {
        var sut = CreateService();

        var hash1 = sut.Anonymize("user@example.com");
        var hash2 = sut.Anonymize("user@example.com");

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Anonymize_DifferentValues_ProduceDifferentHashes()
    {
        var sut = CreateService();

        var hash1 = sut.Anonymize("alice@example.com");
        var hash2 = sut.Anonymize("bob@example.com");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Anonymize_ReturnsLowercaseHexString_Of64Chars()
    {
        var sut  = CreateService();
        var hash = sut.Anonymize("test-value");

        hash.Should().MatchRegex("^[0-9a-f]{64}$",
            "SHA-256 produces 32 bytes = 64 lowercase hex characters");
    }

    // ── Constructor validation ────────────────────────────────────────────

    [Fact]
    public void Constructor_EmptyKey_ThrowsInvalidOperationException()
    {
        var settings = new GdprSettings { PseudonymizationKey = string.Empty };
        var act      = () => new PseudonymizationService(Options.Create(settings));

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*PseudonymizationKey*");
    }

    [Fact]
    public void Constructor_WrongKeyLength_ThrowsInvalidOperationException()
    {
        var settings = new GdprSettings
        {
            PseudonymizationKey = Convert.ToBase64String(new byte[16]), // 128-bit, not 256
        };
        var act = () => new PseudonymizationService(Options.Create(settings));

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*32 bytes*");
    }
}
