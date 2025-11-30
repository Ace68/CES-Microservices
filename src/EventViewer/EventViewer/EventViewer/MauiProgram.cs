using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using EventViewer.Services;

namespace EventViewer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            
            // Read appsettings.json
            var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            var config = new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true)
                .Build();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            
            // Register configuration and HttpClient
            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddScoped(sp => new HttpClient());
            builder.Services.AddScoped<IEventService, EventService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
