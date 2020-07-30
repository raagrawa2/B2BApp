using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Raven.Client.Documents.Session;

namespace POCForVivek.Controllers
{
    /// <summary>
    /// A controller that calls DbSession.SaveChangesAsync() when an action finishes executing successfully.
    /// </summary>
    public class RavenController : Controller
    {
        public RavenController(IAsyncDocumentSession dbSession)
        {
            this.DbSession = dbSession;
        }

        public IAsyncDocumentSession DbSession { get; private set; }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next.Invoke();
            if (executedContext.Exception == null)
            {
                await DbSession.SaveChangesAsync();
            }
        }
    }
}