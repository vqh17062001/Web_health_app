using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Web_health_app.Models.Models
{
    /// <summary>
    /// DTO for assessment test information display
    /// </summary>
    public class AssessmentTestInfoDto
    {
        public string? TestTypeId { get; set; }

        public string? AbsId { get; set; }

        public string? Code { get; set; }

        public string? Unit { get; set; }

        public string? ResultValue { get; set; }

        public DateTime? RecordedAt { get; set; }

        public Guid? RecordedBy { get; set; }

        public string? RecordedByName { get; set; }

        // Additional info from related entities
        public string? TestTypeName { get; set; }

        public string? StudentId { get; set; }

        public string? StudentName { get; set; }

        public string? AssessmentBatchId { get; set; }

        public string Result =>
        TestResultHelper.ResultToString(Code ?? string.Empty, ResultValue);
    }

    /// <summary>
    /// DTO for creating new assessment test result
    /// </summary>
    public class CreateAssessmentTestDto
    {
        [Required(ErrorMessage = "Test type ID is required")]
        public string TestTypeId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Assessment batch student ID is required")]
        public string AbsId { get; set; } = string.Empty;

        public string? Code { get; set; }

        [Required(ErrorMessage = "Unit is required")]
        public string Unit { get; set; }

        [Required(ErrorMessage = "Result value is required")]
        public string ResultValue { get; set; } = string.Empty;

        public DateTime? RecordedAt { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Recorded by is required")]
        public Guid RecordedBy { get; set; }
    }

    /// <summary>
    /// DTO for updating assessment test result
    /// </summary>
    public class UpdateAssessmentTestDto
    {
        public string? Code { get; set; }

        public string? Unit { get; set; }

        public string? ResultValue { get; set; }

        public DateTime? RecordedAt { get; set; }

        public Guid? RecordedBy { get; set; }
    }

    /// <summary>
    /// DTO for assessment test search and filter
    /// </summary>
    public class AssessmentTestSearchDto
    {
        public string? TestTypeId { get; set; }

        public string? AbsId { get; set; }

        public string? AssessmentBatchId { get; set; }

        public string? StudentId { get; set; }

        public string? SearchTerm { get; set; }

        public DateTime? RecordedFrom { get; set; }

        public DateTime? RecordedTo { get; set; }

        public Guid? RecordedBy { get; set; }

        public string? SortBy { get; set; } = "RecordedAt";

        public string? SortDirection { get; set; } = "desc";

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// DTO for paginated assessment tests response
    /// </summary>
    public class AssessmentTestsApiResponse
    {
        public List<AssessmentTestInfoDto> AssessmentTests { get; set; } = new List<AssessmentTestInfoDto>();
        public AssessmentTestsPaginationInfo Pagination { get; set; } = new AssessmentTestsPaginationInfo();
    }

    /// <summary>
    /// Pagination information for assessment tests
    /// </summary>
    public class AssessmentTestsPaginationInfo
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO for bulk assessment test operations
    /// </summary>
    public class BulkAssessmentTestOperationDto
    {
        [Required(ErrorMessage = "Assessment test IDs are required")]
        public List<string> TestIds { get; set; } = new List<string>();

        public string? Operation { get; set; } // "delete", "update_recorder", etc.

        public string? Reason { get; set; }

        public Guid? NewRecordedBy { get; set; }
    }

    /// <summary>
    /// Trả về xếp loại cho từng bài test: "Giỏi", "Khá", "Đạt" hoặc "Chưa đạt".
    /// </summary>
    public static class TestResultHelper
    {
        /// <param name="Excellent">Chuẩn “Giỏi”</param>
        /// <param name="Good">Chuẩn “Khá”</param>
        /// <param name="Pass">Chuẩn “Đạt”</param>
        /// <param name="LowerIsBetter">
        ///     true  ➜ càng nhỏ càng tốt (thời gian)  
        ///     false ➜ càng lớn càng tốt (reps, mét…)
        /// </param>
        private record Threshold(double Excellent, double Good, double Pass, bool LowerIsBetter);

        /// <summary>Chuẩn điểm cho từng bài test – LẤY TỪ BẢNG GIẤY.</summary>
        private static readonly Dictionary<string, Threshold> _thresholds = new()
        {
            // ────── NHÓM SỨC NHANH ──────
            ["RUN_100M"] = new(13.3, 13.6, 14.0, true),
            ["RUN_50M_X2"] = new(16.3, 16.6, 17.0, true),

            // ────── NHÓM SỨC MẠNH ──────
            ["PULL_UPS"] = new(23, 19, 15, false),
            ["DIPS"] = new(23, 20, 17, false),
            ["LIFT_25KG"] = new(32, 28, 24, false),
            ["LONG_JUMP"] = new(5.00, 4.70, 4.40, false),
            ["TRIPLE_JUMP_3STEP"] = new(7.70, 7.30, 6.90, false),

            // ────── NHÓM SỨC BỀN ──────
            ["RUN_3000M_GEAR"] = new(ToSec("12:30"), ToSec("13:10"), ToSec("13:50"), true),
            ["RUN_3000M"] = new(ToSec("11:30"), ToSec("12:10"), ToSec("12:50"), true),

            // ────── BÀI TẬP TỔNG HỢP ──────
            ["OBSTACLE_100M_100M"] = new(ToSec("1:15"), ToSec("1:20"), ToSec("1:25"), true),
            ["OBSTACLE_K91"] = new(ToSec("0:53"), ToSec("0:58"), ToSec("1:03"), true),

            // ────── BƠI ──────
            ["SWIM_3MIN_FREESTYLE"] = new(100, 80, 50, false)
        };

        /// <summary>
        /// Tính xếp loại cho một kết quả.
        /// </summary>
        /// <param name="code">Mã bài test (ví dụ: RUN_100M)</param>
        /// <param name="result">
        ///     Kết quả đo –  
        ///     *Thời gian* dùng định dạng “ss”, “m:ss” hoặc “mm:ss”.  
        ///     *Khoảng cách / số lần* truyền số thuần.
        /// </param>
        /// <returns>"Giỏi", "Khá", "Đạt" hoặc "Chưa đạt/Không xác định"</returns>
        public static string ResultToString(string code, string? result)
        {
            if (string.IsNullOrWhiteSpace(result) ||
                !_thresholds.TryGetValue(code, out var t) ||
                !TryParseNumeric(result.Trim(), out var value))
            {
                return "Không xác định";
            }

            // ĐÁNH GIÁ
            if (t.LowerIsBetter)
            {
                if (value <= t.Excellent) return "Giỏi";
                if (value <= t.Good) return "Khá";
                if (value <= t.Pass) return "Đạt";
            }
            else
            {
                if (value >= t.Excellent) return "Giỏi";
                if (value >= t.Good) return "Khá";
                if (value >= t.Pass) return "Đạt";
            }

            return "Chưa đạt";
        }

        // ──────────── Helpers ────────────
        private static bool TryParseNumeric(string input, out double value)
        {
            // Định dạng thời gian mm:ss
            if (input.Contains(':'))
            {
                value = ToSec(input);
                return true;
            }
            // Số thường (reps, mét, giây)
            return double.TryParse(
                input, NumberStyles.Any,
                CultureInfo.InvariantCulture, out value);
        }

        private static double ToSec(string time)
        {
            // Chuyển “m:ss” → giây dưới dạng double
            var parts = time.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var m) &&
                int.TryParse(parts[1], out var s))
            {
                return m * 60 + s;
            }
            // Nếu không khớp, mặc định 0
            return 0;
        }
    }
}
