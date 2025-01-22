using Discord.WebSocket;
using Logliven.Common;
using Logliven.Discord.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logliven.Discord;

public static class ExtensionMethods {
    public static IServiceCollection SetupDiscord(this IServiceCollection services, IConfiguration configuration) {
        services.ConfigureFromSection<DiscordSocketConfig>(configuration);
        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<DiscordClientBackgroundService>();
        services.AddSingleton<DiscordClientReliabilityBackgroundService>();
        services.AddSingleton<DiscordClientGuildListenerService>();

        return services;
    }
}