using System.Security.Claims;
using Schedules.Utils;

namespace Schedules.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetStudentNumber(this ClaimsPrincipal principal)
        {
            if (principal.Identity is ClaimsIdentity claims)
            {
                return claims.FindFirst(Claims.StudentNumber)?.Value;
            }

            return null;
        }
    }
}