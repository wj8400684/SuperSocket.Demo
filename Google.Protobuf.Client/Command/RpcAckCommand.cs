using Core;
using Server.Command;

namespace Google.Protobuf.Client.Command;

[RequestCommand(CommandType.RpcReply)]
public sealed class RpcAckCommand : RequestCommandAsync
{
    protected override ValueTask OnHandlerAsync(RpcClient client, CommandPackage package)
    {
        return client.TryDispatchAsync(package);
    }
}
