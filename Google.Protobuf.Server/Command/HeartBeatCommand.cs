using Core;
using RpcCore;
using Server.Command.Abstractions;

namespace Server.Command;

[ReplyCommand(CommandType.HeartBeat, CommandType.HeartBeatReply)]
public sealed class HeartBeatCommand : ReplyAsyncCommand<CommandHeartBeat, CommandHeartBeatReply>
{
    protected override ValueTask<CommandHeartBeatReply?> OnHandlerAsync(
        CommandSession session, 
        CommandPackage package, 
        CommandHeartBeat request,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<CommandHeartBeatReply?>(new CommandHeartBeatReply
        {
            ServerTimesteamp = DateTime.Now.ToTimeStamp13()
        });
    }
}