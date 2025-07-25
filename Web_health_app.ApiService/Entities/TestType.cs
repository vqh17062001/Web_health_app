using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class TestType
{
    public string TesttypeId { get; set; } = null!;

    public string? Code { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Unit { get; set; }
}
