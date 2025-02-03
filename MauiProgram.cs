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

            // For Azure OpenAI using Entra ID
            #region Credential chain
            // Build up credential chain for cloud and local tooling options
            var userAssignedIdentityCredential =
                new ManagedIdentityCredential(builder.Configuration.GetValue<string>("AZURE_CLIENT_ID"));

            var visualStudioCredential = new VisualStudioCredential(
                new VisualStudioCredentialOptions()
                {
                    TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID")
                });

            var azureDevCliCredential = new AzureDeveloperCliCredential(
                new AzureDeveloperCliCredentialOptions()
                {
                    TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID")
                });

            var azureCliCredential = new AzureCliCredential(
                new AzureCliCredentialOptions()
                {
                    TenantId = builder.Configuration.GetValue<string>("AZURE_TENANT_ID")
                });

            var credential = new ApiKeyCredential("test");
            var credential2 = new ChainedTokenCredential(userAssignedIdentityCredential, azureDevCliCredential, visualStudioCredential, azureCliCredential);
            #endregion

            // Use in-memory services in local mode
            if (builder.Configuration["EnvironmentMode"] == "local")
            {
                //builder.Services.AddSingleton<IQueryService, InMemoryQueryService>();
                //builder.Services.AddSingleton<IConnectionService, InMemoryConnectionService>();

                var azureOpenAIEndpoint = new Uri(builder.Configuration["AZURE_OPENAI_ENDPOINT"]);

                // Comment out this AddAzureClients section if you're using vanilla OpenAI instead of Azure OpenAI
                builder.Services.AddAzureClients(async clientBuilder =>
                {
                    clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>(
                        (options, _, _) => new AzureOpenAIClient(
                            azureOpenAIEndpoint, credential, options)); // Replace "credential" with new ApiKeyCredential("your-key") to use key based auth with Azure
                });
            }
            // Use Azure services in hosted mode
            else if (builder.Configuration["EnvironmentMode"] == "hosted")
            {
                var azureOpenAIEndpoint = new Uri(builder.Configuration["AZURE_OPENAI_ENDPOINT"]);
                var azureTableEndpoint = new Uri(builder.Configuration["AZURE_STORAGE_ENDPOINT"]);
                var azureKeyVaultEndpoint = new Uri(builder.Configuration["AZURE_KEYVAULT_ENDPOINT"]);

                builder.Services.AddAzureClients(async clientBuilder =>
                {
                    // Register the table storage and key vault services
                    clientBuilder.AddTableServiceClient(azureTableEndpoint);
                    clientBuilder.AddSecretClient(azureKeyVaultEndpoint);

                    // Comment this AddClient block out if you're using vanilla OpenAI instead of Azure OpenAI
                    clientBuilder.AddClient<AzureOpenAIClient, AzureOpenAIClientOptions>(
                        (options, _, _) => new AzureOpenAIClient(
                            azureOpenAIEndpoint, credential, options)); // Replace "credential" with new ApiKeyCredential("your-key") to use key based auth with Azure

                    clientBuilder.UseCredential(credential2);
                });

                builder.Services.AddScoped<IQueryService, AzureTableQueryService>();
                builder.Services.AddScoped<IConnectionService, AzureKeyVaultConnectionService>();
            }


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddMudServices();
            return builder.Build();
        }
    }
}
