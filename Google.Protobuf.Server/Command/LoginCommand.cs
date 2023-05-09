using Server.Command.Abstractions;
using Core;

namespace Server.Command;

[ReplyCommand(CommandType.Login, CommandType.LoginReply)]
public sealed class LoginCommand : ReplyAsyncCommand<CommandLogin, CommandLoginReply>
{
    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    protected override ValueTask<CommandLoginReply?> OnHandlerAsync(
        CommandSession session,
        CommandPackage package,
        CommandLogin request,
        CancellationToken cancellationToken)
    {
        PackageHostServer.PackageCount++;
        
        return new ValueTask<CommandLoginReply?>(new CommandLoginReply
        {
            Token = Guid.NewGuid().ToString("X")
        });
    }

    /// <summary>
    /// 如果当前命令比较耗时
    /// 可以选择自定义线程池调度当前命令
    /// 这样不会影响其他命令执行
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected override ValueTask SchedulerAsync(CommandSession session, CommandPackage package,
        CancellationToken cancellationToken)
    {
        return base.SchedulerAsync(session, package, cancellationToken);
    }

    /// <summary>
    /// 当执行命令异常时候触发
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="request"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    protected override ValueTask OnHandlerErrorAsync(
        CommandSession session,
        CommandPackage package,
        CommandLogin request,
        Exception e,
        CancellationToken cancellationToken)
    {
        return base.OnHandlerErrorAsync(session, package, request, e, cancellationToken);
    }

    /// <summary>
    /// 当解析包异常时触发
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    protected override ValueTask OnDecoderErrorAsync(
        CommandSession session,
        CommandPackage package,
        Exception e,
        CancellationToken cancellationToken)
    {
        return base.OnDecoderErrorAsync(session, package, e, cancellationToken);
    }
}