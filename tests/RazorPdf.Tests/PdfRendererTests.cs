using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MigraDocCore.DocumentObjectModel;

namespace RazorPdf.Tests;

public class PdfRendererTests
{
    [Fact]
    public async Task RenderToPdfAsync_ComponentRendersToParagraph()
    {
        var serviceProvider = new TestServiceProvider();
        var renderer = new PdfRenderer(serviceProvider);

        var parameters = new Dictionary<string, object?>
        {
            { "Message", "Hello from VDOM" }
        };

        var document = await renderer.RenderToPdfAsync<SimpleMessageComponent>(parameters);

        var section = document.Sections[0];
        Assert.True(section.Elements.Count >= 1);
        var paragraph = Assert.IsType<Paragraph>(section.Elements[0]);
        Assert.Contains("Hello from VDOM", GetParagraphText(paragraph));
    }

    private sealed class SimpleMessageComponent : ComponentBase
    {
        [Parameter]
        public string? Message { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "p");
            builder.AddContent(1, Message);
            builder.CloseElement();
        }
    }

    private static string GetParagraphText(Paragraph paragraph)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var element in paragraph.Elements)
        {
            if (element is Text text)
            {
                sb.Append(text.Content);
            }
            else if (element is FormattedText formattedText)
            {
                foreach (var formattedElement in formattedText.Elements)
                {
                    if (formattedElement is Text formattedTextNode)
                    {
                        sb.Append(formattedTextNode.Content);
                    }
                }
            }
        }

        return sb.ToString();
    }

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;
        private readonly IComponentActivator _componentActivator = new TestComponentActivator();

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(ILoggerFactory))
            {
                return _loggerFactory;
            }

            if (serviceType == typeof(IComponentActivator))
            {
                return _componentActivator;
            }

            return null;
        }
    }

    private sealed class TestComponentActivator : IComponentActivator
    {
        public IComponent CreateInstance(Type componentType)
        {
            return (IComponent)Activator.CreateInstance(componentType)!;
        }
    }
}
