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

    public static DiscordAuthedUserInfo GetDiscordUserInfoOrThrow(this HttpContext context) {
        var discordAuthedUserInfo = GetDiscordUserInfo(context);
        if (discordAuthedUserInfo is null) {
            throw new InvalidOperationException("User not authenticated");
        }

        return discordAuthedUserInfo;
    }

    public static DiscordAuthedUserInfo? GetDiscordUserInfo(this HttpContext context) {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Retrieve claims using their URI values.
        var idClaim = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        var nameClaim = context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
        var avatarClaim = context.User.FindFirst("urn:discord:avatar");

        if (idClaim == null || nameClaim == null)
        {
            return null;
        }

        // Parse the id claim.
        if (!ulong.TryParse(idClaim.Value, out var userId))
        {
            return null;
        }

        // Return the parsed user information.
        return new DiscordAuthedUserInfo(userId, nameClaim.Value, avatarClaim?.Value ?? string.Empty);
    }
}