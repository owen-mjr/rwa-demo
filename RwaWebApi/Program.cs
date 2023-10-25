using Microsoft.IdentityModel.Logging;

namespace RwaWebApi
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Logging.AddConsole();

            if (builder.Environment.IsDevelopment())
            {
                IdentityModelEventSource.ShowPII = true;
            }
            var startup = new Startup(builder.Configuration);
            startup.ConfigureServices(builder.Services, builder.Environment);
            var app = builder.Build();
            startup.Configure(app, app.Environment);
            app.Run();
        }
    }
}