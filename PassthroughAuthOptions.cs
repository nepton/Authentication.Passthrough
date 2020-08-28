using System.Security.Claims;
using Authentication.WeChat.MediaPlatform;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Authentication.Passthrough
{
    public class PassthroughAuthOptions : OAuthOptions
    {
        public string RefreshTokenEndpoint { get; set; }

        public string ValidateTokenEndpoint { get; set; }

        public PassthroughAuthOptions()
        {
            CallbackPath          = new PathString(PassthroughDefaults.CallbackPath);
            AuthorizationEndpoint = PassthroughDefaults.AuthorizationEndpoint;

            ClaimsIssuer = PassthroughDefaults.ClaimsIssuer;

            ClaimActions.MapJsonKey(ClaimTypes.Name, "screen_name");
            ClaimActions.MapJsonKey("sub",           "user_id");
        }
    }
}
