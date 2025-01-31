using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Logliven.Common;

public static class ExtensionMethods
{
    public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) => transform(input);
    public static async Task<TOut> PipeAsync<TIn, TOut>(this Task<TIn> input, Func<TIn, TOut> transform) => transform(await input);
    public static async Task<TOut> PipeAsync<TIn, TOut>(this Task<TIn> input, Func<TIn, Task<TOut>> transform) => await transform(await input);
    public static async Task PipeAsync<TIn>(this Task<TIn> input, Action<TIn> transform) => transform(await input);
    public static async ValueTask<TOut> PipeAsync<TIn, TOut>(this ValueTask<TIn> input, Func<TIn, TOut> transform) => transform(await input);
    public static async ValueTask<TOut> PipeAsync<TIn, TOut>(this ValueTask<TIn> input, Func<TIn, Task<TOut>> transform) => await transform(await input);

    public static IServiceCollection ConfigureFromSection<TOptions>(this IServiceCollection serviceCollection, IConfiguration configuration, string? sectionName = null) 
        where TOptions : class {
        sectionName ??= typeof(TOptions).Name;
        serviceCollection.Configure<TOptions>(configuration.GetSection(sectionName));
        serviceCollection.AddSingleton<TOptions>(provider => provider.GetRequiredService<IOptions<TOptions>>().Value);

        return serviceCollection;
    }
}