using Dentity.Sdk.Options.V1;

namespace Dentity.Services.Common.V1;

public static class ServiceOptionsExtensions
{
    public static string FormatUrl(this DentityOptions options) {
        return $"{(options.ServerUseTls ? "https" : "http")}://{options.ServerEndpoint}:{options.ServerPort}";
    }

    public static DentityOptions CloneWithAuthToken(this DentityOptions options, string authToken) {
        var cloned = options.Clone();
        cloned.AuthToken = authToken;
        return cloned;
    }
}