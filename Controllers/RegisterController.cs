using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using POCForVivek.Common;
using POCForVivek.Common.Identity;
using POCForVivek.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace POCForVivek.Controllers
{
    [AllowAnonymous]
    public class RegisterController : RavenController
    {
        private readonly IEmailSender _emailSender;
        private readonly AppSignInManager signInManager;
        private readonly CustomUserManager<A2ZClientCredentials, IDocumentStore> userManager;

        public RegisterController(IEmailSender emailSender, IAsyncDocumentSession dbSession,
            CustomUserManager<A2ZClientCredentials,IDocumentStore> _userManager,
            AppSignInManager _signInManager) : base(dbSession)
        {
            _emailSender = emailSender;
            this.userManager = _userManager;
            this.signInManager = _signInManager;
        }

        public IActionResult Client()
        {
            return View();
        }


        public ActionResult CheckIfClientExists(string username)
        {
            A2ZClientDBLayer dBLayer = new A2ZClientDBLayer(DbSession, userManager, signInManager);
            bool result = dBLayer.CheckClientExists(username);
            return Json(result);
        }


        [HttpPost]
        public IActionResult AddClient(A2ZClientRegistrationModel clientModel)
        {
            if(ModelState.IsValid)
            {
                A2ZClientDBLayer dBLayer = new A2ZClientDBLayer(DbSession,userManager,signInManager);
                string message = dBLayer.RegisterClient(clientModel);
                if (message == "Success")
                {
                    _emailSender.SendEmailAsync(clientModel.ContactEmail, "A2Z Project Client Registration Confirmation Mail", $"Hi,<br />Your client registration on A2Z project is successfull.<br/>Regards<br />A2Z Team").GetAwaiter().GetResult();
                    return View("_Success");
                }
                else
                {
                    return View("_Failure");
                }
            }
            return View("Client",clientModel);
        }
    }
}