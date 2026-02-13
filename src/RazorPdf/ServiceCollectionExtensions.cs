using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
