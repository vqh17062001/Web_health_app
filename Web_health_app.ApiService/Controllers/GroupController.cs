using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Web_health_app.ApiService.Repository;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _groupRepository;

        public GroupController(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
        }

        /// <summary>
        /// Get all groups with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive groups</param>
        /// <returns>Paginated list of groups</returns>
        [HttpGet]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult> GetAllGroups(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (groups, totalCount) = await _groupRepository.GetAllGroupsAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var response = new
                {
                    groups = groups,
                    pagination = new
                    {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasNextPage = pageNumber * pageSize < totalCount,
                        hasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving groups", error = ex.Message });
            }
        }

        /// <summary>
        /// Get group by ID
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Group information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult<GroupInfoDto>> GetGroupById(string id)
        {
            try
            {
                var group = await _groupRepository.GetGroupByIdAsync(id);
                if (group == null)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(group);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the group", error = ex.Message });
            }
        }

        /// <summary>
        /// Get group with detailed information (users and roles)
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Group detailed information</returns>
        [HttpGet("{id}/detail")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult<GroupDetailDto>> GetGroupDetail(string id)
        {
            try
            {
                var groupDetail = await _groupRepository.GetGroupDetailAsync(id);
                if (groupDetail == null)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(groupDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving group details", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active groups (for dropdown lists)
        /// </summary>
        /// <returns>List of active groups</returns>
        [HttpGet("active")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult<List<GroupInfoDto>>> GetActiveGroups()
        {
            try
            {
                var groups = await _groupRepository.GetActiveGroupsAsync();
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active groups", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new group
        /// </summary>
        /// <param name="createGroupDto">Group creation data</param>
        /// <returns>Created group information</returns>
        [HttpPost]
        [Authorize(Roles = "CREATE.GROUPS")]
        public async Task<ActionResult<GroupInfoDto>> CreateGroup([FromBody] CreateGroupDto createGroupDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if group ID already exists
                if (await _groupRepository.GroupIdExistsAsync(createGroupDto.GroupName))
                {
                    return BadRequest(new { message = "Group ID already exists" });
                }

                var createdGroup = await _groupRepository.CreateGroupAsync(createGroupDto);
                return CreatedAtAction(nameof(GetGroupById), new { id = createdGroup.GroupId }, createdGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the group", error = ex.Message });
            }
        }

        /// <summary>
        /// Update group information
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="updateGroupDto">Updated group data</param>
        /// <returns>Updated group information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "UPDATE.GROUPS")]
        public async Task<ActionResult<GroupInfoDto>> UpdateGroup(string id, [FromBody] UpdateGroupDto updateGroupDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedGroup = await _groupRepository.UpdateGroupAsync(id, updateGroupDto);
                if (updatedGroup == null)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(updatedGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the group", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete group (soft delete)
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE.GROUPS")]
        public async Task<ActionResult> DeleteGroup(string id)
        {
            try
            {
                var result = await _groupRepository.DeleteGroupAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(new { message = "Group deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the group", error = ex.Message });
            }
        }

        /// <summary>
        /// Permanently delete group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "DELETE.GROUPS")]
        public async Task<ActionResult> HardDeleteGroup(string id)
        {
            try
            {
                var result = await _groupRepository.HardDeleteGroupAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(new { message = "Group permanently deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while permanently deleting the group", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if group ID exists
        /// </summary>
        /// <param name="groupId">Group ID to check</param>
        /// <returns>Boolean indicating if group ID exists</returns>
        [HttpGet("check-group-id/{groupId}")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult> CheckGroupId(string groupId)
        {
            try
            {
                var exists = await _groupRepository.GroupIdExistsAsync(groupId);
                return Ok(new { exists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking group ID", error = ex.Message });
            }
        }

        /// <summary>
        /// Get groups with user count
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive groups</param>
        /// <returns>Paginated list of groups with user count</returns>
        [HttpGet("with-user-count")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult> GetGroupsWithUserCount(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (groups, totalCount) = await _groupRepository.GetGroupsWithUserCountAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var response = new
                {
                    groups = groups,
                    pagination = new
                    {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasNextPage = pageNumber * pageSize < totalCount,
                        hasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving groups with user count", error = ex.Message });
            }
        }

        /// <summary>
        /// Add users to group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="groupUserAssignmentDto">Group user assignment data</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/users")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.USERS")]
        public async Task<ActionResult> AddUsersToGroup(string id, [FromBody] GroupUserAssignmentDto groupUserAssignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != groupUserAssignmentDto.GroupId)
                {
                    return BadRequest(new { message = "Group ID in URL and body must match" });
                }

                var result = await _groupRepository.AddUsersToGroupAsync(id, groupUserAssignmentDto.UserIds);
                if (!result)
                {
                    return NotFound(new { message = "Group not found or some users not found" });
                }

                return Ok(new { message = "Users added to group successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding users to group", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove users from group
        /// </summary>
        /// <param name="userIds">List of user IDs to remove from their groups</param>
        /// <returns>Success message</returns>
        [HttpDelete("users")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.USERS")]
        public async Task<ActionResult> RemoveUsersFromGroup([FromBody] List<Guid> userIds)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return BadRequest(new { message = "User IDs are required" });
                }

                var result = await _groupRepository.RemoveUsersFromGroupAsync(userIds);
                if (!result)
                {
                    return BadRequest(new { message = "Failed to remove users from groups" });
                }

                return Ok(new { message = "Users removed from groups successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing users from groups", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove specific users from a specific group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="userIds">List of user IDs to remove from the group</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/users")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.USERS")]
        public async Task<ActionResult> RemoveUsersFromSpecificGroup(string id, [FromBody] List<Guid> userIds)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                {
                    return BadRequest(new { message = "User IDs are required" });
                }

                var result = await _groupRepository.RemoveUsersFromSpecificGroupAsync(id, userIds);
                if (!result)
                {
                    return NotFound(new { message = "Group not found or users not found in group" });
                }

                return Ok(new { message = "Users removed from group successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing users from group", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all users in a group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>List of users in the group</returns>
        [HttpGet("{id}/users")]
        [Authorize(Roles = "READ.GROUPS,READ.USERS")]
        public async Task<ActionResult<List<UserInfoDto>>> GetGroupUsers(string id)
        {
            try
            {
                var users = await _groupRepository.GetGroupUsersAsync(id);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving group users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all groups that a user belongs to
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of groups the user belongs to</returns>
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "READ.GROUPS,READ.USERS")]
        public async Task<ActionResult<List<GroupInfoDto>>> GetUserGroups(Guid userId)
        {
            try
            {
                var groups = await _groupRepository.GetUserGroupsAsync(userId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user groups", error = ex.Message });
            }
        }

        /// <summary>
        /// Move user to different group
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newGroupId">New group ID (null to remove from all groups)</param>
        /// <returns>Success message</returns>
        [HttpPut("user/{userId}/move")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.USERS")]
        public async Task<ActionResult> MoveUserToGroup(Guid userId, [FromBody] string? newGroupId)
        {
            try
            {
                var result = await _groupRepository.MoveUserToGroupAsync(userId, newGroupId);
                if (!result)
                {
                    return NotFound(new { message = "User not found or new group not found" });
                }

                var message = newGroupId == null
                    ? "User removed from all groups successfully"
                    : "User moved to new group successfully";

                return Ok(new { message = message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while moving user to group", error = ex.Message });
            }
        }

        /// <summary>
        /// Add roles to group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="groupRoleAssignmentDto">Group role assignment data</param>
        /// <returns>Success message</returns>
        [HttpPost("{id}/roles")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.ROLES")]
        public async Task<ActionResult> AddRolesToGroup(string id, [FromBody] GroupRoleAssignmentDto groupRoleAssignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != groupRoleAssignmentDto.GroupId)
                {
                    return BadRequest(new { message = "Group ID in URL and body must match" });
                }

                var result = await _groupRepository.AddRolesToGroupAsync(id, groupRoleAssignmentDto.RoleIds, groupRoleAssignmentDto.Note);
                if (!result)
                {
                    return NotFound(new { message = "Group not found or some roles not found" });
                }

                return Ok(new { message = "Roles added to group successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding roles to group", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove roles from group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="roleIds">List of role IDs to remove</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/roles")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.ROLES")]
        public async Task<ActionResult> RemoveRolesFromGroup(string id, [FromBody] List<string> roleIds)
        {
            try
            {
                if (roleIds == null || !roleIds.Any())
                {
                    return BadRequest(new { message = "Role IDs are required" });
                }

                var result = await _groupRepository.RemoveRolesFromGroupAsync(id, roleIds);
                if (!result)
                {
                    return NotFound(new { message = "Group not found or roles not found in group" });
                }

                return Ok(new { message = "Roles removed from group successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing roles from group", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all roles assigned to a group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>List of group role assignments</returns>
        [HttpGet("{id}/roles")]
        [Authorize(Roles = "READ.GROUPS,READ.ROLES")]
        public async Task<ActionResult<List<GroupRoleDto>>> GetGroupRoles(string id)
        {
            try
            {
                var groupRoles = await _groupRepository.GetGroupRolesAsync(id);
                return Ok(groupRoles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving group roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all groups that have a specific role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of group role assignments</returns>
        [HttpGet("role/{roleId}")]
        [Authorize(Roles = "READ.GROUPS,READ.ROLES")]
        public async Task<ActionResult<List<GroupRoleDto>>> GetRoleGroups(string roleId)
        {
            try
            {
                var roleGroups = await _groupRepository.GetRoleGroupsAsync(roleId);
                return Ok(roleGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving role groups", error = ex.Message });
            }
        }

        /// <summary>
        /// Replace all roles for a group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <param name="groupRoleAssignmentDto">Group role assignment data</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}/roles")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.ROLES")]
        public async Task<ActionResult> ReplaceGroupRoles(string id, [FromBody] GroupRoleAssignmentDto groupRoleAssignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (id != groupRoleAssignmentDto.GroupId)
                {
                    return BadRequest(new { message = "Group ID in URL and body must match" });
                }

                var result = await _groupRepository.ReplaceGroupRolesAsync(id, groupRoleAssignmentDto.RoleIds, groupRoleAssignmentDto.Note);
                if (!result)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(new { message = "Group roles replaced successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while replacing group roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove all roles from group
        /// </summary>
        /// <param name="id">Group ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/roles/all")]
        [Authorize(Roles = "UPDATE.GROUPS,UPDATE.ROLES")]
        public async Task<ActionResult> RemoveAllGroupRoles(string id)
        {
            try
            {
                var result = await _groupRepository.RemoveAllGroupRolesAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Group not found" });
                }

                return Ok(new { message = "All roles removed from group successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing all roles from group", error = ex.Message });
            }
        }

        /// <summary>
        /// Get groups by time active
        /// </summary>
        /// <param name="timeActiveId">Time Active ID</param>
        /// <returns>List of groups with the specified time active</returns>
        [HttpGet("time-active/{timeActiveId}")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult<List<GroupInfoDto>>> GetGroupsByTimeActive(string timeActiveId)
        {
            try
            {
                var groups = await _groupRepository.GetGroupsByTimeActiveAsync(timeActiveId);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving groups by time active", error = ex.Message });
            }
        }

        /// <summary>
        /// Bulk operation on groups
        /// </summary>
        /// <param name="bulkOperationDto">Bulk operation data</param>
        /// <returns>Number of affected groups</returns>
        [HttpPost("bulk-operation")]
        [Authorize(Roles = "UPDATE.GROUPS,DELETE.GROUPS")]
        public async Task<ActionResult> BulkGroupOperation([FromBody] BulkGroupOperationDto bulkOperationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var affectedCount = await _groupRepository.BulkGroupOperationAsync(bulkOperationDto);

                return Ok(new
                {
                    message = $"Bulk operation completed successfully",
                    affectedGroups = affectedCount,
                    operation = bulkOperationDto.Operation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while performing bulk operation", error = ex.Message });
            }
        }

        /// <summary>
        /// Search groups with advanced filters
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Filtered groups with pagination</returns>
        [HttpPost("search")]
        [Authorize(Roles = "READ.GROUPS")]
        public async Task<ActionResult> SearchGroups(
            [FromBody] GroupSearchDto searchDto,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (groups, totalCount) = await _groupRepository.SearchGroupsAsync(searchDto, pageNumber, pageSize);

                var response = new
                {
                    groups = groups,
                    pagination = new
                    {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasNextPage = pageNumber * pageSize < totalCount,
                        hasPreviousPage = pageNumber > 1
                    },
                    searchCriteria = searchDto
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching groups", error = ex.Message });
            }
        }
    }
}
