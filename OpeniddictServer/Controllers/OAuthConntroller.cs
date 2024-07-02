using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using static OpenIddict.Client.WebIntegration.OpenIddictClientWebIntegrationConstants;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OpeniddictServer.Data;

namespace OpeniddictServer.Controllers
{
    public class OAuthConntroller : Controller
    {

        [HttpGet, HttpPost, Route("~/oauth/callback/github")]
        public async Task<IActionResult> Github([FromServices]SignInManager<ApplicationUser> signInManager)
        {
            // Resolve the claims extracted by OpenIddict from the userinfo response returned by GitHub.
            var result = await HttpContext.AuthenticateAsync(Providers.GitHub);

            var identity = new ClaimsIdentity(
                authenticationType: "ExternalLogin",
                nameType: ClaimTypes.Name,
                roleType: ClaimTypes.Role);

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, result.Principal!.FindFirst("id")!.Value));

            var properties = new AuthenticationProperties
            {
                RedirectUri = result.Properties!.RedirectUri
            };
            return SignIn(new ClaimsPrincipal(identity), properties);
        }
       
    }
}
