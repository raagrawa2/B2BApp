using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using POCForVivek.Common;
using POCForVivek.Common.Identity;

namespace POCForVivek.Models
{
    public class A2ZClientRegistrationModel
    {
        public string Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [RegularExpression("^((?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])|(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[^a-zA-Z0-9])|(?=.*?[A-Z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])|(?=.*?[a-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9])).{8,}$", ErrorMessage = "Passwords must be at least 8 characters and contain at 3 of 4 of the following: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string Password { get; set; }
        [Required]
        [StringLength(100)]
        public string FirmName { get; set; }
        [Required]
        [StringLength(100)]
        public string FirmAddress { get; set; }
        [Required]
        [StringLength(100)]
        public string ContactPersonName { get; set; }
        [Required]
        [StringLength(100)]
        public string ContactNo { get; set; }
        [Required]
        [StringLength(100)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email is not in valid format")]
        public string ContactEmail { get; set; }
        [Required]
        [StringLength(100)]
        public string GSTNo { get; set; }
        [Required]
        [StringLength(100)]
        public string City { get; set; }
        [Required]
        [StringLength(100)]
        public string State { get; set; }
        [Required]
        [StringLength(100)]
        public string PinCode { get; set; }
        [Required]
        [StringLength(100)]
        public string Website { get; set; }
    }

    public class A2ZClientCredentials : Raven.Identity.IdentityUser
    {
        //public string Id { get; set; }
        //public string Username { get; set; }
        public string Password { get; set; }
        public string UserPermissionType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }

    }

    public class A2ZClientDetails
    {
        public string Id { get; set; }
        public string ClientCredId { get; set; }
        public string FirmName { get; set; }
        public string FirmAddress { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactNo { get; set; }
        public string ContactEmail { get; set; }
        public string GSTNo { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PinCode { get; set; }
        public string Website { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }


    public class A2ZClientDBLayer
    {
        private readonly IAsyncDocumentSession DbSession;
        private readonly AppSignInManager signInManager;
        private readonly CustomUserManager<A2ZClientCredentials, IDocumentStore> userManager;

        public A2ZClientDBLayer(IAsyncDocumentSession _DbSession, CustomUserManager<A2ZClientCredentials, IDocumentStore> _userManager,
            AppSignInManager _signInManager)
        {
            DbSession = _DbSession;
            this.userManager = _userManager;
            this.signInManager = _signInManager;

        }

        public bool CheckClientExists(string username)
        {

            List<A2ZClientCredentials> creds = DbSession.Query<A2ZClientCredentials>().Where(x => x.UserName == username).ToListAsync().GetAwaiter().GetResult();
            if (creds != null && creds.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }


        }
        public string RegisterClient(A2ZClientRegistrationModel clientModel)
        {
            A2ZClientCredentials cred = new A2ZClientCredentials
            {
                UserName = clientModel.Username,
                Email = clientModel.ContactEmail,
                UserPermissionType = "Client",
                CreateDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            A2ZClientDetails det = new A2ZClientDetails
            {
                //ClientCredId = clientModel.Id,
                FirmName = clientModel.FirmName,
                FirmAddress = clientModel.FirmAddress,
                ContactPersonName = clientModel.ContactPersonName,
                ContactNo = clientModel.ContactNo,
                ContactEmail = clientModel.ContactEmail,
                GSTNo = clientModel.GSTNo,
                City = clientModel.City,
                State = clientModel.State,
                PinCode = clientModel.PinCode,
                Website = clientModel.Website,
                CreateDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            // Open a Session in synchronous operation mode for cluster-wide transactions

            //SessionOptions options = new SessionOptions
            //{
            //    Database = "A2Zdb",
            //    TransactionMode = TransactionMode.ClusterWide
            //};

            var createUserResult = this.userManager.CreateAsync(cred,clientModel.Password).GetAwaiter().GetResult();
            if (!createUserResult.Succeeded)
            {
                var errorString = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                return errorString;
                //return RedirectToAction("RegisterFailure", new { reason = errorString });
            }

            //DbSession.StoreAsync(cred);
            DbSession.SaveChangesAsync().GetAwaiter().GetResult();
            det.ClientCredId = cred.Id;

            DbSession.StoreAsync(det);
            //DbSession.SaveChangesAsync();

            return "Success";
        }
    }
}
