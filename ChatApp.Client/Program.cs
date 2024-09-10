using System.Net;

namespace ChatApp.Client;

internal class Program
{
    static async Task Main(string[] args)
    {
        var client = new ChatClient();

        string? username = null;
        while(string.IsNullOrWhiteSpace(username))
        {
            Console.Write("Enter username: ");
            username = Console.ReadLine();
        }

        Console.Clear();

        Console.WriteLine("Connecting..");

        client.Username = username;
        await client.ConnectAsync(IPAddress.Loopback, 55123);

        Console.WriteLine($"Connected as '{username}', you can now start sending messages.");

        while(client.Connected)
        {
            var msg = Console.ReadLine();

            if(!string.IsNullOrWhiteSpace(msg))
            {
                await client.SendMessageAsync(msg);
            }
        }
    }
}
