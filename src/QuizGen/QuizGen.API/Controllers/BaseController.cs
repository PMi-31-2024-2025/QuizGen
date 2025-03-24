using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace QuizGen.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return 0; // Invalid user ID
            }
            return userId;
        }

        protected bool IsResourceOwner(int resourceOwnerId)
        {
            var currentUserId = GetCurrentUserId();
            return currentUserId > 0 && currentUserId == resourceOwnerId;
        }

        protected IActionResult UnauthorizedIfNotResourceOwner(int resourceOwnerId)
        {
            if (!IsResourceOwner(resourceOwnerId))
            {
                return Forbid("You are not authorized to access this resource");
            }
            return null;
        }
    }
} 