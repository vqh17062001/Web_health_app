using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web_health_app.ApiService.Repository;
using Web_health_app.Models.Models;

namespace Web_health_app.ApiService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ActionEntityController : ControllerBase
    {
        private readonly IActionRepository _actionRepository;
        private readonly IEntityRepository _entityRepository;

        public ActionEntityController(IActionRepository actionRepository, IEntityRepository entityRepository)
        {
            _actionRepository = actionRepository ?? throw new ArgumentNullException(nameof(actionRepository));
            _entityRepository = entityRepository ?? throw new ArgumentNullException(nameof(entityRepository));
        }

        #region Action Endpoints

        /// <summary>
        /// Get all actions with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <param name="includeInactive">Include inactive actions</param>
        /// <returns>Paginated list of actions</returns>
        [HttpGet("actions")]
        [Authorize(Roles = "READ.ACTIONS")]
        public async Task<ActionResult<ApiResponse<ActionsApiResponse>>> GetAllActions(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return BadRequest(new ApiResponse<ActionsApiResponse>
                    {
                        IsSuccess = false,
                        Message = "Page number and page size must be greater than 0"
                    });
                }

                var (actions, totalCount) = await _actionRepository.GetAllActionsAsync(pageNumber, pageSize, searchTerm, includeInactive);

                var pagination = new ActionsPaginationInfo
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = pageNumber > 1
                };

                var response = new ActionsApiResponse
                {
                    Actions = actions,
                    Pagination = pagination
                };

                return Ok(new ApiResponse<ActionsApiResponse>
                {
                    IsSuccess = true,
                    Message = "Actions retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ActionsApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get action by ID
        /// </summary>
        /// <param name="id">Action ID</param>
        /// <returns>Action information</returns>
        [HttpGet("actions/{id}")]
        [Authorize(Roles = "READ.ACTIONS")]
        public async Task<ActionResult<ApiResponse<ActionInfoDto>>> GetActionById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<ActionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Action ID is required"
                    });
                }

                var action = await _actionRepository.GetActionByIdAsync(id);

                if (action == null)
                {
                    return NotFound(new ApiResponse<ActionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Action not found"
                    });
                }

                return Ok(new ApiResponse<ActionInfoDto>
                {
                    IsSuccess = true,
                    Message = "Action retrieved successfully",
                    Data = action
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ActionInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update action
        /// </summary>
        /// <param name="id">Action ID</param>
        /// <param name="updateActionDto">Action update data</param>
        /// <returns>Updated action information</returns>
        [HttpPut("actions/{id}")]
        [Authorize(Roles = "UPDATE.ACTIONS")]
        public async Task<ActionResult<ApiResponse<ActionInfoDto>>> UpdateAction(string id, [FromBody] UpdateActionDto updateActionDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<ActionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Action ID is required"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<ActionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid input data"
                    });
                }

                var action = await _actionRepository.UpdateActionAsync(id, updateActionDto);

                if (action == null)
                {
                    return NotFound(new ApiResponse<ActionInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Action not found"
                    });
                }

                return Ok(new ApiResponse<ActionInfoDto>
                {
                    IsSuccess = true,
                    Message = "Action updated successfully",
                    Data = action
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<ActionInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get active actions for dropdown
        /// </summary>
        /// <returns>List of active actions</returns>
        [HttpGet("actions/active")]
        [Authorize(Roles = "READ.ACTIONS")]
        public async Task<ActionResult<ApiResponse<List<ActionInfoDto>>>> GetActiveActions()
        {
            try
            {
                var actions = await _actionRepository.GetActiveActionsAsync();

                return Ok(new ApiResponse<List<ActionInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Active actions retrieved successfully",
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

        #endregion

        #region Entity Endpoints

        /// <summary>
        /// Get all entities with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of entities</returns>
        [HttpGet("entities")]
        [Authorize(Roles = "READ.ENTITY")]
        public async Task<ActionResult<ApiResponse<EntitiesApiResponse>>> GetAllEntities(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1 || pageSize < 1)
                {
                    return BadRequest(new ApiResponse<EntitiesApiResponse>
                    {
                        IsSuccess = false,
                        Message = "Page number and page size must be greater than 0"
                    });
                }

                var (entities, totalCount) = await _entityRepository.GetAllEntitiesAsync(pageNumber, pageSize, searchTerm);

                var pagination = new EntitiesPaginationInfo
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / pageSize),
                    HasPreviousPage = pageNumber > 1
                };

                var response = new EntitiesApiResponse
                {
                    Entities = entities,
                    Pagination = pagination
                };

                return Ok(new ApiResponse<EntitiesApiResponse>
                {
                    IsSuccess = true,
                    Message = "Entities retrieved successfully",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<EntitiesApiResponse>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get entity by ID
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <returns>Entity information</returns>
        [HttpGet("entities/{id}")]
        [Authorize(Roles = "READ.ENTITY")]
        public async Task<ActionResult<ApiResponse<EntityInfoDto>>> GetEntityById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<EntityInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Entity ID is required"
                    });
                }

                var entity = await _entityRepository.GetEntityByIdAsync(id);

                if (entity == null)
                {
                    return NotFound(new ApiResponse<EntityInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Entity not found"
                    });
                }

                return Ok(new ApiResponse<EntityInfoDto>
                {
                    IsSuccess = true,
                    Message = "Entity retrieved successfully",
                    Data = entity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<EntityInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update entity
        /// </summary>
        /// <param name="id">Entity ID</param>
        /// <param name="updateEntityDto">Entity update data</param>
        /// <returns>Updated entity information</returns>
        [HttpPut("entities/{id}")]
        [Authorize(Roles = "UPDATE.ENTITY")]
        public async Task<ActionResult<ApiResponse<EntityInfoDto>>> UpdateEntity(string id, [FromBody] UpdateEntityDto updateEntityDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new ApiResponse<EntityInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Entity ID is required"
                    });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<EntityInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Invalid input data"
                    });
                }

                var entity = await _entityRepository.UpdateEntityAsync(id, updateEntityDto);

                if (entity == null)
                {
                    return NotFound(new ApiResponse<EntityInfoDto>
                    {
                        IsSuccess = false,
                        Message = "Entity not found"
                    });
                }

                return Ok(new ApiResponse<EntityInfoDto>
                {
                    IsSuccess = true,
                    Message = "Entity updated successfully",
                    Data = entity
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<EntityInfoDto>
                {
                    IsSuccess = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get entities by minimum security level
        /// </summary>
        /// <param name="minLevel">Minimum security level</param>
        /// <returns>List of entities with security level >= minLevel</returns>
        [HttpGet("entities/by-security-level/{minLevel}")]
        [Authorize(Roles = "READ.ENTITY")]
        public async Task<ActionResult<ApiResponse<List<EntityInfoDto>>>> GetEntitiesBySecurityLevel(byte minLevel)
        {
            try
            {
                if (minLevel < 1 || minLevel > 5)
                {
                    return BadRequest(new ApiResponse<List<EntityInfoDto>>
                    {
                        IsSuccess = false,
                        Message = "Security level must be between 1 and 5"
                    });
                }

                var entities = await _entityRepository.GetEntitiesBySecurityLevelAsync(minLevel);

                return Ok(new ApiResponse<List<EntityInfoDto>>
                {
                    IsSuccess = true,
                    Message = "Entities retrieved successfully",
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

        #endregion
    }
}
