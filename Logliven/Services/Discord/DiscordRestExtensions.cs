namespace Logliven.Services.Discord;

public static class DiscordRestExtensions {
    public static IServiceCollection SetupDiscordRest(this IServiceCollection services) {
        services.AddSingleton<DiscordRestClientAccessor>();
        services.AddSingleton<DiscordRestClientService>();

        return services;
    }
}