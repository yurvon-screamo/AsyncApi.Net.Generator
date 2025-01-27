using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using AsyncApi.Net.Generator;
using AsyncApi.Net.Generator.AsyncApiSchema.v2;

namespace StreetlightsAPI;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging => logging.AddSimpleConsole(console => console.SingleLine = true))
            .ConfigureWebHostDefaults(web =>
            {
                web.UseStartup<Startup>();
                web.UseUrls("http://localhost:5000");
            });
    }
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add AsyncApi.Net.Generator to the application services. 
        services.AddAsyncApiSchemaGeneration(options =>
        {
            options.AssemblyMarkerTypes = new[] { typeof(StreetlightMessageBus) };

            options.Middleware.UiTitle = "Streetlights API";

            options.AsyncApi = new AsyncApiDocument
            {
                Info = new Info
                {
                    Title = "Streetlights API",
                    Description = "The Smartylighting Streetlights API allows you to remotely manage the city lights.",
                    License = new License
                    {
                        Name = "Apache 2.0",
                        Url = "https://www.apache.org/licenses/LICENSE-2.0"
                    }
                },
                Servers = new()
                {
                    ["mosquitto"] = new Server
                    {
                        Url = "test.mosquitto.org",
                        Protocol = "mqtt",
                    },
                    ["webapi"] = new Server
                    {
                        Url = "localhost:5000",
                        Protocol = "http",
                    },
                },
            };
        });

        services.ConfigureNamedAsyncApi("Foo", asyncApi =>
        {
            asyncApi.Info = new Info()
            {
                Version = "1.0.0",
                Title = "Foo",
            };
            asyncApi.Servers = new()
            {
                ["mosquitto"] = new Server
                {
                    Url = "test.mosquitto.org",
                    Protocol = "mqtt",
                },
                ["webapi"] = new Server
                {
                    Url = "localhost:5000",
                    Protocol = "http",
                },
            };
        });

        services.AddScoped<IStreetlightMessageBus, StreetlightMessageBus>();
        services.AddControllers();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app)
    {
        app.UseDeveloperExceptionPage();

        app.UseRouting();
        app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod());

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapAsyncApiDocuments();
            endpoints.MapAsyncApiUi();

            endpoints.MapControllers();
        });

        // Print the AsyncAPI doc location
        ILogger<Program> logger = app.ApplicationServices.GetService<ILoggerFactory>().CreateLogger<Program>();
        System.Collections.Generic.ICollection<string> addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses;

        logger.LogInformation("AsyncAPI doc available at: {URL}", $"{addresses.FirstOrDefault()}/asyncapi/asyncapi.json");
        logger.LogInformation("AsyncAPI UI available at: {URL}", $"{addresses.FirstOrDefault()}/asyncapi/ui/");
    }
}
