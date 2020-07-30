﻿using Microsoft.AspNetCore.Identity;
using POCForVivek.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace POCForVivek.Common
{

    public interface ICustomUserValidator<TUser> : IUserValidator<TUser> where TUser : A2ZClientCredentials
    {
    }

    /// <summary>
    /// Provides validation services for user classes.
    /// </summary>
    /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
    public class CustomUserValidator<TUser> : IUserValidator<TUser>, ICustomUserValidator<TUser> where TUser : A2ZClientCredentials
    {
        /// <summary>
        /// Creates a new instance of <see cref="UserValidator{TUser}"/>/
        /// </summary>
        /// <param name="errors">The <see cref="IdentityErrorDescriber"/> used to provider error messages.</param>
        public CustomUserValidator(IdentityErrorDescriber errors = null)
        {
            Describer = errors ?? new IdentityErrorDescriber();
        }

        /// <summary>
        /// Gets the <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{TUser}"/>.
        /// </summary>
        /// <value>The <see cref="IdentityErrorDescriber"/> used to provider error messages for the current <see cref="UserValidator{TUser}"/>.</value>
        public IdentityErrorDescriber Describer { get; private set; }

        /// <summary>
        /// Validates the specified <paramref name="user"/> as an asynchronous operation.
        /// </summary>
        /// <param name="manager">The <see cref="UserManager{TUser}"/> that can be used to retrieve user properties.</param>
        /// <param name="user">The user to validate.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation, containing the <see cref="IdentityResult"/> of the validation operation.</returns>
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var errors = new List<IdentityError>();
            await ValidateUserName(manager, user, errors);
            if (manager.Options.User.RequireUniqueEmail)
            {
                await ValidateEmail(manager, user, errors);
            }
            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateUserName(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
        {
            var userName = await manager.GetUserNameAsync(user);
            if (string.IsNullOrWhiteSpace(userName))
            {
                errors.Add(Describer.InvalidUserName(userName));
            }
            else if (!string.IsNullOrEmpty(manager.Options.User.AllowedUserNameCharacters) &&
                userName.Any(c => !manager.Options.User.AllowedUserNameCharacters.Contains(c)))
            {
                errors.Add(Describer.InvalidUserName(userName));
            }
            else
            {
                var owner = await manager.FindByNameAsync(userName);
                if (owner != null &&
                    !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
                {
                    errors.Add(Describer.DuplicateUserName(userName));
                }
            }
        }

        // make sure email is not empty, valid, and unique
        private async Task ValidateEmail(UserManager<TUser> manager, TUser user, List<IdentityError> errors)
        {
            var email = await manager.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
            {
                errors.Add(Describer.InvalidEmail(email));
                return;
            }
            if (!new EmailAddressAttribute().IsValid(email))
            {
                errors.Add(Describer.InvalidEmail(email));
                return;
            }
            //var owner = await manager.FindByEmailAsync(email);
            //if (owner != null &&
            //    !string.Equals(await manager.GetUserIdAsync(owner), await manager.GetUserIdAsync(user)))
            //{
            //    errors.Add(Describer.DuplicateEmail(email));
            //}
        }

    }

    public interface ICustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : A2ZClientCredentials
    {
    }

    public class CustomPasswordValidator<TUser> : ICustomPasswordValidator<TUser> where TUser : A2ZClientCredentials
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            Regex re = new Regex("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$");
            Match ma = re.Match(password);
            if (ma.Success)
            {

                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                //List<IdentityError> errors = new List<IdentityError>();
                IdentityError error = new IdentityError
                {
                    Code = "Code-001",
                    Description = "Invalid password or does not match criteria"
                };

                //errors.Add(error);

                return Task.FromResult(IdentityResult.Failed(error));
            }
        }
    }


}
