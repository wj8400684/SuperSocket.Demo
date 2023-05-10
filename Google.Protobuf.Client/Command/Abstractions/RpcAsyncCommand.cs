using Core;
using Google.Protobuf;
using Google.Protobuf.Client;
using SuperSocket.Client.Command;

namespace Server.Command.Abstractions;

/// <summary>
/// 带有回复的异步命令执行器
/// </summary>
/// <typeparam name="TRequestPackage"></typeparam>
public abstract class RpcAsyncCommand<TRequestPackage>
    : IAsyncCommand<CommandPackage>
    where TRequestPackage : IMessage<TRequestPackage>
{
    private readonly CommandType _replyCommand;

    private readonly MessageParser<TRequestPackage> _requestParser =
        new(() => Activator.CreateInstance<TRequestPackage>()!);

    ValueTask IAsyncCommand<CommandPackage>.ExecuteAsync(object sender, CommandPackage package) => OnSchedulerAsync((RpcClient)sender, package);

    /// <summary>
    /// 执行包命令
    /// </summary>
    /// <param name="client">连接回话</param>
    /// <param name="package">完整包</param>
    /// <param name="request">具体请求内容</param>
    /// <returns></returns>
    protected abstract ValueTask<CommandPackage> OnHandlerAsync(
        RpcClient client,
        CommandPackage package,
        TRequestPackage request);

    /// <summary>
    /// 执行调度
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    protected async virtual ValueTask OnSchedulerAsync(RpcClient client, CommandPackage package)
    {
        await TryProcessPackageAsync(client, package);
    }

    /// <summary>
    /// 解码包
    /// </summary>
    /// <param name="client"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual ValueTask<TRequestPackage> OnDecoderPackageAsync(
        RpcClient client,
        ByteString value)
    {
        return ValueTask.FromResult(_requestParser.ParseFrom(value));
    }

    /// <summary>
    /// 当解包发生错误时候触发
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual ValueTask OnDecoderErrorAsync(
        RpcClient client,
        CommandPackage package,
        Exception e)
    {
        var replyCommand = package.CreateRpcError(_replyCommand, ErrorCode.Package, "解析封包失败，请稍后重试");

        return client.SendAsync(replyCommand);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <param name="request"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual ValueTask OnHandlerErrorAsync(
        RpcClient client,
        CommandPackage package,
        TRequestPackage request,
        Exception e)
    {
        var replyCommand = package.CreateRpcError(_replyCommand, ErrorCode.Unknown, "发生未知错误，请稍后重试");

        return client.SendAsync(replyCommand);
    }

    /// <summary>
    /// 进行解包
    /// 执行包命令
    /// 编码响应包
    /// 发送响应包
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    private async ValueTask TryProcessPackageAsync(
        RpcClient client,
        CommandPackage package)
    {
        var request = await TryDecoderPackageAsync(client, package);

        if (request == null)
            return;

        CommandPackage replyCommand;

        try
        {
            replyCommand = await OnHandlerAsync(client, package, request);
        }
        catch (Exception e)
        {
            await OnHandlerErrorAsync(client, package, request, e);
            return;
        }

        if (replyCommand == null)
            return;

        replyCommand.Key = CommandType.RpcReply;
        replyCommand.RpcKey = _replyCommand;
        replyCommand.Content = ByteString.Empty;

        await client.SendAsync(replyCommand);
    }

    /// <summary>
    /// 解析包如果解析失败则按照返回状态是否选择断开连接
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    private async ValueTask<TRequestPackage?> TryDecoderPackageAsync(
        RpcClient client,
        CommandPackage package)
    {
        try
        {
            return await OnDecoderPackageAsync(client, package.Content);
        }
        catch (Exception e)
        {
            await OnDecoderErrorAsync(client, package, e);
        }

        return default;
    }
}

/// <summary>
/// 带有回复的异步命令执行器
/// </summary>
/// <typeparam name="TRequestPackage"></typeparam>
/// <typeparam name="TReplyPackage"></typeparam>
public abstract class RpcAsyncCommand<TRequestPackage, TReplyPackage>
    : IAsyncCommand<CommandPackage>
    where TRequestPackage : IMessage<TRequestPackage>
    where TReplyPackage : IMessage<TReplyPackage>
{
    private readonly CommandType _replyCommand;

    private readonly MessageParser<TRequestPackage> _requestParser =
        new(() => Activator.CreateInstance<TRequestPackage>()!);

    ValueTask IAsyncCommand<CommandPackage>.ExecuteAsync(object sender, CommandPackage package) => OnSchedulerAsync((RpcClient)sender, package);

    /// <summary>
    /// 执行包命令
    /// </summary>
    /// <param name="client">连接回话</param>
    /// <param name="package">完整包</param>
    /// <param name="request">具体请求内容</param>
    /// <returns></returns>
    protected abstract ValueTask<TReplyPackage?> OnHandlerAsync(
        RpcClient client,
        CommandPackage package,
        TRequestPackage request);

    /// <summary>
    /// 执行调度
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    protected async virtual ValueTask OnSchedulerAsync(RpcClient client, CommandPackage package)
    {
        await TryProcessPackageAsync(client, package);
    }

    /// <summary>
    /// 编码包
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    protected virtual ValueTask<ByteString> OnEncoderPackageAsync(
        RpcClient client,
        TReplyPackage package)
    {
        return ValueTask.FromResult(package.ToByteString());
    }

    /// <summary>
    /// 解码包
    /// </summary>
    /// <param name="client"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual ValueTask<TRequestPackage> OnDecoderPackageAsync(
        RpcClient client,
        ByteString value)
    {
        return ValueTask.FromResult(_requestParser.ParseFrom(value));
    }

    /// <summary>
    /// 当解包发生错误时候触发
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual ValueTask OnDecoderErrorAsync(
        RpcClient client,
        CommandPackage package,
        Exception e)
    {
        return client.SendAsync(package.CreateRpcError(_replyCommand, ErrorCode.Package, "解析封包失败，请稍后重试"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <param name="request"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual ValueTask OnHandlerErrorAsync(
        RpcClient client,
        CommandPackage package,
        TRequestPackage request,
        Exception e)
    {
        return client.SendAsync(package.CreateRpcError(_replyCommand, ErrorCode.Unknown, "发生未知错误，请稍后重试"));
    }

    /// <summary>
    /// 进行解包
    /// 执行包命令
    /// 编码响应包
    /// 发送响应包
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    private async ValueTask TryProcessPackageAsync(
        RpcClient client,
        CommandPackage package)
    {
        var request = await TryDecoderPackageAsync(client, package);

        if (request == null)
            return;

        TReplyPackage? replyPackage;

        try
        {
            replyPackage = await OnHandlerAsync(client, package, request);
        }
        catch (Exception e)
        {
            await OnHandlerErrorAsync(client, package, request, e);
            return;
        }

        if (replyPackage == null)
            return;

        var replyContent = await OnEncoderPackageAsync(client, replyPackage);

        var replyCommand = package.CreateRpcInfo(_replyCommand, replyContent);

        await client.SendAsync(replyCommand);
    }

    /// <summary>
    /// 解析包如果解析失败则按照返回状态是否选择断开连接
    /// </summary>
    /// <param name="client"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    private async ValueTask<TRequestPackage?> TryDecoderPackageAsync(
        RpcClient client,
        CommandPackage package)
    {
        try
        {
            return await OnDecoderPackageAsync(client, package.Content);
        }
        catch (Exception e)
        {
            await OnDecoderErrorAsync(client, package, e);
        }

        return default;
    }
}