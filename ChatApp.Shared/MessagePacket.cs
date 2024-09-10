using System.Text;

namespace ChatApp.Shared;

public class MessagePacket
{
    private string name;
    private string message;

    public MessagePacket(string name, string message)
    {
        this.name = name;
        this.message = message;
    }

    public byte[] Serialize()
    {
        byte[] nameBuffer = Encoding.UTF8.GetBytes(name);
        int nameLen = nameBuffer.Length;

        byte[] msgBuffer = Encoding.UTF8.GetBytes(message);
        int msgLen = msgBuffer.Length;

        var ms = new MemoryStream();

        ms.Write(BitConverter.GetBytes((int)OpCodes.Message));

        ms.Write(BitConverter.GetBytes(nameLen));
        ms.Write(nameBuffer);

        ms.Write(BitConverter.GetBytes(msgLen));
        ms.Write(msgBuffer);

        return ms.ToArray();
    }
}
