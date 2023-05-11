using Core;
using Server.Command.Abstractions;

namespace Server.Command.Ack;

[RequestCommand(typeof(CommandRpcReply))]
public sealed class RpcAckCommand : RequestAsyncCommand
{
    protected override ValueTask OnHandlerAsync(
        CommandSession session,
        CommandPackage package,
        CancellationToken cancellationToken)
    {
        return session.TryDispatchAsync(package);
    }
}