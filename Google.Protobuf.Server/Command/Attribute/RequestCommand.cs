using Core;
using SuperSocket.Command;

namespace Server.Command;

public sealed class RequestCommand : CommandAttribute
{
    public RequestCommand(Type packageType)
    {
        Key = packageType.Name;
    }
}