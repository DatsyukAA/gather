// See https://aka.ms/new-console-template for more information
using DotaKit;

Console.WriteLine("Hello, World!");

string username = args[0];
string password = args[1];

DotaClient client = new(username, password);

// connect
client.Connect();

client.onConnected += (callback) =>
{
    Console.WriteLine("Connected! Logging '{0}' into Steam...", client.username);
};
client.onDisconnected += (callback) =>
{
    Console.WriteLine("Disconnected from Steam, reconnecting...");
};

client.Wait();


