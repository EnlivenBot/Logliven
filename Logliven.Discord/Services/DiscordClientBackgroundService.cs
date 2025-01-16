using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Logliven.Discord.Services;

public class DiscordClientBackgroundService : BackgroundService
{
    private readonly DiscordSocketClient _discordSocketClient;
    private readonly IConfiguration _configuration;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly InteractionService _interactionService;
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public DiscordClientBackgroundService(DiscordSocketClient discordSocketClient, IConfiguration configuration,
        ILoggerFactory loggerFactory, IHostApplicationLifetime hostApplicationLifetime, IServiceProvider serviceProvider)
    {
        _discordSocketClient = discordSocketClient;
        _configuration = configuration;
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _logger = loggerFactory.CreateLogger("Discord.Net");
        _discordSocketClient.Log += DiscordSocketClientOnLog;
        _discordSocketClient.Ready += DiscordSocketClientOnShardReady;

        _interactionService = new InteractionService(_discordSocketClient.Rest,
            new InteractionServiceConfig() { LogLevel = LogSeverity.Debug});
        _interactionService.Log += DiscordSocketClientOnLog;
        _discordSocketClient.InteractionCreated += async interaction =>
        {
            await _interactionService.ExecuteCommandAsync(new SocketInteractionContext(_discordSocketClient, interaction), _serviceProvider);
        };
    }

    private async Task DiscordSocketClientOnShardReady()
    {
        await _interactionService.AddModulesAsync(typeof(DiscordClientBackgroundService).Assembly, _serviceProvider);
        await _interactionService.RegisterCommandsGloballyAsync();
    }

    private Task DiscordSocketClientOnLog(LogMessage arg)
    {
        var logLevel = arg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => throw new ArgumentOutOfRangeException()
        };

        _logger.Log(logLevel, arg.Exception, "[{Source}] {Message}", arg.Source, arg.Message);

        return Task.CompletedTask;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = _configuration.GetValue<string>("DiscordBotToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogCritical("Discord bot token is empty. Shutdown");
            _hostApplicationLifetime.StopApplication();
        }

        await _discordSocketClient.LoginAsync(TokenType.Bot, token);
        await _discordSocketClient.StartAsync();
        
        await base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}