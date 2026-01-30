using System.Security.Claims;

namespace Habit_Tracker_Backend.Helpers
{
    public static class ClaimsHelper
    {
        public static long GetUserId(ClaimsPrincipal user)
        {
            return long.Parse(user.FindFirst("userId")!.Value);
        }
    }

}
