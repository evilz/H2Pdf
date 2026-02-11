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
    private readonly AsyncLocal<PdfBuildContext?> _current = new();

    public PdfBuildContext? Current => _current.Value;

    public PdfBuildContext GetRequiredContext()
    {
        return _current.Value ?? throw new InvalidOperationException("No active PdfBuildContext is available.");
    }

    public IDisposable PushContext(PdfBuildContext context)
    {
        var prior = _current.Value;
        _current.Value = context;
        return new ContextScope(() => _current.Value = prior);
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
