using Core;
using SuperSocket.Client.Command;

namespace Server.Command;

public sealed class RequestCommand : CommandAttribute
{
    public RequestCommand(CommandType request)
    {
        Key = (int)request;
    }
}