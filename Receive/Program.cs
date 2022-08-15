using System.Text;
using DLL.Entities;
using DLL.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


using IHost host = Host.CreateDefaultBuilder(args).ConfigureAppConfiguration(app =>
{
    app.AddJsonFile("appsettings.json");
})
    .ConfigureServices((_, services) =>
            services.AddSingleton<UsersService>()
            .Configure<UserStoreDatabaseSettings>(_.Configuration.GetSection("UserStoreDatabase")))
    .Build();


var factory = new ConnectionFactory() { HostName = "localhost" };
using (var connection = factory.CreateConnection())
using (var channel = connection.CreateModel())
{
    channel.QueueDeclare(queue: "MyQueue",
                         durable: false,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += async (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);


        Console.WriteLine(" [x] Received {0}", message);

        IServiceProvider provider = host.Services;
        UsersService usersService = provider.GetRequiredService<UsersService>();
        await usersService.CreateAsync(new User() { Name = message });
    };
    channel.BasicConsume(queue: "MyQueue",
                         autoAck: true,
                         consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}



