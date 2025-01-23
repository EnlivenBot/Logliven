using System.Collections.Concurrent;
using System.Diagnostics;
using Discord;
using Discord.Rest;
using Microsoft.AspNetCore.Authentication;

namespace Logliven.Services.Discord;

public class DiscordRestClientAccessor(IHttpContextAccessor httpContextAccessor) {
    private readonly ConcurrentDictionary<string, RestClientContainer> _cache = new();

    public async Task<DiscordRestClient> GetRestClientFromAccessToken() {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HTTP context");
        if (httpContext.User.Identity is not { IsAuthenticated: true } or { Name: null } ) {
            throw new InvalidOperationException("User not authenticated");
        }
        
        var identity = httpContext.User.Identity;
        var token = await httpContext.GetTokenAsync("Discord", "access_token");
        Debug.Assert(token is not null);
        
        var container = _cache.GetOrAdd(identity.Name, _ => new RestClientContainer());

        await container.Semaphore.WaitAsync();
        try {
            if (container.Token != token) {
                await (container.Client?.LogoutAsync() ?? Task.CompletedTask);
                await (container.Client?.DisposeAsync() ?? ValueTask.CompletedTask);
                container.Client = new DiscordRestClient();
                await container.Client.LoginAsync(TokenType.Bearer, token);
                
                container.Token = token;
            }
        }
        finally {
            container.Semaphore.Release();
        }

        return container.Client!;
    }

    private class RestClientContainer {
        public SemaphoreSlim Semaphore { get; } = new(1);
        public DiscordRestClient? Client { get; set; } = new();
        public string? Token { get; set; }
    }
}