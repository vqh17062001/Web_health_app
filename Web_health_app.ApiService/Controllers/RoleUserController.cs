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
    public class RoleUserController : ControllerBase
    {
        private readonly IRoleUserRepository _roleUserRepository;

        public RoleUserController(IRoleUserRepository roleUserRepository)
        {
            _roleUserRepository = roleUserRepository ?? throw new ArgumentNullException(nameof(roleUserRepository));
        }

        /// <summary>
        /// Assign roles to user
        /// </summary>
        /// <param name="userRoleAssignmentDto">User role assignment data</param>
        /// <returns>Success message</returns>
        [HttpPost("assign-roles-to-user")]
        [Authorize(Roles = "UPDATE.USERS,UPDATE.ROLES")]
        public async Task<ActionResult> AssignRolesToUser([FromBody] UserRoleAssignmentDto userRoleAssignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _roleUserRepository.AssignRolesToUserAsync(
                    userRoleAssignmentDto.UserId,
                    userRoleAssignmentDto.RoleIds);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to assign roles. User or some roles may not exist." });
                }

                return Ok(new { message = "Roles assigned to user successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning roles to user", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign users to role
        /// </summary>
        /// <param name="roleUserAssignmentDto">Role user assignment data</param>
        /// <returns>Success message</returns>
        [HttpPost("assign-users-to-role")]
        [Authorize(Roles = "UPDATE.USERS,UPDATE.ROLES")]
        public async Task<ActionResult> AssignUsersToRole([FromBody] RoleUserAssignmentDto roleUserAssignmentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var tasks = roleUserAssignmentDto.UserIds.Select(userId =>
                    _roleUserRepository.AssignRolesToUserAsync(userId, new List<string> { roleUserAssignmentDto.RoleId }));

                var results = await Task.WhenAll(tasks);

                if (results.Any(r => !r))
                {
                    return BadRequest(new { message = "Failed to assign some users to role. Some users may not exist." });
                }

                return Ok(new { message = "Users assigned to role successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while assigning users to role", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove roles from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">Role IDs to remove</param>
        /// <returns>Success message</returns>
        [HttpDelete("user/{userId}/roles")]
        [Authorize(Roles = "UPDATE.USERS,UPDATE.ROLES")]
        public async Task<ActionResult> RemoveRolesFromUser(Guid userId, [FromBody] List<string> roleIds)
        {
            try
            {
                if (roleIds == null || !roleIds.Any())
                {
                    return BadRequest(new { message = "Role IDs are required" });
                }

                var result = await _roleUserRepository.RemoveRolesFromUserAsync(userId, roleIds);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to remove roles from user" });
                }

                return Ok(new { message = "Roles removed from user successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing roles from user", error = ex.Message });
            }
        }

        /// <summary>
        /// Replace all roles for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleIds">New role IDs</param>
        /// <returns>Success message</returns>
        [HttpPut("user/{userId}/roles")]
        [Authorize(Roles = "UPDATE.USERS,UPDATE.ROLES")]
        public async Task<ActionResult> ReplaceUserRoles(Guid userId, [FromBody] List<string> roleIds)
        {
            try
            {
                if (roleIds == null)
                {
                    return BadRequest(new { message = "Role IDs are required (can be empty array)" });
                }

                var result = await _roleUserRepository.ReplaceUserRolesAsync(userId, roleIds);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to replace user roles. User or some roles may not exist." });
                }

                return Ok(new { message = "User roles replaced successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while replacing user roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of roles assigned to the user</returns>
        [HttpGet("user/{userId}/roles")]
        [Authorize(Roles = "READ.USERS,READ.ROLES")]
        public async Task<ActionResult<List<RoleInfoDto>>> GetUserRoles(Guid userId)
        {
            try
            {
                var roles = await _roleUserRepository.GetUserRolesAsync(userId);
                return Ok(new
                {
                    userId = userId,
                    roles = roles,
                    totalRoles = roles.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all users assigned to a role
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of users assigned to the role</returns>
        [HttpGet("role/{roleId}/users")]
        [Authorize(Roles = "READ.USERS,READ.ROLES")]
        public async Task<ActionResult<List<UserInfoDto>>> GetRoleUsers(string roleId)
        {
            try
            {
                var users = await _roleUserRepository.GetRoleUsersAsync(roleId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving role users", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <returns>Boolean indicating if user has the role</returns>
        [HttpGet("user/{userId}/has-role/{roleId}")]
        [Authorize(Roles = "READ.USERS,READ.ROLES")]
        public async Task<ActionResult> CheckUserHasRole(Guid userId, string roleId)
        {
            try
            {
                var hasRole = await _roleUserRepository.UserHasRoleAsync(userId, roleId);
                return Ok(new
                {
                    userId = userId,
                    roleId = roleId,
                    hasRole = hasRole
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking user role", error = ex.Message });
            }
        }

        /// <summary>
        /// Remove all roles from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("user/{userId}/roles/all")]
        [Authorize(Roles = "UPDATE.USERS,UPDATE.ROLES")]
        public async Task<ActionResult> RemoveAllUserRoles(Guid userId)
        {
            try
            {
                var result = await _roleUserRepository.RemoveAllUserRolesAsync(userId);

                if (!result)
                {
                    return BadRequest(new { message = "Failed to remove all roles from user" });
                }

                return Ok(new { message = "All roles removed from user successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing all roles from user", error = ex.Message });
            }
        }
    }
}
