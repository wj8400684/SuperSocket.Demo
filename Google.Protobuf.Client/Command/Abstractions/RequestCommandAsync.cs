using Core;
using SuperSocket.Client.Command;

namespace Google.Protobuf.Client;

public abstract class RequestCommandAsync<TRequestPackage> : IAsyncCommand<CommandPackage> 
    where TRequestPackage : IMessage<TRequestPackage>
{
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
    protected abstract ValueTask OnHandlerAsync(
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
        return ValueTask.CompletedTask;
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
        return ValueTask.CompletedTask;
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

        try
        {
            await OnHandlerAsync(client, package, request);
        }
        catch (Exception e)
        {
            await OnHandlerErrorAsync(client, package, request, e);
        }
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
