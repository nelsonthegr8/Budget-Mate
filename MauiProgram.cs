using CommunityToolkit.Maui;
using Financial_ForeCast.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using System.ClientModel;
using Microsoft.Extensions.Configuration;

namespace Financial_ForeCast
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                // Initialize the .NET MAUI Community Toolkit by adding the below line of code
                .UseMauiCommunityToolkit()
                // After initializing the .NET MAUI Community Toolkit, optionally add additional fonts
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<LocalDbService>();
            builder.Services.AddMauiBlazorWebView();

            // Uncomment for vanilla OpenAI (non azure)
            // builder.Services.AddScoped<OpenAIClient>(factory =>
            // {
            //     return new OpenAIClient(new ApiKeyCredential("your-openai-key"));
            // });
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}
