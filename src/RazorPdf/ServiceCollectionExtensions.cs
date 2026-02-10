using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RazorPdf;

/// <summary>
/// Extension methods for configuring RazorPdf services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds RazorPdf services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddRazorPdf(this IServiceCollection services)
    {
        // Add PdfRenderer as a singleton
        services.TryAddSingleton<PdfRenderer>();

        return services;
    }
}
