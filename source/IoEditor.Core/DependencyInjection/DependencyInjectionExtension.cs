using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using IoEditor.Models.Studio;

using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class DependencyInjectionExtension
    {
        public static void AddIoEditorServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<StudioOptions>(configuration.GetSection("StudioOptions"));

            // image handling
            services.AddSingleton<BackgroundPartImageLoader>();
            services.AddSingleton<IPartImageLoader>(provider => provider.GetRequiredService<BackgroundPartImageLoader>());
            
            services.AddSingleton<PartImageCache>();
            services.AddSingleton<IPartImageProxyFactory, PartImageProxyFactory>();

            // base data
            services.AddSingleton<PartLibrary>();
            services.AddSingleton<ColorLibrary>();
        }
    }
}