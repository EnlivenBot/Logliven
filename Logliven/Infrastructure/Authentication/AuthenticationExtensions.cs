using Discord.OAuth2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace Logliven.Infrastructure.Authentication;

public static class AuthenticationExtensions {
    public static IServiceCollection SetupAuthentication(this IServiceCollection services) {
        services.AddCascadingAuthenticationState();
        
        services.AddAuthentication(options => {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
            })
            .AddDiscord();

        services.AddSingleton<IPostConfigureOptions<DiscordOptions>, DiscordAuthOptionsPostConfiguration>();

        return services;
    }
}