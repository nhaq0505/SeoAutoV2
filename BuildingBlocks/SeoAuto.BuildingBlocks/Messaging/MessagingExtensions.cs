using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SeoAuto.BuildingBlocks.Messaging;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessageBroker(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly? assembly = null)
    {
        // 1. Đọc thông số cấu hình RabbitMQ từ appsettings.json (hoặc lấy giá trị mặc định)
        var host = configuration["MessageBroker:Host"] ?? "localhost";
        var username = configuration["MessageBroker:UserName"] ?? "guest";
        var password = configuration["MessageBroker:Password"] ?? "guest";

        // 2. Cấu hình MassTransit
        services.AddMassTransit(x =>
        {
            // Tự động đặt tên Queue dạng kebab-case (vd: audit-requested-event)
            x.SetKebabCaseEndpointNameFormatter();

            // Nếu có truyền Assembly, tự động đăng ký tất cả Consumer tìm thấy
            if (assembly != null)
            {
                x.AddConsumers(assembly);
            }

            // Cấu hình kết nối tới RabbitMQ
            x.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Tự động cấu hình các endpoint/queue dựa trên các Consumer đã đăng ký
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}