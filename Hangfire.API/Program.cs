using Hangfire;
using Hangfire.API.BackgroundJobs;
using Hangfire.API.Filters;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.CookiePolicy;
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

    // Hangfire added
    builder.Services.AddHangfire(x =>
    {
        var hangfireConfig = builder.Configuration.GetRequiredSection("Hangfire");
        x.UseSqlServerStorage(hangfireConfig.GetValue<string>("ConnectionString"), new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        });

        // https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        RecurringJob.AddOrUpdate<SimpleJob>(x => x.Execute(CancellationToken.None), hangfireConfig.GetValue<string>("SimpleJobCronSchedule"));
    });
    builder.Services.AddHangfireServer();

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

    var options = new DashboardOptions()
    {
        Authorization = new[]
        {
            new HangfireDashboardAuthorizationFilter()
        }
    };
    app.UseHangfireDashboard("/hangfire", options);

    app.MapControllers();

    app.Run();
}
catch (Exception ex) // catches any exceptions thrown during start-up
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}