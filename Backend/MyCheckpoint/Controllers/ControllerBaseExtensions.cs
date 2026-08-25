using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace MyCheckpoint.Controllers
{
    public static class ControllerBaseExtensions
    {
        public static int GetUserId(this ControllerBase controller)
        {
            var idClaim = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idClaim!);
        }

        public static int? GetUserIdOrNull(this ControllerBase controller)
        {
            var idClaim = controller.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return idClaim is null ? null : int.Parse(idClaim);
        }
    }
}
