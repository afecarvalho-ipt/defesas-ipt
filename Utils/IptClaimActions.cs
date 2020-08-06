using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.Extensions.Configuration;

namespace Schedules.Utils
{
    /// <summary>
    /// ClaimAction that reads claims from IPT and adds:
    /// - The student number (if the user is a student).
    /// - The user's email.
    /// - The user's role.
    /// </summary>
    public class IptRoleClaimAction : ClaimAction
    {
        static readonly Regex StudentEmail = new Regex(@"(\d{2,})");
        private readonly List<string> admins = new List<string>();

        public IptRoleClaimAction(IConfiguration configuration) : base(ClaimTypes.Role, ClaimValueTypes.String)
        {
            configuration.Bind("Auth:Admins", this.admins);
        }

        public override void Run(JsonElement userData, ClaimsIdentity identity, string issuer)
        {
            if (identity == null) { return; }

            // https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens

            var emailToken = userData.GetProperty("email");

            if (emailToken.ValueKind == JsonValueKind.String)
            {
                // Students usually are aluno12345@ipt.pt.
                // Faculty usually don't have numbers on their email.
                var userName = emailToken.GetString().Split('@').First()?.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(identity.Name))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                }

                var studentNumberMatch = StudentEmail.Match(userName);

                if (studentNumberMatch.Success)
                {
                    identity.AddClaim(new Claim(SchedulesClaimTypes.StudentNumber, studentNumberMatch.Captures[0].Value));
                    identity.AddClaim(new Claim(ClaimTypes.Role, SchedulesRoles.Student));
                }
                else
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, SchedulesRoles.Faculty));
                }

                if (this.admins.Contains(userName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, SchedulesRoles.Admin));
                }
            }
        }
    }
}