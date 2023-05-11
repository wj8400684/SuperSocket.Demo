using Core;
using Server.Command.Abstractions;

namespace Server.Command.Ack;

[RequestCommand(CommandType.RpcReply)]
public sealed class RpcCommand : RequestAsyncCommand
{
    protected override async ValueTask OnHandlerAsync(
        CommandSession session,
        CommandPackage package,
        CancellationToken cancellationToken)
    {
        await session.TryDispatchAsync(package);
    }
}