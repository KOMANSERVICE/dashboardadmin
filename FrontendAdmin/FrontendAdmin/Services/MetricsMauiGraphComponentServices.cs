using FrontendAdmin.Components.Graphs;
using FrontendAdmin.Shared.Pages.Swarm.Models;
using FrontendAdmin.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace FrontendAdmin.Services;

public class CpuMauiGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(CpuMauiGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class MemoryMauiGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(MemoryMauiGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class HealthStatusMauiGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(HealthStatusMauiGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class RestartCountMauiGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(RestartCountMauiGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class UptimeMauiGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(UptimeMauiGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}
