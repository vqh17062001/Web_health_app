using Microsoft.EntityFrameworkCore;
using Web_health_app.ApiService.Entities;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public class ActionRepository : IActionRepository
    {
        private readonly HealthDbContext _context;

        public ActionRepository(HealthDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<(List<ActionInfoDto> Actions, int TotalCount)> GetAllActionsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.Actions.AsQueryable();

                // Filter by active status
                if (!includeInactive)
                {
                    query = query.Where(a => a.IsActive);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(a =>
                        a.ActionId.Contains(searchTerm) ||
                        a.ActionName.Contains(searchTerm) ||
                        a.Code.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();

                var actions = await query
                    .OrderBy(a => a.ActionId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ActionInfoDto
                    {
                        ActionId = a.ActionId,
                        ActionName = a.ActionName,
                        Code = a.Code,
                        IsActive = a.IsActive
                    })
                    .ToListAsync();

                return (actions, totalCount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving actions", ex);
            }
        }

        public async Task<ActionInfoDto?> GetActionByIdAsync(string actionId)
        {
            try
            {
                var action = await _context.Actions
                    .Where(a => a.ActionId == actionId)
                    .Select(a => new ActionInfoDto
                    {
                        ActionId = a.ActionId,
                        ActionName = a.ActionName,
                        Code = a.Code,
                        IsActive = a.IsActive
                    })
                    .FirstOrDefaultAsync();

                return action;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving action by ID", ex);
            }
        }

        public async Task<ActionInfoDto?> UpdateActionAsync(string actionId, UpdateActionDto updateActionDto)
        {
            try
            {
                var action = await _context.Actions.FirstOrDefaultAsync(a => a.ActionId == actionId);

                if (action == null)
                {
                    return null;
                }





                if (updateActionDto.IsActive.HasValue)
                {
                    action.IsActive = updateActionDto.IsActive.Value;
                }

                await _context.SaveChangesAsync();

                return new ActionInfoDto
                {
                    ActionId = action.ActionId,
                    ActionName = action.ActionName,
                    Code = action.Code,
                    IsActive = action.IsActive
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating action", ex);
            }
        }

        public async Task<List<ActionInfoDto>> GetActiveActionsAsync()
        {
            try
            {
                var actions = await _context.Actions
                    .Where(a => a.IsActive)
                    .Select(a => new ActionInfoDto
                    {
                        ActionId = a.ActionId,
                        ActionName = a.ActionName,
                        Code = a.Code,
                        IsActive = a.IsActive
                    })
                    .OrderBy(a => a.ActionName)
                    .ToListAsync();

                return actions;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active actions", ex);
            }
        }
    }
}
