# How to use

1. Add follow code in startup.
2. Map token key to your claim

> enjoy it :)
> 
```C#
services.AddAuthentication().AddPassthroughTest(options =>
{
    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

    // provider token for test
    options.PassthroughTokenHandler = () => new
    {
        token_type    = "bearer",
        access_token  = "1234567890",
        refresh_token = "0987654321",
        expires_in    = "3600",
        user_id       = "nepton_h23f620G2Xw2812aM3a9Z",
        screen_name   = "nepton",
    };

    // map token to your claim
    options.ClaimActions.MapJsonKey(ClaimTypes.Name,       "screen_name");
    options.ClaimActions.MapJsonKey(JwtClaimTypes.Subject, "user_id");
});
```