using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.Authentication.Passthrough
{
    public class PassthroughAuthHandler : RemoteAuthenticationHandler<PassthroughAuthOptions>
    {
        protected HttpClient Backchannel => Options.Backchannel;

        private OAuthEvents OAuthEvents => (OAuthEvents) Events;

        public PassthroughAuthHandler(
            IOptionsMonitor<PassthroughAuthOptions> optionsAccessor,
            ILoggerFactory                          loggerFactory,
            UrlEncoder                              encoder,
            ISystemClock                            clock)
            : base(optionsAccessor, loggerFactory, encoder, clock)
        {
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            if (string.IsNullOrEmpty(properties.RedirectUri))
            {
                properties.RedirectUri = OriginalPathBase + OriginalPath + Request.QueryString;
            }

            var    dict  = new Dictionary<string, string>();
            string value = Options.StateDataFormat.Protect(properties);
            dict.Add("state", value);

            var url = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, dict);

            var authorizationEndpoint = BuildRedirectUri(url);
            var redirectContext = new RedirectContext<OAuthOptions>(
                Context, Scheme, Options,
                properties, authorizationEndpoint);

            await OAuthEvents.RedirectToAuthorizationEndpoint(redirectContext);
        }

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var protectedText = Request.Query["state"].ToArray().FirstOrDefault();
            var properties    = Options.StateDataFormat.Unprotect(protectedText);

            // simulator token return from oauth server
            var tokenObject = Options.PassthroughTokenHandler?.Invoke() ?? new
            {
                token_type    = "bearer",
                access_token  = "1234567890",
                refresh_token = "0987654321",
                expires_in    = "3600",
                user_id       = "nepton_h23f620G2Xw2812aM3a9Z",
                screen_name   = "nepton",
            };

            var token = OAuthTokenResponse.Success(JsonDocument.Parse(JsonSerializer.Serialize(tokenObject)));
            if (token.Error != null)
                return HandleRequestResult.Fail(token.Error, properties);

            var identity = new ClaimsIdentity(ClaimsIssuer);
            var context = new OAuthCreatingTicketContext(
                new ClaimsPrincipal(identity),
                properties,
                Context,
                Scheme,
                Options,
                Backchannel,
                token,
                token.Response.RootElement);
            context.RunClaimActions();

            await OAuthEvents.CreatingTicket(context);

            var ticket = new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
            return HandleRequestResult.Success(ticket);
        }
    }
}
