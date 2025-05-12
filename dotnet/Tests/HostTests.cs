using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Dentity;
using Xunit;

namespace Tests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class HostTests
{
    [Fact(DisplayName = "Test default service host")]
    public async Task TestGenericHost() {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services => {
                services.AddDentity();
            }).Build();

        await host.StartAsync();

        var dentity = host.Services.GetRequiredService<DentityService>();
        var providerService = dentity.Provider;
        var accountService = dentity.Wallet;

        providerService.Should().NotBeNull();
        accountService.Should().NotBeNull();

        accountService.Options.ServerEndpoint.Should().Be(ServiceBase.DefaultServerEndpoint);
        accountService.Options.ServerPort.Should().Be(ServiceBase.DefaultServerPort);
        accountService.Options.ServerUseTls.Should().Be(ServiceBase.DefaultServerUseTls);
        accountService.Options.AuthToken.Should().Be(string.Empty);

        await host.StopAsync();
    }

    [Fact(DisplayName = "Test configured service host")]
    public async Task TestConfiguredGenericHost() {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services => {
                services.AddDentity(options => {
                    options.AuthToken = "auth";
                    options.ServerEndpoint = "example.com";
                    options.ServerPort = 42;
                    options.ServerUseTls = true;
                });
            }).Build();

        await host.StartAsync();

        var dentity = host.Services.GetRequiredService<DentityService>();
        var providerService = dentity.Provider;
        var walletService = dentity.Wallet;

        providerService.Should().NotBeNull();
        walletService.Should().NotBeNull();

        walletService.Options.ServerEndpoint.Should().Be("example.com");
        walletService.Options.ServerPort.Should().Be(42);
        walletService.Options.ServerUseTls.Should().BeTrue();
        walletService.Options.AuthToken.Should().Be("auth");

        await host.StopAsync();
    }

    [Fact(DisplayName = "Test disabled token persistence")]
    public async Task TestDisableTokenPersistence() {
        var host = Host
            .CreateDefaultBuilder()
            .ConfigureServices(services => {
                services.AddDentity(options => {

                });
            }).Build();

        await host.StartAsync();

        await host.StopAsync();
    }
}