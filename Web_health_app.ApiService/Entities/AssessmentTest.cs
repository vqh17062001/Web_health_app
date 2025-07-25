using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class AssessmentTest
{
    public string? TesttypeId { get; set; }

    public string? AbsId { get; set; }

    public string? Code { get; set; }

    public string? Unit { get; set; }

    public string? ResultValue { get; set; }

    public DateTime? RecordedAt { get; set; }

    public Guid? RecordedBy { get; set; }

    public virtual AssessmentBatchStudent? Abs { get; set; }

    public virtual TestType? Testtype { get; set; }
}
