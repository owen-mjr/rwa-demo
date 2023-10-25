using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace RwaWeb;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddBlazorise(options =>
        {
            options.Immediate = true;
        })
        .AddBootstrapProviders()
        .AddFontAwesomeIcons();


        var startup = new Startup(builder.Configuration);
        startup.ConfigureServices(builder.Services);

        await builder.Build().RunAsync();
    }
}
