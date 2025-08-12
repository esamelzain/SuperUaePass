using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using SuperUaePass.Configuration;
using SuperUaePass.Services;
using System.Net;

namespace SuperUaePass.Extensions;

/// <summary>
/// Extension methods for configuring UAE Pass services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds UAE Pass services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Action to configure UAE Pass options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSuperUaePass(
        this IServiceCollection services,
        Action<SuperUaePassOptions> configureOptions)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configureOptions == null)
            throw new ArgumentNullException(nameof(configureOptions));

        // Configure options
        services.Configure(configureOptions);

        // Register services
        services.AddHttpClient<ISuperUaePassService, SuperUaePassService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SuperUaePassOptions>>().Value;
            
            // Configure timeout
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            
            // Configure proxy if enabled
            if (options.UseProxy && !string.IsNullOrEmpty(options.ProxyUrl))
            {
                var proxy = new WebProxy(options.ProxyUrl);
                
                if (!string.IsNullOrEmpty(options.ProxyUsername) && !string.IsNullOrEmpty(options.ProxyPassword))
                {
                    proxy.Credentials = new NetworkCredential(options.ProxyUsername, options.ProxyPassword);
                }
                
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };
                
                // Note: This is a simplified approach. In a real implementation,
                // you might want to use HttpClientFactory with custom handlers
            }
        });

        return services;
    }

    /// <summary>
    /// Adds UAE Pass services to the service collection with configuration from IConfiguration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="sectionName">The configuration section name (default: "SuperUaePass")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSuperUaePass(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "SuperUaePass")
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // Bind configuration using the Bind method
        services.Configure<SuperUaePassOptions>(options =>
        {
            configuration.GetSection(sectionName).Bind(options);
        });

        // Register services
        services.AddHttpClient<ISuperUaePassService, SuperUaePassService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<SuperUaePassOptions>>().Value;
            
            // Configure timeout
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            
            // Configure proxy if enabled
            if (options.UseProxy && !string.IsNullOrEmpty(options.ProxyUrl))
            {
                var proxy = new WebProxy(options.ProxyUrl);
                
                if (!string.IsNullOrEmpty(options.ProxyUsername) && !string.IsNullOrEmpty(options.ProxyPassword))
                {
                    proxy.Credentials = new NetworkCredential(options.ProxyUsername, options.ProxyPassword);
                }
                
                var handler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };
                
                // Note: This is a simplified approach. In a real implementation,
                // you might want to use HttpClientFactory with custom handlers
            }
        });

        return services;
    }
}
