#pragma warning disable EXTEXP0018
using System.Reflection;
using Logliven.Client.Services;
using Logliven.Common;
using Microsoft.EntityFrameworkCore;
using Logliven.Components;
using Logliven.Discord;
using Logliven.Infrastructure.Authentication;
using Logliven.Infrastructure.Exceptions;
using Logliven.Postgres;
using Logliven.Services;
using Logliven.Services.Discord;
using Scalar.AspNetCore;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

var builder = WebApplication.CreateBuilder(args)
    .AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();
builder.Services.AddSlimMessageBus(b => {
    b.WithProviderMemory()
        .AutoDeclareFrom(Assembly.GetExecutingAssembly(), typeof(DiscordBotClient).Assembly);
});

builder.Services.SetupDiscord(builder.Configuration);
builder.Services.SetupDiscordRest();
builder.Services.SetupAuthentication();

builder.Services.AddPooledDbContextFactory<LoglivenDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("LoglivenDB")));
builder.EnrichNpgsqlDbContext<LoglivenDbContext>();

builder.Services.AddScoped<IGuildService, GuildService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseWebAssemblyDebugging();
}
else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapOpenApi();
app.MapScalarApiReference()
    .RequireAuthorization();

app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Logliven.Client._Imports).Assembly);

app.UseExceptionHandler(new ExceptionHandlerOptions() {
    StatusCodeSelector = ex => ex switch {
        NotFoundException => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status500InternalServerError
    }
});

await using (var scope = app.Services.CreateAsyncScope()) {
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LoglivenDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

app.Run();