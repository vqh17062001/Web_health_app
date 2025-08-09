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
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// Get all roles with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive roles</param>
        /// <returns>Paginated list of roles</returns>
        [HttpGet]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult> GetAllRoles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (roles, totalCount) = await _roleRepository.GetAllRolesAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var response = new
                {
                    roles = roles,
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
                return StatusCode(500, new { message = "An error occurred while retrieving roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Role information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult<RoleInfoDto>> GetRoleById(string id)
        {
            try
            {
                var role = await _roleRepository.GetRoleByIdAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving role", error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active roles (for dropdown lists)
        /// </summary>
        /// <returns>List of active roles</returns>
        [HttpGet("active")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult<List<RoleInfoDto>>> GetActiveRoles()
        {
            try
            {
                var roles = await _roleRepository.GetActiveRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving active roles", error = ex.Message });
            }
        }

        /// <summary>
        /// Create new role (RoleId will be auto-generated from RoleName)
        /// </summary>
        /// <param name="createRoleDto">Role creation data (RoleId is auto-generated)</param>
        /// <returns>Created role information</returns>
        [HttpPost]
        [Authorize(Roles = "CREATE.ROLES")]
        public async Task<ActionResult<RoleInfoDto>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdRole = await _roleRepository.CreateRoleAsync(createRoleDto);
                return CreatedAtAction(nameof(GetRoleById), new { id = createdRole.RoleId }, createdRole);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating role", error = ex.Message });
            }
        }

        /// <summary>
        /// Update role information
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="updateRoleDto">Updated role data</param>
        /// <returns>Updated role information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "UPDATE.ROLES")]
        public async Task<ActionResult<RoleInfoDto>> UpdateRole(string id, [FromBody] UpdateRoleDto updateRoleDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedRole = await _roleRepository.UpdateRoleAsync(id, updateRoleDto);
                if (updatedRole == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                return Ok(updatedRole);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating role", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete role (soft delete)
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE.ROLES")]
        public async Task<ActionResult> DeleteRole(string id)
        {
            try
            {
                var result = await _roleRepository.DeleteRoleAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Role not found" });
                }

                return Ok(new { message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting role", error = ex.Message });
            }
        }

        /// <summary>
        /// Permanently delete role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "DELETE.ROLES")]
        public async Task<ActionResult> HardDeleteRole(string id)
        {
            try
            {
                var result = await _roleRepository.HardDeleteRoleAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "Role not found" });
                }

                return Ok(new { message = "Role permanently deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while permanently deleting role", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if role ID exists
        /// </summary>
        /// <param name="roleId">Role ID to check</param>
        /// <returns>Boolean indicating if role ID exists</returns>
        [HttpGet("check-role-id/{roleId}")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult> CheckRoleId(string roleId)
        {
            try
            {
                var exists = await _roleRepository.RoleIdExistsAsync(roleId);
                return Ok(new { roleId = roleId, exists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking role ID", error = ex.Message });
            }
        }

        /// <summary>
        /// Assign permissions to role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="roleAssignmentDto">Role assignment data</param>
        /// <returns>Success message</returns>
        //[HttpPost("{id}/permissions")]
        //[Authorize(Roles = "UPDATE.ROLES")]
        //public async Task<ActionResult> AssignPermissionsToRole(string id, [FromBody] RoleAssignmentDto roleAssignmentDto)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        if (id != roleAssignmentDto.RoleId)
        //        {
        //            return BadRequest(new { message = "Role ID in URL and body must match" });
        //        }

        //        var result = await _roleRepository.AssignPermissionsToRoleAsync(id, roleAssignmentDto.PermissionIds);
        //        if (!result)
        //        {
        //            return NotFound(new { message = "Role not found" });
        //        }

        //        return Ok(new { message = "Permissions assigned successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "An error occurred while assigning permissions", error = ex.Message });
        //    }
        //}

        /// <summary>
        /// Get roles with user count
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive roles</param>
        /// <returns>Paginated list of roles with user count</returns>
        [HttpGet("with-user-count")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult> GetRolesWithUserCount(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (roles, totalCount) = await _roleRepository.GetRolesWithUserCountAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var response = new
                {
                    roles = roles,
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
                return StatusCode(500, new { message = "An error occurred while retrieving roles with user count", error = ex.Message });
            }
        }

        /// <summary>
        /// Get permissions assigned to a role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>List of permission IDs assigned to the role</returns>
        [HttpGet("{id}/permissions")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult> GetRolePermissions(string id)
        {
            try
            {
                var permissions = await _roleRepository.GetRolePermissionsAsync(id);
                return Ok(new
                {
                    roleId = id,
                    permissions = permissions,
                    totalPermissions = permissions.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving role permissions", error = ex.Message });
            }
        }

        /// <summary>
        /// Get users assigned to a specific role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>List of users assigned to the role</returns>
        [HttpGet("{id}/users")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult<List<UserInfoDto>>> GetUsersInRole(string id)
        {
            try
            {
                var users = await _roleRepository.GetUsersInRoleAsync(id);
                return Ok(new
                {
                    roleId = id,
                    users = users,
                    totalUsers = users.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users in role", error = ex.Message });
            }
        }

        /// <summary>
        /// Search roles with advanced filtering
        /// </summary>
        /// <param name="searchDto">Search criteria</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Filtered list of roles</returns>
        [HttpPost("search")]
        [Authorize(Roles = "READ.ROLES")]
        public async Task<ActionResult> SearchRoles(
            [FromBody] RoleSearchDto searchDto,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (roles, totalCount) = await _roleRepository.SearchRolesAsync(searchDto, pageNumber, pageSize);

                var response = new
                {
                    roles = roles,
                    pagination = new
                    {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                        hasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                        hasPreviousPage = pageNumber > 1
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while searching roles", error = ex.Message });
            }
        }
    }
}
