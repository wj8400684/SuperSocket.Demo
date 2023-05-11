using Core;
using SuperSocket.Command;

namespace Server.Command;

public sealed class ReplyCommand : CommandAttribute
{
    public string Reply { get; private set; }

    public ReplyCommand(Type requestType, Type replyType)
    {
        Key = requestType.Name;
        Reply = replyType.Name;
    }
}