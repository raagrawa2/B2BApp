using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using POCForVivek.Common.CustomAttributes;
using POCForVivek.Models;

namespace POCForVivek.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _context;


        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor context)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        //[AllowAnonymous]
        [Authorize(Roles = "Client")]
        public IActionResult LoggedInUser()
        {
            var ticket = AuthenticationHttpContextExtensions.AuthenticateAsync(_context.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme).GetAwaiter().GetResult();
            //var identity = ticket != null && ticket.Principal != null ? ticket.Ticket.Principal : null;

            //ClaimsPrincipal principal = _context.HttpContext.User;
            A2ZClientCredentials cred = new A2ZClientCredentials();
            //cred.UserName = principal.Claims.Where(x => x.Type == "Name" ).FirstOrDefault().Value;
            cred.UserName = ticket.Principal.Identity.Name;

            return View(cred);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
