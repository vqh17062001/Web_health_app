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
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Get all users with pagination and search
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="searchTerm">Search term for filtering</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet]
        [Authorize(Roles = "READ.USERS")]

        public async Task<ActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 10;

                var (users, totalCount) = await _userRepository.GetAllUsersAsync(pageNumber, pageSize, searchTerm);

                var response = new
                {
                    users = users,
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
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User information</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "READ.USERS")]

        public async Task<ActionResult<UserInfoDto>> GetUserById(Guid id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User information</returns>
        [HttpGet("username/{username}")]
        [AllowAnonymous]

        public async Task<ActionResult<UserInfoDto>> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user", error = ex.Message });
            }
        }

        [HttpPost("firstchangepassword")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> ChangePasswordUser([FromBody] ChangePasswordModel changePasswordModel) {

            try {



                var result = await _userRepository.FirstChangePasswordAsync(changePasswordModel);


                return result;

            }
            catch (Exception ex) { 
            
                return StatusCode(500, new { message = "An error occurred while retrieving user", error = ex.Message });


            }


        }
        /// <summary>
        /// Change user password
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="changePasswordDto">Password change data</param>
        /// <returns>Success message</returns>
        [HttpPost("changepassword")]
        [Authorize(Roles = "UPDATE.USERS")]

        public async Task<ActionResult> ChangePassword(ChangePasswordModel changePasswordModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _userRepository.ChangePasswordAsync(changePasswordModel);
                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while changing password", error = ex.Message });
            }
        }




        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="createUserDto">User creation data</param>
        /// <returns>Created user information</returns>
        [HttpPost]
        [Authorize(Roles = "CREATE.USERS")]

        public async Task<ActionResult<UserInfoDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdUser = await _userRepository.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserId }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateUserDto">Updated user data</param>
        /// <returns>Updated user information</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "UPDATE.USERS")]
        public async Task<ActionResult<UserInfoDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedUser = await _userRepository.UpdateUserAsync(id, updateUserDto);
                if (updatedUser == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "DELETE.USERS")]

        public async Task<ActionResult> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userRepository.DeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting user", error = ex.Message });
            }
        }

        /// <summary>
        /// Permanently delete user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/permanent")]
        [Authorize(Roles = "DELETE.USERS")]

        public async Task<ActionResult> HardDeleteUser(Guid id)
        {
            try
            {
                var result = await _userRepository.HardDeleteUserAsync(id);
                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User permanently deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while permanently deleting user", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>Boolean indicating if username exists</returns>
        [HttpGet("check-username/{username}")]
        [Authorize(Roles = "READ.USERS")]

        public async Task<ActionResult> CheckUsername(string username)
        {
            try
            {
                var exists = await _userRepository.UsernameExistsAsync(username);
                return Ok(new { username = username, exists = exists });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking username", error = ex.Message });
            }
        }

        /// <summary>
        /// Get users managed by a specific manager
        /// </summary>
        /// <param name="managerId">Manager user ID</param>
        /// <returns>List of users managed by the manager</returns>
        [HttpGet("manager/{managerId}")]
        [Authorize(Roles = "READ.USERS")]

        public async Task<ActionResult> GetUsersByManager(Guid managerId)
        {
            try
            {
                var users = await _userRepository.GetUsersByManagerAsync(managerId);
                return Ok(new
                {
                    managerId = managerId,
                    users = users,
                    totalCount = users.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users by manager", error = ex.Message });
            }
        }

        [HttpGet("GetUsersByCompareSecurityLevel/{level}")]

        public async Task<ActionResult> GetUsersByCompareSecurityLevel(int level)
        {
            try
            {
                var listUsers = await _userRepository.GetUserWithCompareSecurityLevel(level);
                return Ok(listUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users by group", error = ex.Message });
            }
        }





        

        /// <summary>
        /// Get current user information from JWT token
        /// </summary>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        public async Task<ActionResult<UserInfoDto>> GetCurrentUser()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrEmpty(username))
                {
                    return BadRequest(new { message = "Cannot determine current user from token" });
                }

                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound(new { message = "Current user not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving current user", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// DTO for changing password
    /// </summary>
   
}
