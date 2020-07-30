using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POCForVivek.Common;
using POCForVivek.Common.Identity;
using POCForVivek.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace POCForVivek.Controllers
{
    [AllowAnonymous]
    public class LoginController : RavenController
    {
        private readonly IEmailSender _emailSender;
        private readonly AppSignInManager signInManager;
        private readonly CustomUserManager<A2ZClientCredentials, IDocumentStore> userManager;
        private readonly IHttpContextAccessor _context;


        public LoginController(IEmailSender emailSender, IAsyncDocumentSession dbSession,
            CustomUserManager<A2ZClientCredentials, IDocumentStore> _userManager,
            AppSignInManager _signInManager, IHttpContextAccessor context) : base(dbSession)
        {
            _emailSender = emailSender;
            this.userManager = _userManager;
            this.signInManager = _signInManager;
            _context = context;
        }

        public IActionResult Client()
        {
            return View();
        }

        [Serializable]
        public class dtClientLogin
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        [HttpPost]
        public IActionResult LoginClient([FromBody]dtClientLogin dt)
        {
            A2ZClientCredentials clientModel = new A2ZClientCredentials();
            clientModel.UserName = dt.UserName;
            clientModel.Password = dt.Password;
            var result = signInManager.PasswordSignInAsync(clientModel.UserName, clientModel.Password, true, false).GetAwaiter().GetResult();
            if(result.Succeeded)
            {
                Redirect("/Home/LoggedInUser");
                return Json("Client logged in successfully.");
            }
            else
            {
                return Json("Username or Password given is incorrect.");
            }

        }
    }
}