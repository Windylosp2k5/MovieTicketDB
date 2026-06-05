using System.Web.Mvc;
using System.Web.Routing;

namespace MovieTicketDB.Filters
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var role = filterContext.HttpContext.Session["Role"] as string;
            if (role != "Admin")
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Movie",
                    action = "Login",
                    returnUrl = filterContext.HttpContext.Request.RawUrl
                }));
            }
        }
    }
}
