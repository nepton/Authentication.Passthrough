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

namespace Authentication.Passthrough
{
    internal class PassthroughAuthHandler : RemoteAuthenticationHandler<PassthroughAuthOptions>
    {
        protected HttpClient Backchannel => Options.Backchannel;

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring. 
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        private OAuthEvents OAuthEvents => (OAuthEvents) Events;

        //protected const string CorrelationPrefix = ".AspNetCore.Correlation.";
        protected const string CorrelationProperty = ".xsrf";

        private const string CorrelationMarker = "N";
        //protected const string AuthSchemeKey = ".AuthScheme";

        //protected static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        public PassthroughAuthHandler(
            IOptionsMonitor<PassthroughAuthOptions> optionsAccessor,
            ILoggerFactory                          loggerFactory,
            UrlEncoder                              encoder,
            ISystemClock                            clock)
            : base(optionsAccessor, loggerFactory, encoder, clock)
        {
        }

        protected virtual string FormatScope(IEnumerable<string> scopes)
            => string.Join(",", scopes); // // OAuth2 3.3 space separated, but weixin not

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
            var protectedText = this.Request.Query["state"].ToArray().FirstOrDefault();
            var properties = Options.StateDataFormat.Unprotect(protectedText);

            var demoToken = new
            {
                access_token  = "1234567890",
                token_type    = "bearer",
                refresh_token = "0987654321",
                expires_in    = "3600",
                user_id       = "nepton_h23fG2Xw2812aM3a9Z",
                screen_name   = "nepton",
                sub           = "hello",
            };

            var tokens = OAuthTokenResponse.Success(JsonDocument.Parse(JsonSerializer.Serialize(demoToken)));
            if (tokens.Error != null)
                return HandleRequestResult.Fail(tokens.Error, properties);

            if (string.IsNullOrEmpty(tokens.AccessToken))
                return HandleRequestResult.Fail("Failed to retrieve access token.", properties);

            var identity = new ClaimsIdentity(ClaimsIssuer);

            var context = new OAuthCreatingTicketContext(new ClaimsPrincipal(identity), properties, Context, Scheme, Options, Backchannel, tokens, tokens.Response.RootElement);
            context.RunClaimActions();

            await OAuthEvents.CreatingTicket(context);
            var ticket = new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);

            return HandleRequestResult.Success(ticket);
        }
    }
}
