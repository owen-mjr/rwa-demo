using RwaWebApi.Services.Interfaces;
using RwaWebApi.Services;
using RwaWebApi.Data;
using RwaWebApi.Data.Interfaces;
using RwaWebApi.Infrastructure;

namespace RwaWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
        {
            // Add services to the container.
            services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddSingleton<IConcertRepository, ListConcertRepository>();
            services.AddScoped<ITicketManagementService, MockTicketManagementService>();

            services.AddHealthChecks();
            services.AddScoped<ApplicationInitializer, ApplicationInitializer>();

            if (env.IsDevelopment())
            {
                services.AddCors(options =>
                {
                    options.AddDefaultPolicy(builder =>
                    {
                        builder.SetIsOriginAllowed(origin => new Uri(origin).IsLoopback);
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    });
                });
            }
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseCors();

            }
            using var serviceScope = app.Services.CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<ApplicationInitializer>().Initialize();

            app.UseRetryTestingMiddleware();

            app.MapControllers();
            app.MapHealthChecks("/healthz");
        }
    }
}
