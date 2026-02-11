using System;
using System.Threading;

namespace RazorPdf;

/// <summary>
/// Build context used by Razor components to emit PDF document models.
/// </summary>
public sealed class PdfBuildContext
{
    public PdfBuildContext(PdfRenderOptions? options = null)
    {
        Builder = new PdfDocumentBuilder();
        options?.ConfigureDocument?.Invoke(Builder);
    }

    public PdfDocumentBuilder Builder { get; }

    public PdfDocumentModel Build()
    {
        return Builder.Build();
    }
}

public sealed class PdfBuildContextAccessor
{
    private readonly AsyncLocal<PdfBuildContext?> _currentContext = new();

    public PdfBuildContext? Current => _currentContext.Value;

    public PdfBuildContext GetRequiredContext()
    {
        return _currentContext.Value ?? throw new InvalidOperationException("No active PdfBuildContext is available.");
    }

    public IDisposable PushContext(PdfBuildContext context)
    {
        var prior = _currentContext.Value;
        _currentContext.Value = context;
        return new ContextScope(() => _currentContext.Value = prior);
    }

    private sealed class ContextScope : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;

        public ContextScope(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _onDispose();
        }
    }
}
