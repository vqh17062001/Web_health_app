using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class Department
{
    public string DepartmentCode { get; set; } = null!;

    public string? Battalion { get; set; }

    public string? Course { get; set; }

    public string? CharacterCode { get; set; }
}
