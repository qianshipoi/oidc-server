using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OpeniddictServer.ViewModels.Clients;

using System.Web;

namespace OpeniddictServer.Controllers
{
    public class ClientsController : Controller
    {
        [Authorize, HttpGet, Route("~/wpf-browser-auth-client-callback")]
        public IActionResult Index()
        {
            // get state and code from query string
            var state = Request.Query["state"];
            var code = Request.Query["code"];

            if(string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(code))
            {
                return View("Error", "请求不合法：'state'或'code'参数缺失。");
            }

            var url = $"openiddict-wpf-browser-auth-client://signin{HttpUtility.UrlDecode(Request.QueryString.Value)}";

            return View(new WpfClientAuthViewModel { Url = url });
        }

        [HttpGet, Route("~/wpf-browser-auth-client-signout-callback")]
        public IActionResult WpfClientSignout()
        {
            var url = $"openiddict-wpf-browser-auth-client://signout{HttpUtility.UrlDecode(Request.QueryString.Value)}";
            return View(new WpfClientAuthViewModel { Url = url });
        }
    }
}
