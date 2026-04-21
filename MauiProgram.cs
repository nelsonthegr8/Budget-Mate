using CommunityToolkit.Maui;
using Financial_ForeCast.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using System.ClientModel;

namespace Financial_ForeCast
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            
            // Load appsettings.json for configuration
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            builder
                .UseMauiApp<App>()
                // Initialize the .NET MAUI Community Toolkit
                .UseMauiCommunityToolkit()
                // Configure fonts
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Database Service Registration - Unified service with connection factory pattern
            // Same business logic works for both local SQLite and server databases
            builder.Services.AddSingleton<DbServiceFactory>();
            builder.Services.AddScoped<IDbService>(factory => 
                factory.GetRequiredService<DbServiceFactory>().GetService());

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}
