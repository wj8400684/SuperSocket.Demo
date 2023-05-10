using Core;
using Microsoft.Extensions.Logging;
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
        client.Logger.LogInformation($"添加订单");

        return ValueTask.FromResult(package);
    }
}
