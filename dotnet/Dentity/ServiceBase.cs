using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Dentity.Sdk.Options.V1;
using Dentity.Services.Common.V1;
#if __BROWSER__
using System.Net.Http;
using Grpc.Net.Client.Web;
#endif

namespace Dentity;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public abstract class ServiceBase
{
    internal const string DefaultEcosystem = "";
    internal const bool DefaultServerUseTls = true;
    internal const int DefaultServerPort = 443;
    internal const string DefaultServerEndpoint = "prod.trinsic.cloud";

    protected internal ServiceBase() : this(new()) { }

    protected internal ServiceBase(DentityOptions options) {
        Options = options;
        EnsureOptionDefaults();
        Channel = CreateChannel(Options);
    }

    private void EnsureOptionDefaults() {
        if (string.IsNullOrWhiteSpace(Options.ServerEndpoint))
        {
            Options.ServerEndpoint = DefaultServerEndpoint;
        }

        if (Options.ServerPort == default)
        {
            Options.ServerPort = DefaultServerPort;
        }

        if (Options.ServerPort == DefaultServerPort)
        {
            Options.ServerUseTls = DefaultServerUseTls;
        }
    }

    private static GrpcChannel CreateChannel(DentityOptions options) {
#if __BROWSER__
        var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
        return GrpcChannel.ForAddress(options.FormatUrl(), new GrpcChannelOptions { HttpClient = httpClient });
#else
        return GrpcChannel.ForAddress(options.FormatUrl());
#endif
    }

    /// <summary>
    /// Gets the options set on this service.
    /// </summary>
    public DentityOptions Options { get; }

    /// <summary>
    /// Gets the gRPC channel used by this service. This channel can be reused
    /// by passing this instance to other service constructors.
    /// </summary>
    protected GrpcChannel Channel { get; }

    /// <summary>
    /// Create call metadata by setting authentication and version headers
    /// </summary>
    /// <param name="request">Protobuf request message to create headers for</param>
    /// <returns></returns>
    protected Task<Metadata> BuildMetadataAsync(IMessage? request = null) {
        var headers = new Metadata() {
            {"TrinsicSDKLanguage", "dotnet"},
            {"TrinsicSDKVersion", GetSdkVersion()}
        };

        // Build authentication header
        if (!string.IsNullOrWhiteSpace(Options.AuthToken))
        {
            headers.Add("Authorization", $"Bearer {Options.AuthToken}");
        }

        return Task.FromResult(headers);
    }

    /// <summary>
    /// Create call metadata by setting the required authentication and version headers for provided request
    /// </summary>
    /// <param name="request">Protobuf request message to create headers for</param>
    /// <returns></returns>
    protected Metadata BuildMetadata(IMessage? request = null) {
        var headers = new Metadata() {
            {"TrinsicSDKLanguage", "dotnet"},
            {"TrinsicSDKVersion", GetSdkVersion()}
        };

        // Build authentication header
        if (!string.IsNullOrWhiteSpace(Options.AuthToken))
        {
            headers.Add("Authorization", $"Bearer {Options.AuthToken}");
        }

        return headers;
    }

    /// <summary>
    /// Fetches the current version of the SDK
    /// </summary>
    /// <returns></returns>
    protected internal static string GetSdkVersion() {
        // This will always be 1.0.0 on local builds, since the version number is set on the github action during publish.
        return Assembly.GetAssembly(typeof(ServiceBase))?.GetName().Version?.ToString(3) ?? "unknown";
    }
}