using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace RazorPdf;

internal sealed class PdfComponentRenderer : Renderer
{
    private readonly ILogger<PdfComponentRenderer>? _logger;

    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

    public PdfComponentRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        : base(serviceProvider, loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PdfComponentRenderer>();
    }

    public Task<IReadOnlyList<PdfVdomNode>> RenderToVdomAsync<TComponent>(ParameterView parameters)
        where TComponent : IComponent
    {
        return Dispatcher.InvokeAsync(async () =>
        {
            var component = (IComponent)InstantiateComponent(typeof(TComponent));
            var componentId = AssignRootComponentId(component);

            try
            {
                await RenderRootComponentAsync(componentId, parameters);
                return BuildVdom(componentId);
            }
            finally
            {
                RemoveRootComponent(componentId);
            }
        });
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        return Task.CompletedTask;
    }

    protected override void HandleException(Exception exception)
    {
        _logger?.LogError(exception, "Unhandled exception while rendering RazorPdf components.");
    }

    private IReadOnlyList<PdfVdomNode> BuildVdom(int componentId)
    {
        var frames = GetCurrentRenderTreeFrames(componentId);
        var nodes = new List<PdfVdomNode>();

        BuildNodes(frames.Array, 0, frames.Count, nodes);

        return nodes;
    }

    private void BuildNodes(RenderTreeFrame[] frames, int startIndex, int endIndex, List<PdfVdomNode> nodes)
    {
        var index = startIndex;
        while (index < endIndex)
        {
            var frame = frames[index];
            switch (frame.FrameType)
            {
                case RenderTreeFrameType.Text:
                    if (!string.IsNullOrEmpty(frame.TextContent))
                    {
                        nodes.Add(new PdfVdomText(frame.TextContent));
                    }
                    index++;
                    break;

                case RenderTreeFrameType.Markup:
                    if (!string.IsNullOrEmpty(frame.MarkupContent))
                    {
                        nodes.Add(new PdfVdomText(frame.MarkupContent));
                    }
                    index++;
                    break;

                case RenderTreeFrameType.Element:
                {
                    var elementEndIndex = index + frame.ElementSubtreeLength;
                    var attributes = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                    var childIndex = index + 1;

                    while (childIndex < elementEndIndex && frames[childIndex].FrameType == RenderTreeFrameType.Attribute)
                    {
                        TryAddAttribute(frames[childIndex], attributes);
                        childIndex++;
                    }

                    var children = new List<PdfVdomNode>();
                    BuildNodes(frames, childIndex, elementEndIndex, children);
                    nodes.Add(new PdfVdomElement(frame.ElementName, attributes, children));
                    index = elementEndIndex;
                    break;
                }

                case RenderTreeFrameType.Component:
                {
                    var componentEndIndex = index + frame.ComponentSubtreeLength;
                    var childNodes = BuildVdom(frame.ComponentId);
                    nodes.AddRange(childNodes);
                    index = componentEndIndex;
                    break;
                }

                case RenderTreeFrameType.Region:
                {
                    var regionEndIndex = index + frame.RegionSubtreeLength;
                    BuildNodes(frames, index + 1, regionEndIndex, nodes);
                    index = regionEndIndex;
                    break;
                }

                default:
                    index++;
                    break;
            }
        }
    }

    private static void TryAddAttribute(RenderTreeFrame frame, IDictionary<string, object?> attributes)
    {
        if (frame.AttributeValue is null)
        {
            return;
        }

        if (frame.AttributeValue is MulticastDelegate || IsEventCallback(frame.AttributeValue))
        {
            return;
        }

        if (frame.AttributeValue is bool boolValue)
        {
            if (boolValue)
            {
                attributes[frame.AttributeName] = string.Empty;
            }
            return;
        }

        attributes[frame.AttributeName] = frame.AttributeValue;
    }

    private static bool IsEventCallback(object attributeValue)
    {
        if (attributeValue is EventCallback)
        {
            return true;
        }

        var type = attributeValue.GetType();
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>);
    }
}
