using System;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Authentication.Passthrough
{
    public class PassthroughAuthOptions : OAuthOptions
    {
        public string RefreshTokenEndpoint { get; set; }

        public string ValidateTokenEndpoint { get; set; }


        /// <summary>
        /// 模拟的Token
        /// </summary>
        public Func<object> PassthroughTokenHandler
        {
            get;
            set;
        }

        public PassthroughAuthOptions()
        {
            CallbackPath          = new PathString(PassthroughDefaults.CallbackPath);
            AuthorizationEndpoint = PassthroughDefaults.AuthorizationEndpoint;

            ClaimsIssuer = PassthroughDefaults.ClaimsIssuer;

            ClientId     = "passthrough";
            ClientSecret = "passthrough";
        }
    }
}
