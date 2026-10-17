using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.Gdpr.Infrastructure.Extensions;
using StangaNetLib.Gdpr.Pseudonymization;
using StangaNetLib.Gdpr.Pseudonymization.Abstractions;

namespace StangaNetLib.Gdpr.Tests.Extensions;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddStangaNetLibGdpr_RegistersPseudonymizationService()
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GdprSettings:PseudonymizationKey"] = key,
                ["GdprSettings:DataControllerName"] = "Test Controller",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddStangaNetLibGdpr(configuration);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IPseudonymizationService>().Should().BeOfType<PseudonymizationService>();
    }
}
