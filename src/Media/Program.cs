// See https://aka.ms/new-console-template for more information

using DSharpPlus;
using Media.Clients.Impl;
using Media.EventBus;
using Media.Logging;
using Media.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VkNet;

IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.development.json")
                .Build();

var services = new ServiceCollection()
    .AddSingleton(sp => configuration)
    .AddSingleton(sp => LoggerFactory.Create(builder => builder.AddConsole()))
    .AddSingleton(sp => LoggerFactory.Create(builder => builder.AddRabbitLogger(configuration =>
    {
        configuration.Exchange = "logs";
        configuration.Bus = Rabbit.CreateBus("localhost");
    })))
    .AddScoped(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<VkApi>())
    .AddScoped(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<Discord>())
    .AddScoped(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<Vk>())
    .AddScoped(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<Media.Clients.Impl.Telegram>())
    .AddSingleton(sp => Rabbit.CreateBus("localhost"))
    .AddSingleton<DiscordClient>()
    .AddSingleton<Discord>()
    .AddSingleton(sp => new VkApi(sp.GetRequiredService<ILogger<VkApi>>()))
    .AddSingleton<Vk>()
    .AddSingleton(sp => new Media.Clients.Impl.Telegram(configuration, sp.GetRequiredService<ILogger<Media.Clients.Impl.Telegram>>()))
    .BuildServiceProvider();

IBus eventBus = services.GetRequiredService<IBus>();

static async Task SendToExchange(IBus amqp, Message message)
{
    await amqp.SendExchangeAsync("media", message, $"media.receive.{message.MediaService}");
}

var discord = await Task.Factory.StartNew(async () =>
{
    var discord = services.GetRequiredService<Discord>();
    var amqp = services.GetRequiredService<IBus>();

    await discord.Subscribe(async msg =>
    {
        await SendToExchange(amqp, msg);
    });

    await eventBus.ReceiveAsync<Message>("mediaDiscord", async message =>
    {
        if (message == null) return;
        await discord.Publish(message.Channel, message);
    },
    exchange: "media",
    routingKey: "media.send.discord");

    await discord.Start();
});

var vk = await Task.Factory.StartNew(async () =>
{
    var vk = services.GetRequiredService<Vk>();
    var amqp = services.GetRequiredService<IBus>();

    await vk.Subscribe(async msg =>
    {
        await SendToExchange(amqp, msg);
    });
    await eventBus.ReceiveAsync<Message>("mediaVk", async message =>
    {
        if (message == null) return;
        await vk.Publish(message.Channel, message);
    },
    exchange: "media",
    routingKey: "media.send.vk");

    await vk.Start();
});

var tg = await Task.Factory.StartNew(async () =>
{
    var tg = services.GetRequiredService<Media.Clients.Impl.Telegram>();
    var amqp = services.GetRequiredService<IBus>();

    await tg.Subscribe(async msg =>
    {
        await SendToExchange(amqp, msg);
    });
    await eventBus.ReceiveAsync<Message>("mediaTelegram", async message =>
    {
        if (message == null) return;
        await tg.Publish(message.Channel, message);
    },
    exchange: "media",
    routingKey: "media.send.telegram");

    await tg.Start();
});

Task.WaitAll(tg, discord, vk);
await Task.Delay(-1);

