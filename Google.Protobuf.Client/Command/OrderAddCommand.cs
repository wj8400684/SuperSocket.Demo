using Core;
using Server.Command;
using Server.Command.Abstractions;

namespace Google.Protobuf.Client.Command;

[ReplyCommand(CommandType.AddOrder, CommandType.AddOrderReply)]
public sealed class OrderAddCommand : RpcAsyncCommand<CommandOrder>
{
    protected override ValueTask<CommandPackage> OnHandlerAsync(
        RpcClient client, 
        CommandPackage package, 
        CommandOrder request)
    {
        package.SuccessFul = true;

        return ValueTask.FromResult(package);
    }
}
