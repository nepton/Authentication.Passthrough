namespace AspNetCore.Authentication.Passthrough
{
    public static class PassthroughDefaults
    {
        public const string AuthenticationScheme = "Passthrough";

        public const string DisplayName = "Passthrough For Test";

        public const string ClaimsIssuer = "Passthrough";

        public const string CallbackPath = "/signin-passthrough";

        public const string AuthorizationEndpoint = "/signin-passthrough";
    }
}
