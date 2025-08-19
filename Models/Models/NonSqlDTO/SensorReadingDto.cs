using System;
using System.Collections.Generic;

namespace Web_health_app.Models.Models.NonSqlDTO
{
    public class SensorReadingInfoDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
        public MetadataDto Metadata { get; set; } = new MetadataDto();
        public List<ReadingDto> Readings { get; set; } = new List<ReadingDto>();
    }

    public class MetadataDto
    {
        public string UserId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string? SensorType { get; set; }
    }

    public class ReadingDto
    {
        public string Key { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class SensorReadingListDto
    {
        public List<SensorReadingInfoDto> SensorReadings { get; set; } = new List<SensorReadingInfoDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class SensorReadingSearchDto
    {
        public string? UserId { get; set; }
        public string? DeviceId { get; set; }
        public string? SensorType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SensorStatisticsDto
    {
        public Dictionary<string, long> SensorTypeCounts { get; set; } = new Dictionary<string, long>();
        public long TotalReadings { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
