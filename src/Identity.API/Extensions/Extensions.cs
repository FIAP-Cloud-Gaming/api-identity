using EventBusRabbitMQ;

namespace Identity.API.Extensions;

internal static class Extensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediator(assembly);

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));

        services.AddScoped(typeof(IPipelineBehavior<>), typeof(TransactionBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(LoggingBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehavior<>));
        services.AddScoped(typeof(IPipelineBehavior<>), typeof(IdempotencyBehavior<>));

        services.AddScoped<IIdentityIntegrationEventService, IdentityIntegrationEventService>();

        return services;
    }

    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IdentityDb")
            ?? throw new InvalidOperationException("IdentityDb connection string not found");

        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddNpgSql(
                connectionString,
                name: "identity-db",
                tags: ["db", "postgres", "ready"]);

        return services;
    }

    public static IHostApplicationBuilder ConfigureIdentitySerilog(this IHostApplicationBuilder builder)
    {
        var lokiUri = builder.Configuration["Loki:Uri"] ?? "http://loki:3100";

        builder.Services.AddSerilog(loggerConfiguration =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "Identity.API")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}")
                .WriteTo.GrafanaLoki(lokiUri, labels:
                [
                    new LokiLabel { Key = "app", Value = "identity-api" },
                    new LokiLabel { Key = "environment", Value = builder.Environment.EnvironmentName }
                ]);
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureEventBus(this IHostApplicationBuilder builder)
    {
        builder.AddRabbitMqEventBus(builder.Configuration.GetConnectionString("RabbitMQ")!, "identity-service")
        .AddSubscription<PaymentSucceededIntegrationEvent, PaymentSucceededIntegrationEventHandler>();

        return builder;
    }
}
