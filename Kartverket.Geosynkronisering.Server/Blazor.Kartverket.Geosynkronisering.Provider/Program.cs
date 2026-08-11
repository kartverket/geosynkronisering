using Blazor.Kartverket.Geosynkronisering.Provider;
using Blazor.Kartverket.Geosynkronisering.Provider.Data;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor.Services;
using Pusher;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddMudServices();

//builder.Services.AddScoped<FeedbackController.Progress>();
builder.Services.AddSingleton<FeedbackController.Progress>();

builder.Services.AddHttpClient();

var app = builder.Build();


// Log with Serilog 
var config = app.Configuration;    
//var section = config.GetSection("SerilogCustom");
var serilogPath = config["SerilogCustom:filepath"];

// #162 : Web-pusher Logs folder should be below application and not in %TEMP%
if (false)
{
    if (serilogPath.ToUpper() == "%TEMP%")
    {
        serilogPath = Path.GetTempPath();
    }
}
var basePath = AppDomain.CurrentDomain.BaseDirectory;
serilogPath = basePath + "logs/";

var logFile = Path.Combine(serilogPath, config["SerilogCustom:logfile"]);
var errorFile = Path.Combine(serilogPath, config["SerilogCustom:errorfile"]);
var warningFile = Path.Combine(serilogPath, config["SerilogCustom:warningfile"]);
var debugFile = Path.Combine(serilogPath, config["SerilogCustom:debugfile"]);
var fatalFile = Path.Combine(serilogPath, config["SerilogCustom:fatalfile"]);


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Information).WriteTo.File(logFile, rollingInterval: RollingInterval.Day, shared: true))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Debug).WriteTo.File(debugFile, rollingInterval: RollingInterval.Day, shared: true))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Error).WriteTo.File(errorFile, rollingInterval: RollingInterval.Day, shared: true))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Warning).WriteTo.File(warningFile, shared: true))
    .WriteTo.Logger(l => l.Filter.ByIncludingOnly(ev => ev.Level == LogEventLevel.Fatal).WriteTo.File(fatalFile, rollingInterval: RollingInterval.Day, shared: true))
    .CreateLogger();

Log.Information("Starting Geosync Provider Blazor (.NET 6.0)!");


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

// Lars
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = new PathString("/revisionlog")
});

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapDefaultControllerRoute();
//});

app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
