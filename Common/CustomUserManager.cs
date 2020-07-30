using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using POCForVivek.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Raven.Client.Documents;

namespace POCForVivek.Common
{
    public class CustomUserManager<TUser,TDocumentStore> : UserManager<TUser>
            where TUser : A2ZClientCredentials
            where TDocumentStore: class, IDocumentStore
    {
        public CustomUserManager(IRavenDBUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<TUser> passwordHasher, IEnumerable<ICustomUserValidator<TUser>> userValidators,
            IEnumerable<ICustomPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors, IServiceProvider tokenProviders,
            ILogger<UserManager<TUser>> logger,TDocumentStore context)
            : base(
                store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors,
                tokenProviders, logger)
        {
            _context = context;
            Store = store;
        }

        public IDocumentStore _context { get; private set; }

        public new IRavenDBUserStore<TUser> Store { get; private set; }


        public override async Task<IdentityResult> CreateAsync(TUser user)
        {
            ThrowIfDisposed();
            //await UpdateSecurityStampInternal(user);
            var result = await ValidateUserAsync(user);
            if (!result.Succeeded)
            {
                return result;
            }
            //if (Options.Lockout.AllowedForNewUsers && SupportsUserLockout)
            //{
            //    await GetUserLockoutStore().SetLockoutEnabledAsync(user, true, CancellationToken);
            //}
            //await UpdateNormalizedUserNameAsync(user);
            //await UpdateNormalizedEmailAsync(user);

            return await Store.CreateAsync(user, new System.Threading.CancellationToken());

            //var DbSession = _context.OpenSession();

            //DbSession.Store(user);
            //DbSession.SaveChanges();


            //return IdentityResult.Success;
        }

        /// <summary>
        /// Creates the specified <paramref name="user"/> in the backing store with given password,
        /// as an asynchronous operation.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <param name="password">The password for the user to hash and store.</param>
        /// <returns>
        /// The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/>
        /// of the operation.
        /// </returns>
        public override async Task<IdentityResult> CreateAsync(TUser user, string password)
        {
            ThrowIfDisposed();
            var passwordStore = GetPasswordStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }
            var result = await UpdatePasswordHash(passwordStore, user, password);
            if (!result.Succeeded)
            {
                return result;
            }
            return await CreateAsync(user);
        }

        /// <summary>
        /// Updates a user's password hash.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="validatePassword">Whether to validate the password.</param>
        /// <returns>Whether the password has was successfully updated.</returns>
        protected override Task<IdentityResult> UpdatePasswordHash(TUser user, string newPassword, bool validatePassword)
            => UpdatePasswordHash(GetPasswordStore(), user, newPassword, validatePassword);

        private async Task<IdentityResult> UpdatePasswordHash(IUserPasswordStore<TUser> passwordStore,
            TUser user, string newPassword, bool validatePassword = true)
        {
            if (validatePassword)
            {
                var validate = await ValidatePasswordAsync(user, newPassword);
                if (!validate.Succeeded)
                {
                    return validate;
                }
            }
            var hash = newPassword != null ? PasswordHasher.HashPassword(user, newPassword) : null;
            await passwordStore.SetPasswordHashAsync(user, hash, CancellationToken);
            //await UpdateSecurityStampInternal(user);
            return IdentityResult.Success;
        }

        private IUserPasswordStore<TUser> GetPasswordStore()
        {
            var cast = Store as IUserPasswordStore<TUser>;
            if (cast == null)
            {
                throw new NotSupportedException("The User store is not supported");
            }
            return cast;
        }

        /// <summary>
        /// Should return <see cref="IdentityResult.Success"/> if validation is successful. This is
        /// called before saving the user via Create or Update.
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>A <see cref="IdentityResult"/> representing whether validation was successful.</returns>
        protected async new Task<IdentityResult> ValidateUserAsync(TUser user)
        {
            //if (SupportsUserSecurityStamp)
            //{
            //    var stamp = await GetSecurityStampAsync(user);
            //    if (stamp == null)
            //    {
            //        throw new InvalidOperationException(Resources.NullSecurityStamp);
            //    }
            //}
            var errors = new List<IdentityError>();
            foreach (var v in UserValidators)
            {
                var result = await v.ValidateAsync(this, user);
                if (!result.Succeeded)
                {
                    errors.AddRange(result.Errors);
                }
            }
            if (errors.Count > 0)
            {
                Logger.LogWarning(13, "User {userId} validation failed: {errors}.", await GetUserIdAsync(user), string.Join(";", errors.Select(e => e.Code)));
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
        }

    }
}
