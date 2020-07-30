using Microsoft.AspNetCore.Identity;
using POCForVivek.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace POCForVivek.Common.Identity
{
    public class AppClaimsPrincipleFactory : IUserClaimsPrincipalFactory<A2ZClientCredentials>
    {
        public Task<ClaimsPrincipal> CreateAsync(A2ZClientCredentials user)
        {
            return Task.Factory.StartNew(() =>
            {
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                var principle = new ClaimsPrincipal(identity);

                return principle;
            });
        }
    }
}
