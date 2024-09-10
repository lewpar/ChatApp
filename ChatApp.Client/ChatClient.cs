using ChatApp.Shared;

using System.Net;
using System.Net.Sockets;

namespace ChatApp.Client;

public class ChatClient
{
    public string Username { get; set; }
    public bool Connected { get; private set; }

    private TcpClient client;

    private Dictionary<OpCodes, Func<NetworkStream, Task>> handlers;

    public ChatClient()
    {
        client = new TcpClient();
        Username = "Anonymous";

        handlers = new Dictionary<OpCodes, Func<NetworkStream, Task>>()
        {
            { OpCodes.Message, HandleMessageAsync }
        };
    }

    public async Task ConnectAsync(IPAddress ipAddress, int port)
    {
        try
        {
            await client.ConnectAsync(ipAddress, port);
            Connected = true;

            _ = ListenForDataAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
        }
    }

    public async Task ListenForDataAsync()
    {
        while(Connected)
        {
            var stream = client.GetStream();

            var opCode = (OpCodes)await stream.ReadIntAsync();

            if (!handlers.ContainsKey(opCode))
            {
                Console.WriteLine($"Server '{client.Client.RemoteEndPoint}' sent invalid opcode '{opCode}'.");
                return;
            }

            await handlers[opCode].Invoke(client.GetStream());
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if(!Connected)
        {
            return;
        }

        NetworkStream stream = client.GetStream();
        await stream.WriteAsync(new MessagePacket(Username, message).Serialize());
    }

    private async Task HandleMessageAsync(NetworkStream stream)
    {
        int nameLen = await stream.ReadIntAsync();
        string name = await stream.ReadStringAsync(nameLen);

        int messageLen = await stream.ReadIntAsync();
        string message = await stream.ReadStringAsync(messageLen);

        Console.WriteLine($"[{name}]: {message}");
    }
}
