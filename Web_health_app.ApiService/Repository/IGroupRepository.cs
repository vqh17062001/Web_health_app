using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Repository
{
    public interface IGroupRepository
    {
        /// <summary>
        /// Get all groups with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="searchTerm">Optional search term for filtering</param>
        /// <param name="includeInactive">Include inactive groups in results</param>
        /// <returns>Paginated list of groups</returns>
        Task<(List<GroupInfoDto> Groups, int TotalCount)> GetAllGroupsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);

        /// <summary>
        /// Get group by ID
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>Group information or null if not found</returns>
        Task<GroupInfoDto?> GetGroupByIdAsync(string groupId);

        /// <summary>
        /// Get group with detailed information (users and roles)
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>Group detailed information or null if not found</returns>
        Task<GroupDetailDto?> GetGroupDetailAsync(string groupId);

        /// <summary>
        /// Get all active groups (for dropdown lists)
        /// </summary>
        /// <returns>List of active groups</returns>
        Task<List<GroupInfoDto>> GetActiveGroupsAsync();

        /// <summary>
        /// Create new group
        /// </summary>
        /// <param name="createGroupDto">Group creation data</param>
        /// <returns>Created group information</returns>
        Task<GroupInfoDto> CreateGroupAsync(CreateGroupDto createGroupDto);

        /// <summary>
        /// Update group information
        /// </summary>
        /// <param name="groupId">Group ID to update</param>
        /// <param name="updateGroupDto">Updated group data</param>
        /// <returns>Updated group information or null if not found</returns>
        Task<GroupInfoDto?> UpdateGroupAsync(string groupId, UpdateGroupDto updateGroupDto);

        /// <summary>
        /// Delete group (soft delete by setting IsActive to false)
        /// </summary>
        /// <param name="groupId">Group ID to delete</param>
        /// <returns>True if group deleted successfully, false if not found</returns>
        Task<bool> DeleteGroupAsync(string groupId);

        /// <summary>
        /// Permanently delete group
        /// </summary>
        /// <param name="groupId">Group ID to delete permanently</param>
        /// <returns>True if group deleted successfully, false if not found</returns>
        Task<bool> HardDeleteGroupAsync(string groupId);

        /// <summary>
        /// Check if group ID exists
        /// </summary>
        /// <param name="groupId">Group ID to check</param>
        /// <param name="excludeGroupId">Group ID to exclude from check (for updates)</param>
        /// <returns>True if group ID exists, false otherwise</returns>
        Task<bool> GroupIdExistsAsync(string groupName, string? excludeGroupId = null);

        /// <summary>
        /// Get groups with user count
        /// </summary>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="searchTerm">Search term</param>
        /// <param name="includeInactive">Include inactive groups</param>
        /// <returns>Groups with user count information</returns>
        Task<(List<GroupWithUserCountDto> Groups, int TotalCount)> GetGroupsWithUserCountAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool includeInactive = false);

        /// <summary>
        /// Add users to group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="userIds">List of user IDs to add</param>
        /// <returns>True if assignment successful</returns>
        Task<bool> AddUsersToGroupAsync(string groupId, List<Guid> userIds);

        /// <summary>
        /// Remove users from group
        /// </summary>
        /// <param name="userIds">List of user IDs to remove from group</param>
        /// <returns>True if removal successful</returns>
        Task<bool> RemoveUsersFromGroupAsync(List<Guid> userIds);

        /// <summary>
        /// Remove specific users from a specific group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="userIds">List of user IDs to remove from the group</param>
        /// <returns>True if removal successful</returns>
        Task<bool> RemoveUsersFromSpecificGroupAsync(string groupId, List<Guid> userIds);

        /// <summary>
        /// Get all users in a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>List of user information in the group</returns>
        Task<List<UserInfoDto>> GetGroupUsersAsync(string groupId);

        /// <summary>
        /// Get all groups that a user belongs to
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of group information the user belongs to</returns>
        Task<List<GroupInfoDto>> GetUserGroupsAsync(Guid userId);

        /// <summary>
        /// Move user to different group
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newGroupId">New group ID (null to remove from all groups)</param>
        /// <returns>True if move successful</returns>
        Task<bool> MoveUserToGroupAsync(Guid userId, string? newGroupId);

        /// <summary>
        /// Add roles to group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="roleIds">List of role IDs to add</param>
        /// <param name="note">Optional note for the assignment</param>
        /// <returns>True if assignment successful</returns>
        Task<bool> AddRolesToGroupAsync(string groupId, List<string> roleIds, string? note = null);

        /// <summary>
        /// Remove roles from group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="roleIds">List of role IDs to remove</param>
        /// <returns>True if removal successful</returns>
        Task<bool> RemoveRolesFromGroupAsync(string groupId, List<string> roleIds);

        /// <summary>
        /// Get all roles assigned to a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>List of group role assignments</returns>
        Task<List<GroupRoleDto>> GetGroupRolesAsync(string groupId);

        /// <summary>
        /// Get all groups that have a specific role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of group role assignments</returns>
        Task<List<GroupRoleDto>> GetRoleGroupsAsync(string roleId);

        /// <summary>
        /// Replace all roles for a group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <param name="roleIds">New list of role IDs</param>
        /// <param name="note">Optional note for the assignment</param>
        /// <returns>True if replacement successful</returns>
        Task<bool> ReplaceGroupRolesAsync(string groupId, List<string> roleIds, string? note = null);

        /// <summary>
        /// Remove all roles from group
        /// </summary>
        /// <param name="groupId">Group ID</param>
        /// <returns>True if removal successful</returns>
        Task<bool> RemoveAllGroupRolesAsync(string groupId);

        /// <summary>
        /// Get groups by time active
        /// </summary>
        /// <param name="timeActiveId">Time Active ID</param>
        /// <returns>List of groups with the specified time active</returns>
        Task<List<GroupInfoDto>> GetGroupsByTimeActiveAsync(string timeActiveId);

        /// <summary>
        /// Bulk operation on groups
        /// </summary>
        /// <param name="bulkOperationDto">Bulk operation data</param>
        /// <returns>Number of affected groups</returns>
        Task<int> BulkGroupOperationAsync(BulkGroupOperationDto bulkOperationDto);

        /// <summary>
        /// Search groups with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <param name="pageNumber">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Filtered groups with pagination</returns>
        Task<(List<GroupInfoDto> Groups, int TotalCount)> SearchGroupsAsync(GroupSearchDto searchDto, int pageNumber = 1, int pageSize = 10);
    }
}
