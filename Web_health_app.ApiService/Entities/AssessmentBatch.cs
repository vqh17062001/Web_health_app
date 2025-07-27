using System;
using System.Collections.Generic;

namespace Web_health_app.ApiService.Entities;

public partial class AssessmentBatch
{
    public string BatchId { get; set; } = null!;

    public string? CodeName { get; set; }

    public string? Description { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public short Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ManagerBy { get; set; }

    public virtual ICollection<AssessmentBatchStudent> AssessmentBatchStudents { get; set; } = new List<AssessmentBatchStudent>();

    public string GetAssessmentBatchStatusString()
    {
        return Status switch
        {
            0 => "Khởi tạo chưa được phê duyệt",
            1 => "Đã được phê duyệt",
            2 => "Đang tiến hành kiểm tra",
            3 => "Hoàn tất kiểm tra",
            -1 => "Lỗi phải tổ chức lại",
            -2 => "Đã xóa",
            _ => "Không xác định"
        };
    }

}
