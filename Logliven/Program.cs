#pragma warning disable EXTEXP0018
using Microsoft.EntityFrameworkCore;
using Logliven.Components;
using Logliven.Discord;
using Logliven.Infrastructure.Authentication;
using Logliven.Postgres;
using Logliven.Services.Discord;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.SetupDiscord(builder.Configuration);
builder.Services.SetupDiscordRest();
builder.Services.SetupAuthentication();
builder.Services.AddPooledDbContextFactory<LoglivenDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Logliven")));

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

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Logliven.Client._Imports).Assembly);

await using (var scope = app.Services.CreateAsyncScope()) {
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LoglivenDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.MigrateAsync();
}

app.Run();