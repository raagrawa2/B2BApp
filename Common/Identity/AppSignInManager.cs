using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POCForVivek.Models;
using Raven.Client.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POCForVivek.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace POCForVivek.Common.Identity
{
    public class AppSignInManager : SignInManager<A2ZClientCredentials>
    {
        private readonly CustomUserManager<A2ZClientCredentials,IDocumentStore> _userManager;
        private readonly IDocumentStore _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;

        public AppSignInManager(
    CustomUserManager<A2ZClientCredentials,IDocumentStore> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<A2ZClientCredentials> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<A2ZClientCredentials>> logger,
            IAuthenticationSchemeProvider schemeProvider,
            IUserConfirmation<A2ZClientCredentials> confirmation,
            IDocumentStore dbContext
            )
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemeProvider, confirmation)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public override Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            // here goes the external username and password look up
            A2ZClientCredentials user = this._userManager.FindByNameAsync(userName).GetAwaiter().GetResult();
            if (null != user)
            {
                if (true == user.LockoutEnabled)
                {
                    return Task.FromResult(SignInResult.LockedOut);
                }

                var result = this._userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash ,password);

                if(result == PasswordVerificationResult.Success)
                {
                    ClaimsIdentity identity = new ClaimsIdentity(this.GetUserClaims(user), "Basic");
                    ClaimsPrincipal principal = new ClaimsPrincipal(identity);
                    _contextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(20),
                        IsPersistent = true,
                        AllowRefresh = false
                    }).GetAwaiter().GetResult();
                    return Task.FromResult(SignInResult.Success);
                }
                else
                {
                    return Task.FromResult(SignInResult.Failed);
                }

            }
            else
            {
                return Task.FromResult(SignInResult.Failed);
            }
        }

        private IEnumerable<Claim> GetUserClaims(A2ZClientCredentials user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }

        private IEnumerable<Claim> GetUserRoleClaims(A2ZClientCredentials user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Role, user.UserPermissionType.ToString()));
            return claims;
        }

    }
}
