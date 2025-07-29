using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IActionRepository
    {
        Task<(List<ActionInfoDto> Actions, int TotalCount)> GetAllActionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);
        Task<ActionInfoDto?> GetActionByIdAsync(string actionId);
        Task<ActionInfoDto?> UpdateActionAsync(string actionId, UpdateActionDto updateActionDto);
        Task<List<ActionInfoDto>> GetActiveActionsAsync();
    }
}
