using Microsoft.AspNetCore.CookiePolicy;
using Quartz;
using Quartz.API.BackgroundJobs;
using Quartz.API.Extensions;
using Serilog;

// Serilog config done as per: https://github.com/datalust/dotnet6-serilog-example
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Quartz: https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/aspnet-core-integration.html
    builder.Services.AddQuartz(x =>
    {
        x.UseMicrosoftDependencyInjectionJobFactory();

        // Register the job, loading the schedule from configuration
        x.AddJobAndTrigger<SimpleJob>(builder.Configuration);
    });

    // ASP.NET Core hosting
    builder.Services.AddQuartzServer(x =>
    {
        // when shutting down we want jobs to complete gracefully
        x.WaitForJobsToComplete = true;
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseExceptionHandler("/error-local-development");
    }
    else
    {
        app.UseExceptionHandler("/error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseCookiePolicy(new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Lax,
        HttpOnly = HttpOnlyPolicy.Always,
        Secure = CookieSecurePolicy.Always
    });

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex) // catches any exceptiosn thrown during start-up
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}