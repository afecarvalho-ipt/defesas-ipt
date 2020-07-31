using System.Security.Claims;
using Schedules.Utils;

namespace Schedules.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns the current user's student number, if they are a student.
        /// Otherwise, returns null.
        /// </summary>
        public static string GetStudentNumber(this ClaimsPrincipal principal)
        {
            if (principal?.Identity is ClaimsIdentity claims)
            {
                return claims.FindFirst(SchedulesClaimTypes.StudentNumber)?.Value;
            }

            return null;
        }
    }
}