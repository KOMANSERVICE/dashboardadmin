using FrontendAdmin.Shared.Pages.Swarm.Models;
using FrontendAdmin.Shared.Services;
using FrontendAdmin.Web.Client.Components.Graphs;
using Microsoft.AspNetCore.Components;

namespace FrontendAdmin.Web.Client.Services;

public class CpuGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(CpuGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class MemoryGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(MemoryGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class HealthStatusGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(HealthStatusGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class RestartCountGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(RestartCountGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}

public class UptimeGraphComponentService : IGraphComponent<ContainerMetricsSummaryDto>
{
    public RenderFragment Render(IEnumerable<ContainerMetricsSummaryDto> items) => builder =>
    {
        builder.OpenComponent(0, typeof(UptimeGraphComponent));
        builder.AddAttribute(1, "Stats", items);
        builder.CloseComponent();
    };
}
