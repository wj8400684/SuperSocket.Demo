using Core;
using SuperSocket.Command;

namespace Server.Command;

public sealed class ReplyCommand : CommandAttribute
{
    public CommandType Reply { get; private set; }

    public ReplyCommand(CommandType request, CommandType reply)
    {
        Key = (int)request;
        Reply = reply;
    }
}