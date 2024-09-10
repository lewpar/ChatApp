using ChatApp.Shared;

using System.Net;
using System.Net.Sockets;

namespace ChatApp.Server;

public class ChatServer
{
    private List<TcpClient> clients;

    private CancellationToken cancelToken;
    private CancellationTokenSource cancelTokenSource;

    private TcpListener listener;

    private Dictionary<OpCodes, Func<NetworkStream, Task>> handlers;

    public ChatServer(IPAddress address, int port)
    {
        clients = new List<TcpClient>();
        listener = new TcpListener(address, port);

        cancelTokenSource = new CancellationTokenSource();
        cancelToken = cancelTokenSource.Token;

        handlers = new Dictionary<OpCodes, Func<NetworkStream, Task>>()
        {
            { OpCodes.Message, HandleMessageAsync }
        };
    }

    public async Task StartAsync()
    {
        Console.WriteLine("Starting server..");
        listener.Start();
        Console.WriteLine("Started.");

        Console.WriteLine("Listening for clients..");
        await ListenForClientsAsync();
    }

    private async Task ListenForClientsAsync()
    {
        while (!cancelTokenSource.IsCancellationRequested)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            Console.WriteLine($"Client '{client.Client.RemoteEndPoint}' connected.");

            var stream = client.GetStream();
            clients.Add(client);

            while(!cancelToken.IsCancellationRequested)
            {
                var opCode = (OpCodes)await stream.ReadIntAsync();

                if(!handlers.ContainsKey(opCode))
                {
                    Console.WriteLine($"Client '{client.Client.RemoteEndPoint}' sent invalid opcode '{opCode}'.");
                    return;
                }

                await handlers[opCode].Invoke(client.GetStream());
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{ex.StackTrace}");
        }
    }

    private async Task HandleMessageAsync(NetworkStream stream)
    {
        int nameLen = await stream.ReadIntAsync();
        string name = await stream.ReadStringAsync(nameLen);

        int messageLen = await stream.ReadIntAsync();
        string message = await stream.ReadStringAsync(messageLen);

        Console.WriteLine($"Received message '{message}' from '{name}'.");

        // Repeat the packet to all connected clients
        foreach(var client in clients)
        {
            NetworkStream targetStream = client.GetStream();
            await targetStream.WriteAsync(new MessagePacket(name, message).Serialize());
        }
    }

    public async Task StopAsync()
    {
        await cancelTokenSource.CancelAsync();
    }
}
