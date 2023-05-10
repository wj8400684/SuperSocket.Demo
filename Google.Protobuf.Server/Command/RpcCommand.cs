using Core;
using Server.Command.Abstractions;

namespace Server.Command.Ack;

[RequestCommand(CommandType.RpcReply)]
public sealed class RpcCommand : RequestAsyncCommand
{
    protected override ValueTask OnHandlerAsync(
        CommandSession session,
        CommandPackage package,
        CancellationToken cancellationToken)
    {
        return session.TryDispatchAsync(package);
    }
}