using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Environment
{
    public string Id { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public DateTime TimeStamp { get; set; }

    public string? TemperatureC { get; set; }

    public string? HumidityRh { get; set; }

    public string? UvIndexMwCm2 { get; set; }

    public virtual Student Student { get; set; } = null!;
}
