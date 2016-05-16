using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication.Controllers
{
    public class BaseController : Controller
    {
        protected static string ErrorPage = "~/Error"; protected static string NotFoundPage = "~/NotFoundPage";
        protected static string LoginPage = "~/Login";
        public RedirectResult RedirectToNotFoundPage => Redirect(NotFoundPage);
        public RedirectResult RedirectToLoginPage => Redirect(LoginPage);

        protected override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);
            filterContext.Result = Redirect(ErrorPage);
        }
    }
}
