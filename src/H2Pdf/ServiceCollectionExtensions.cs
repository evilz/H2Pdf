using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace H2Pdf;

/// <summary>
/// Extension methods for configuring H2Pdf services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds H2Pdf services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddRazorPdf(this IServiceCollection services) => AddH2Pdf(services);

    /// <summary>
    /// Adds H2Pdf services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddH2Pdf(this IServiceCollection services)
    {
        // Ensure ILoggerFactory is available (use NullLoggerFactory if not already registered)
        services.TryAddSingleton<ILoggerFactory, NullLoggerFactory>();
        
        // Add PdfRenderer as transient to avoid misleading singleton semantics
        // HtmlRenderer creates new instances per call which is thread-safe
        services.TryAddTransient<PdfRenderer>();
        services.TryAddTransient<HtmlPdfRenderer>();
        services.TryAddSingleton<PdfBuildContextAccessor>();

        return services;
    }
}
