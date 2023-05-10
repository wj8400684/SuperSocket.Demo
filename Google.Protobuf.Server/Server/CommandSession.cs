using System.Net;
using Core;
using Google.Protobuf;
using SuperSocket.Channel;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace Server;

public sealed class CommandSession : AppSession
{
    private readonly IPackageEncoder<CommandPackage> _encoder;
    private readonly CancellationTokenSource _connectionTokenSource;
    private readonly PacketDispatcher _packetDispatcher = new();
    private readonly PacketIdentifierProvider _packetIdentifierProvider = new();

    public CommandSession(IServiceProvider serviceProvider)
    {
        _encoder = serviceProvider.GetRequiredService<IPackageEncoder<CommandPackage>>();
        _connectionTokenSource = new CancellationTokenSource();
    }

    #region 属性

    /// <summary>
    /// 远程地址
    /// </summary>
    internal string RemoteAddress { get; private set; } = null!;

    /// <summary>
    /// 连接token
    /// </summary>
    internal CancellationToken ConnectionToken => _connectionTokenSource.Token;

    #endregion

    /// <summary>
    /// 客户端连接
    /// </summary>
    /// <returns></returns>
    protected override ValueTask OnSessionConnectedAsync()
    {
        RemoteAddress = ((IPEndPoint)RemoteEndPoint).Address.ToString();

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 客户端断开
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected override ValueTask OnSessionClosedAsync(CloseEventArgs e)
    {
        _packetIdentifierProvider.Reset();
        _connectionTokenSource.Cancel();
        _connectionTokenSource.Dispose();
        _packetDispatcher.CancelAll();
        _packetDispatcher.Dispose();

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyPackage"></typeparam>
    /// <param name="package"></param>
    /// <param name="responseTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<ValueCommandResponse<TReplyPackage>> GetResponsePacketAsync<TReplyPackage>(
        CommandPackage package,
        TimeSpan responseTimeout,
        CancellationToken cancellationToken)
        where TReplyPackage : IMessage<TReplyPackage>
    {
        using var timeOut = new CancellationTokenSource(responseTimeout);
        return GetResponsePacketAsync<TReplyPackage>(package, ConnectionToken, cancellationToken, timeOut.Token);
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TReplyPackage"></typeparam>
    /// <param name="package"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    internal ValueTask<ValueCommandResponse<TReplyPackage>> GetResponsePacketAsync<TReplyPackage>(
        CommandPackage package,
        CancellationToken cancellationToken) 
        where TReplyPackage : IMessage<TReplyPackage>
    {
        return GetResponsePacketAsync<TReplyPackage>(package, ConnectionToken, cancellationToken);
    }

    /// <summary>
    /// 获取响应包
    /// </summary>
    /// <typeparam name="TResponsePacket"></typeparam>
    /// <param name="package"></param>
    /// <param name="tokens"></param>
    /// <returns></returns>
    internal async ValueTask<ValueCommandResponse<TReplyPackage>> GetResponsePacketAsync<TReplyPackage>(
        CommandPackage package,
        params CancellationToken[] tokens) where TReplyPackage : IMessage<TReplyPackage>
    {
        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(tokens);

        package.Identifier = _packetIdentifierProvider.GetNextPacketIdentifier();

        using var packetAwaitable = _packetDispatcher.AddAwaitable<TReplyPackage>(package.Identifier);

        this.LogDebug($"[{RemoteAddress}]: commandKey= {package.Key};Identifier= {package.Identifier} WaitAsync");

        try
        {
            //发送转发封包
            await SendPackageAsync(package);
        }
        catch (Exception e)
        {
            packetAwaitable.Fail(e);
            this.LogError(e,
                $"[{RemoteAddress}]: commandKey= {package.Key};Identifier= {package.Identifier} WaitAsync 发送封包抛出一个异常");
        }

        try
        {
            //等待封包结果
            return await packetAwaitable.WaitAsync(tokenSource.Token);
        }
        catch (Exception e)
        {
            if (e is TimeoutException)
                this.LogError(
                    $"[{RemoteAddress}]: commandKey= {package.Key};Identifier= {package.Identifier} WaitAsync Timeout");

            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    internal ValueTask TryDispatchAsync(CommandPackage package)
    {
        var result = _packetDispatcher.TryDispatch(package);

        this.LogDebug(
            $"[{RemoteAddress}]: commandKey= {package.Key};Identifier= {package.Identifier} TryDispatch result= {result}");

        return ValueTask.CompletedTask;
    }


    internal ValueTask SendPackageAsync(CommandPackage package)
    {
        return Channel.IsClosed ? ValueTask.CompletedTask : Channel.SendAsync(_encoder, package);
    }
}