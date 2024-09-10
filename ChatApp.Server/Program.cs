using System.Net;

namespace ChatApp.Server;

internal class Program
{
    static async Task Main(string[] args)
    {
        var server = new ChatServer(IPAddress.Parse("0.0.0.0"), 55123);
        await server.StartAsync();
    }
}
