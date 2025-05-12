using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Dentity;
using Dentity.Sdk.Options.V1;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to add Dentity Service dependencies
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Dentity Service dependencies and optionally configure the service options
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddDentity(this IServiceCollection serviceCollection, Action<DentityOptions>? configureOptions = null) {
        if (configureOptions is not null)
        {
            serviceCollection.Configure(configureOptions);
        }

        return serviceCollection
            .AddSingleton(provider => new DentityService(provider.GetService<IOptions<DentityOptions>>()?.Value ?? new DentityOptions()))
            .AddSingleton<IDentityService>(provider => new DentityService(provider.GetService<IOptions<DentityOptions>>()?.Value ?? new DentityOptions()));
    }
}