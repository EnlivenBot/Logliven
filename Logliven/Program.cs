using Microsoft.EntityFrameworkCore;
using Logliven.Components;
using Logliven.Discord;
using Logliven.Postgres;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDiscord(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();


builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
    })
    .AddDiscord(options => {
        options.AppId = builder.Configuration["Discord:AppId"];
        options.AppSecret = builder.Configuration["Discord:AppSecret"];

        options.Scope.Add("guilds");
    });

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