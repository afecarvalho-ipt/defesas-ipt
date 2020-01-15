using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Newtonsoft.Json.Linq;

namespace Schedules.Utils
{
    public class IptRoleClaimAction : ClaimAction
    {
        static readonly Regex StudentEmail = new Regex(@"(\d{2,})");

        public IptRoleClaimAction() : base(ClaimTypes.Role, ClaimValueTypes.String)
        {
        }

        public override void Run(JsonElement userData, ClaimsIdentity identity, string issuer)
        {
            if (identity == null) { return; }

            // https://docs.microsoft.com/en-us/azure/active-directory/develop/id-tokens

            var emailToken = userData.GetProperty("email");

            if (emailToken.ValueKind == JsonValueKind.String)
            {
                var email = emailToken.GetString().Split('@').First();

                if (string.IsNullOrWhiteSpace(identity.Name))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Name, email));
                }

                var studentNumberMatch = StudentEmail.Match(email);

                if (studentNumberMatch.Success)
                {
                    identity.AddClaim(new Claim(SchedulesClaimTypes.StudentNumber, studentNumberMatch.Captures[0].Value));
                    identity.AddClaim(new Claim(ClaimTypes.Role, SchedulesRoles.Student));
                }
                else
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, SchedulesRoles.Faculty));
                }
            }
        }
    }
}