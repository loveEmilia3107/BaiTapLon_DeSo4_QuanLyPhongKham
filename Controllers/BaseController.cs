using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace QuanLyPhongKham.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"]?.ToString();
            var actionName = context.RouteData.Values["action"]?.ToString();

            // stackoverflow block
            if (string.Equals(controllerName, "TaiKhoan", StringComparison.OrdinalIgnoreCase) &&
               (string.Equals(actionName, "Login", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionName, "Register", StringComparison.OrdinalIgnoreCase)))
            {
                base.OnActionExecuting(context);
                return;
            }

            var accId = context.HttpContext.Session.GetInt32("AccID");

            if (accId == null)
            {
                context.Result = new RedirectToActionResult("Login", "TaiKhoan", null);
                return;
            }

            base.OnActionExecuting(context);
        }

        protected int CurrentAccId => HttpContext.Session.GetInt32("AccID") ?? 0;

        // BenhNhan = 1, Admin = 0
        protected int CurrentRole => HttpContext.Session.GetInt32("Role") ?? 1;

        // if isAdmin, role = 0
        protected bool IsAdmin => CurrentRole == 0;
    }
}
