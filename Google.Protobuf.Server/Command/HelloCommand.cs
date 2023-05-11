using Core;
using Server.Command.Abstractions;

namespace Server.Command;

[RequestCommand(typeof(CommandHello))]
public sealed class HelloCommand : RequestAsyncCommand<CommandHello>
{
    protected override ValueTask OnHandlerAsync(
        CommandSession session,
        CommandPackage package,
        CommandHello request,
        CancellationToken cancellationToken)
    {
        session.LogInformation(request.Msg);

        return ValueTask.CompletedTask;
    }
}