
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Monitoring.SharedKernel.SharedExtensions
{
    public static class OpenTelemetryConfig
    {
        public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
        {
            builder.ConfigureOpenTelemetry();
            return builder;
        }

        public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
        {
            var sericeName = builder.Configuration["ServiceName"];
            if (string.IsNullOrEmpty(sericeName))
            {
                throw new Exception("Service must have a name.");
            }
            builder.Logging.AddOpenTelemetry(x =>
            {
                x.IncludeScopes = true;
                x.IncludeFormattedMessage = true;
            });

            builder.Services.AddOpenTelemetry().WithTracing(opt =>
            {
                opt
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(sericeName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSqlClientInstrumentation()
                .AddConsoleExporter()
                //.AddOtlpExporter()
                .AddZipkinExporter()
                .AddJaegerExporter();

            });

            builder.Services.AddOpenTelemetry().WithMetrics(opt =>
            {
                opt
                .AddRuntimeInstrumentation().AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel", "System.Net.Http");
            });

            var useOtlpExporter = !string.IsNullOrEmpty(builder.Configuration["OTLP:Endpoint"]);

            if (useOtlpExporter)
            {
                builder.Services.Configure<OpenTelemetryLoggerOptions>(opt => opt.AddOtlpExporter());
                builder.Services.ConfigureOpenTelemetryMeterProvider(opt => opt.AddOtlpExporter());
                builder.Services.ConfigureOpenTelemetryTracerProvider(opt => opt.AddOtlpExporter());
            }
            return builder;
        }

    }
}
