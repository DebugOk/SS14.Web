using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus;
using SS14.ServerHub.Shared.Data;
using SS14.WebEverythingShared;

namespace SS14.ServerHub;

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
        services.AddControllers();

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicies.PolicyHubPublic, builder =>
            {
                builder.AllowAnyOrigin();
            });
        });

        services.AddDbContext<HubDbContext>(options =>
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Must set DefaultConnection");
            options.UseNpgsql(connectionString);
        });

        services.AddHttpClient("ServerStatusCheck",
            client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "SS14.ServerHub/1.0 Status Checker");
                var hubOptions = Configuration.GetSection(HubOptions.Position).Get<HubOptions>() ?? new HubOptions();
                client.MaxResponseContentBufferSize = hubOptions.MaxStatusResponseSize * 1024;
            });

        services.AddOptions<HubOptions>()
            .Bind(Configuration.GetSection(HubOptions.Position));
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // app.UseHttpsRedirection();

        MoreStartupHelpers.AddForwardedSupport(app, Configuration);

        app.UseRouting();

        app.UseCors();

        app.UseHttpMetrics();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapMetrics();
        });
    }
}