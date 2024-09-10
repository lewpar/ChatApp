using System.Net.Sockets;
using System.Text;

namespace ChatApp.Shared;

public static class StreamExtensions
{
    public static async Task<int> ReadIntAsync(this NetworkStream stream)
    {
        var buffer = new byte[sizeof(int)];
        await stream.ReadAtLeastAsync(buffer, sizeof(int));

        return BitConverter.ToInt32(buffer);
    }

    public static async Task<byte[]> ReadBytesAsync(this NetworkStream stream, int length)
    {
        var buffer = new byte[length];
        await stream.ReadAtLeastAsync(buffer, length);

        return buffer;
    }

    public static async Task<string> ReadStringAsync(this NetworkStream stream, int length)
    {
        var buffer = new byte[length];
        await stream.ReadAtLeastAsync(buffer, length);

        return Encoding.UTF8.GetString(buffer);
    }
}
