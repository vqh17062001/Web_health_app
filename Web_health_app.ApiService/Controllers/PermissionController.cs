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
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionController(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        }

        /// <summary>
        /// Get all permissions with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive permissions</param>
        /// <returns>Paginated list of permissions</returns>
        [HttpGet]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<PermissionsApiResponse>>> GetAllPermissions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return BadRequest(new ApiResponse<PermissionsApiResponse>
                    {
                        IsSuccess = false,
                        Message = "Page number and page size must be greater than 0"
                    });
                }

                var (permissions, totalCount) = await _permissionRepository.GetAllPermissionsAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var pagination = new PermissionsPaginationInfo
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = pageNumber > 1
                };

                var response = new PermissionsApiResponse
                {
                    Permissions = permissions,
                    Pagination = pagination
                };

                return Ok(new ApiResponse<PermissionsApiResponse>
                {
                    IsSuccess = true,
                    Message = "Permissions retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PermissionsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get permission by ID
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>Permission information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<PermissionInfoDto>>> GetPermissionById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Permission ID is required"
                    });
                }

                var permission = await _permissionRepository.GetPermissionByIdAsync(id);

                if (permission == null)
                {
                    return NotFound(new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Permission not found"
                    });
                }

                return Ok(new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = true,
                    Message = "Permission retrieved successfully",
                    Data = permission
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Create new permission
        /// </summary>
        /// <param name="createPermissionDto">Permission creation data</param>
        /// <returns>Created permission information</returns>
        [HttpPost]
        [Authorize(Roles = "CREATE.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<PermissionInfoDto>>> CreatePermission([FromBody] CreatePermissionDto createPermissionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid input data"
                    });
                }

                var permission = await _permissionRepository.CreatePermissionAsync(createPermissionDto);

                return CreatedAtAction(
                    nameof(GetPermissionById),
                    new { id = permission.PermissionId },
                    new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = true,
                        Message = "Permission created successfully",
                        Data = permission
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update permission
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <param name="updatePermissionDto">Permission update data</param>
        /// <returns>Updated permission information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "UPDATE.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<PermissionInfoDto>>> UpdatePermission(string id, [FromBody] UpdatePermissionDto updatePermissionDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Permission ID is required"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid input data"
                    });
                }

                var permission = await _permissionRepository.UpdatePermissionAsync(id, updatePermissionDto);

                if (permission == null)
                {
                    return NotFound(new ApiResponse<PermissionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Permission not found"
                    });
                }

                return Ok(new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = true,
                    Message = "Permission updated successfully",
                    Data = permission
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<PermissionInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete permission (soft delete)
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}/inactive")]
        [Authorize(Roles = "DELETE.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<object>>> DeletePermission(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Permission ID is required"
                    });
                }

                var result = await _permissionRepository.DeletePermissionAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Permission not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Permission deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Hard delete permission (permanent removal)
        /// </summary>
        /// <param name="id">Permission ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id}/hard")]
        [Authorize(Roles = "DELETE.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<object>>> HardDeletePermission(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Permission ID is required"
                    });
                }

                var result = await _permissionRepository.HardDeletePermissionAsync(id);

                if (!result)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = "Permission not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    IsSuccess = true,
                    Message = "Permission permanently deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get permissions by Action ID
        /// </summary>
        /// <param name="actionId">Action ID</param>
        /// <returns>List of permissions for the action</returns>
        [HttpGet("by-action/{actionId}")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<List<PermissionInfoDto>>>> GetPermissionsByAction(string actionId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(actionId))
                {
                    return BadRequest(new ApiResponse<List<PermissionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = "Action ID is required"
                    });
                }

                var permissions = await _permissionRepository.GetPermissionsByActionAsync(actionId);

                return Ok(new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Permissions retrieved successfully",
                    Data = permissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get permissions by Entity ID
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <returns>List of permissions for the entity</returns>
        [HttpGet("by-entity/{entityId}")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<List<PermissionInfoDto>>>> GetPermissionsByEntity(string entityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityId))
                {
                    return BadRequest(new ApiResponse<List<PermissionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = "Entity ID is required"
                    });
                }

                var permissions = await _permissionRepository.GetPermissionsByEntityAsync(entityId);

                return Ok(new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Permissions retrieved successfully",
                    Data = permissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get permissions by Role ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>List of permissions for the role</returns>
        [HttpGet("by-role/{roleId}")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<List<PermissionInfoDto>>>> GetPermissionsByRole(string roleId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(roleId))
                {
                    return BadRequest(new ApiResponse<List<PermissionInfoDto>>
                    {
                        IsSuccess = false,
                        Message = "Role ID is required"
                    });
                }

                var permissions = await _permissionRepository.GetPermissionsByRoleAsync(roleId);

                return Ok(new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Permissions retrieved successfully",
                    Data = permissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<PermissionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get available actions for dropdown
        /// </summary>
        /// <returns>List of available actions</returns>
        [HttpGet("available-actions")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<List<ActionInfoDto>>>> GetAvailableActions()
        {
            try
            {
                var actions = await _permissionRepository.GetAvailableActionsAsync();

                return Ok(new ApiResponse<List<ActionInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Available actions retrieved successfully",
                    Data = actions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<ActionInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get available entities for dropdown
        /// </summary>
        /// <returns>List of available entities</returns>
        [HttpGet("available-entities")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<List<EntityInfoDto>>>> GetAvailableEntities()
        {
            try
            {
                var entities = await _permissionRepository.GetAvailableEntitiesAsync();

                return Ok(new ApiResponse<List<EntityInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Available entities retrieved successfully",
                    Data = entities
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<EntityInfoDto>>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Check if permission ID exists
        /// </summary>
        /// <param name="permissionId">Permission ID to check</param>
        /// <param name="excludePermissionId">Permission ID to exclude from check</param>
        /// <returns>Boolean indicating if permission ID exists</returns>
        [HttpGet("exists/{permissionId}")]
        [Authorize(Roles = "READ.PERMISSIONS")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckPermissionIdExists(
            string permissionId,
            [FromQuery] string? excludePermissionId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permissionId))
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        IsSuccess = false,
                        Message = "Permission ID is required"
                    });
                }

                var exists = await _permissionRepository.PermissionIdExistsAsync(permissionId, excludePermissionId);

                return Ok(new ApiResponse<bool>
                {
                    IsSuccess = true,
                    Message = "Permission ID existence checked successfully",
                    Data = exists
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<bool>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }
    }
}
