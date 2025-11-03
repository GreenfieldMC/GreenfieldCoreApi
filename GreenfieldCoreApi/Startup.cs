using System.Diagnostics;
using System.Net;
using Asp.Versioning;
using GreenfieldCoreApi.Transformers;
using GreenfieldCoreDataAccess.Database.Repositories;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.ScriptManager;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Commands;
using GreenfieldCoreServices.Models.Clients;
using GreenfieldCoreServices.Services;
using GreenfieldCoreServices.Services.Caching;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace GreenfieldCoreApi;

public static class Startup
{
    
    internal static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            // .WriteTo.Console()
            // .WriteTo.Debug()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();
        
        builder.Host.UseSerilog(Log.Logger);
    }
    
    internal static async Task PerformDatabaseMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scriptManager = scope.ServiceProvider.GetRequiredService<IScriptManager>();
        await scriptManager.ApplyPendingScripts(CancellationToken.None);
    }
    
    internal static void ConfigureDatabases(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITransactionScope, TransactionScope>();
        services.AddTransient<IScriptManager, ScriptManager>();
        services.AddTransient<IClientRepository, ClientRepository>();
        services.AddTransient<IUserRepository, UserRepository>();
    }
    
    internal static void ConfigureServices(this IServiceCollection services)
    {
        services.AddLogging(builder => builder.AddConsole());
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IClientAuthService, ClientAuthService>();
    }

    internal static void ConfigureCaching(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService<Guid, Client>, ClientCacheService>();
    }

    internal static void ConfigureConfiguration(this IConfigurationBuilder configBuilder, IWebHostEnvironment env)
    {
        configBuilder.SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
            .AddJsonFile($"connectionstrings.{env.EnvironmentName}.json", optional: false)
            .AddJsonFile($"jwtsettings.{env.EnvironmentName}.json", optional: false)
            .AddEnvironmentVariables();
    }
    
    internal static void ConfigureCommandServices(this IServiceCollection services)
    {
        services.AddHostedService<CommandProcessService>();
        services.AddKeyedTransient<ICommand, ClientCommand>("client");
    }
    
    internal static void ConfigureAuthentication(this IServiceCollection services, ConfigurationManager configurationManager)
    {
        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    ValidIssuer = configurationManager.GetValue<string>("jwtSettings:issuer"),
                    ValidAudience = configurationManager.GetValue<string>("jwtSettings:audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configurationManager.GetValue<string>("jwtSettings:key")!))
                };
            });
        services.AddScoped<IClaimsTransformation, RoleClaimTransformer>();
        services.AddMvc();
    }
    
    internal static void ConfigureWebServices(this IServiceCollection services)
    {
        services.AddApiVersioning(x =>
        {
            x.DefaultApiVersion = new ApiVersion(1, 0);
            x.AssumeDefaultVersionWhenUnspecified = true;
            x.ReportApiVersions = true;
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
        services.AddOpenApi(opt =>
        {
            opt.AddDocumentTransformer<ClientCredentialsTransformer>();
            opt.AddDocumentTransformer<TitleTransformer>();
        });
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllers();
        services.AddOpenApi();
    }

    internal static void ConfigureWebApplication(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        
        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        };
        
        var knownProxies = ParseKnownProxies(logger, app.Configuration["known-proxies"]);
        knownProxies.ForEach(forwardedHeadersOptions.KnownProxies.Add);
        logger.LogInformation("Configured {KnownProxiesCount} known proxies: {Join}", knownProxies.Count, string.Join(", ", knownProxies));
        
        app.UseForwardedHeaders(forwardedHeadersOptions);
        
        app.MapOpenApi();
        app.MapScalarApiReference(options => options
            .WithLayout(ScalarLayout.Classic)
            .WithTitle("Greenfield Core API (" + app.Environment.EnvironmentName + ")")
            .AddPreferredSecuritySchemes("OAuth2")
            .AddClientCredentialsFlow("OAuth2", flow =>
                {
                    flow.TokenUrl = "/api/v1.0/login/token";
                })
            .WithPersistentAuthentication()
        );
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHttpsRedirection();

        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var problemFactory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var globalLogger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                
                var problemDetails = problemFactory.CreateProblemDetails(
                    context,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "An internal server error has occurred!",
                    detail: "A technical error occurred in the server. Please contact support with the trace ID.",
                    instance: exceptionHandlerPathFeature?.Path
                );
                problemDetails.Extensions["traceId"] = traceId;
                globalLogger.LogError("An unhandled exception occurred. Trace ID: {TraceId}", traceId);

                context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
        
        app.MapControllers();
    }
    
    private static List<IPAddress> ParseKnownProxies(ILogger logger, string? proxyString)
    {
        if (string.IsNullOrEmpty(proxyString)) return [];
        return proxyString.Split(';', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => 
                        {
                            if (IPAddress.TryParse(p, out var ipAddress)) return ipAddress;
                            logger.LogWarning("Invalid IP address format for known proxy: {Proxy}", p);
                            return null;
                        })
                        .Where(ip => ip is not null)
                        .ToList()!;
            
    }
    
}