using System.Reflection;
using Core;
using Google.Protobuf;
using SuperSocket.Command;

namespace Server.Command.Abstractions;

/// <summary>
/// 带有回复的异步命令执行器
/// </summary>
/// <typeparam name="TRequestPackage"></typeparam>
/// <typeparam name="TReplyPackage"></typeparam>
public abstract class RpcAsyncCommand<TRequestPackage, TReplyPackage>
    : IAsyncCommand<CommandSession, CommandPackage>
    where TRequestPackage : IMessage<TRequestPackage>
    where TReplyPackage : IMessage<TReplyPackage>
{
    private readonly CommandType _replyCommand;

    private readonly MessageParser<TRequestPackage> _requestParser =
        new(() => Activator.CreateInstance<TRequestPackage>()!);

    protected RpcAsyncCommand()
    {
        var result = this.GetType().GetCustomAttribute<ReplyCommand>();

        if (result == null)
            return;

        _replyCommand = result.Reply;
    }

    ValueTask IAsyncCommand<CommandSession, CommandPackage>.
        ExecuteAsync(CommandSession session, CommandPackage package) =>
        SchedulerAsync(session, package, session.ConnectionToken);

    /// <summary>
    /// 执行包命令
    /// </summary>
    /// <param name="session">连接回话</param>
    /// <param name="package">完整包</param>
    /// <param name="request">具体请求内容</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected abstract ValueTask<TReplyPackage?> OnHandlerAsync(
        CommandSession session,
        CommandPackage package,
        TRequestPackage request,
        CancellationToken cancellationToken);

    /// <summary>
    /// 执行调度
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    protected virtual async ValueTask SchedulerAsync(
        CommandSession session,
        CommandPackage package,
        CancellationToken cancellationToken)
    {
        await ProcessPackageAsync(session, package, cancellationToken);
    }

    /// <summary>
    /// 当执行命令异常时触发
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="request"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async ValueTask OnHandlerErrorAsync(
        CommandSession session,
        CommandPackage package,
        TRequestPackage request,
        Exception e,
        CancellationToken cancellationToken)
    {
        await session.SendPackageAsync(package.CreateError(_replyCommand, ErrorCode.Unknown, "发生未知错误，请稍后重试"));
    }

    /// <summary>
    /// 当解包发生错误时候触发
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="e"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async ValueTask OnDecoderErrorAsync(
        CommandSession session,
        CommandPackage package,
        Exception e,
        CancellationToken cancellationToken)
    {
        await session.SendPackageAsync(package.CreateError(_replyCommand, ErrorCode.Package, "解析封包失败，请稍后重试"));
    }

    /// <summary>
    /// 解码包
    /// </summary>
    /// <param name="session"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<TRequestPackage> OnDecoderPackageAsync(
        CommandSession session,
        ByteString value,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(_requestParser.ParseFrom(value));
    }

    /// <summary>
    /// 编码包
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual ValueTask<ByteString> OnEncoderPackageAsync(
        CommandSession session,
        TReplyPackage package,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(package.ToByteString());
    }

    /// <summary>
    /// 进行解包
    /// 执行包命令
    /// 编码响应包
    /// 发送响应包
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    private async ValueTask ProcessPackageAsync(
        CommandSession session,
        CommandPackage package,
        CancellationToken cancellationToken)
    {
        var request = await TryDecoderPackageAsync(session, package, cancellationToken);

        if (request == null)
            return;

        TReplyPackage? replyPackage;

        try
        {
            replyPackage = await OnHandlerAsync(session, package, request, cancellationToken);
        }
        catch (Exception e)
        {
            await OnHandlerErrorAsync(session, package, request, e, cancellationToken);
            return;
        }

        if (replyPackage == null)
            return;

        var replyContent = await OnEncoderPackageAsync(session, replyPackage, cancellationToken);

        var replyCommand = package.CreateRpcInfo(_replyCommand, replyContent);
        
        await session.SendPackageAsync(replyCommand);
    }

    /// <summary>
    /// 解析包
    /// </summary>
    /// <param name="session"></param>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async ValueTask<TRequestPackage?> TryDecoderPackageAsync(
        CommandSession session,
        CommandPackage package,
        CancellationToken cancellationToken)
    {
        try
        {
            return await OnDecoderPackageAsync(session, package.Content, cancellationToken);
        }
        catch (Exception e)
        {
            await OnDecoderErrorAsync(session, package, e, cancellationToken);
        }

        return default;
    }
}