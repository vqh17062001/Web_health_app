using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class PhysiologicalMetric
{
    public string Id { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime MeasuredAt { get; set; }

    public string? MetricType { get; set; }

    public string? MetricValue { get; set; }

    public string? MetricUnit { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Student Student { get; set; } = null!;
}
