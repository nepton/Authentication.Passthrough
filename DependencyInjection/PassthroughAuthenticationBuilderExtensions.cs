using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace AspNetCore.Authentication.Passthrough.DependencyInjection
{
    public static class PassthroughAuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddPassthroughTest(this AuthenticationBuilder builder)
            => builder.AddPassthroughTest(PassthroughDefaults.AuthenticationScheme, PassthroughDefaults.DisplayName, _ =>
            {
            });

        public static AuthenticationBuilder AddPassthroughTest(this AuthenticationBuilder builder, Action<PassthroughAuthOptions> setupAction)
            => builder.AddPassthroughTest(PassthroughDefaults.AuthenticationScheme, PassthroughDefaults.DisplayName, setupAction);

        public static AuthenticationBuilder AddPassthroughTest(this AuthenticationBuilder builder, string authenticationScheme, Action<PassthroughAuthOptions> setupAction)
            => builder.AddPassthroughTest(authenticationScheme, PassthroughDefaults.DisplayName, setupAction);

        public static AuthenticationBuilder AddPassthroughTest(
            this AuthenticationBuilder     builder,
            string                         authenticationScheme,
            string                         displayName,
            Action<PassthroughAuthOptions> setupAction)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<PassthroughAuthOptions>, PassthroughPostConfigureOptions>());
            return builder.AddRemoteScheme<PassthroughAuthOptions, PassthroughAuthHandler>(authenticationScheme, displayName, setupAction);
        }
    }
}
