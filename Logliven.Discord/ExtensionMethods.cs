using Discord.WebSocket;
using Logliven.Common;
using Logliven.Discord.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logliven.Discord;

public static class ExtensionMethods {
    public static IServiceCollection SetupDiscord(this IServiceCollection services, IConfiguration configuration) {
        services.ConfigureFromSection<DiscordSocketConfig>(configuration);
        services.AddSingleton<DiscordBotClient>();
        services.AddSingleton<DiscordSocketClient>(provider => provider.GetRequiredService<DiscordBotClient>());
        services.AddHostedService<DiscordClientBackgroundService>();
        services.AddHostedService<DiscordClientReliabilityBackgroundService>();
        services.AddHostedService<DiscordClientGuildListenerService>();

        return services;
    }
}