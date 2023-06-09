﻿using Core;
using Microsoft.Extensions.Logging;
using SuperSocket.Client;
using SuperSocket.Client.Command;
using SuperSocket.ProtoBase;
using System.Net;

namespace Google.Protobuf.Client;

public sealed class RpcClient : EasyCommandClient<CommandType, CommandPackage>
{
    private readonly IEasyClient<CommandPackage, CommandPackage> _easyClient;
    private readonly PacketDispatcher _packetDispatcher = new();
    private readonly PacketIdentifierProvider _packetIdentifierProvider = new();

    public RpcClient(
        IPackageHandler<CommandType, CommandPackage> packageHandler,
        IPipelineFilter<CommandPackage> pipelineFilter,
        IPackageEncoder<CommandPackage> packageEncoder,
        ILogger<RpcClient> logger) : base(packageHandler, pipelineFilter, packageEncoder, logger)
    {
        _easyClient = this;
    }

    internal new ILogger Logger => base.Logger;

    internal new ValueTask<bool> ConnectAsync(EndPoint remoteEndPoint, CancellationToken cancellationToken)
    {
        //if (remoteEndPoint is IPEndPoint endPoint)
        //{
        //    AsUdp(endPoint);
        //    return ValueTask.FromResult(true);
        //}

        return base.ConnectAsync(remoteEndPoint, cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    public ValueTask TryDispatchAsync(CommandPackage package)
    {
        _packetDispatcher.TryDispatch(package);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// 获取响应封包
    /// </summary>
    /// <typeparam name="TRespPacket"></typeparam>
    /// <param name="package"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public ValueTask<ValueCommandResponse<TReplyPackage>> GetResponseAsync<TReplyPackage>(CommandPackage package) where TReplyPackage : IMessage<TReplyPackage>
    {
        return GetResponseAsync<TReplyPackage>(package, CancellationToken.None);
    }

    /// <summary>
    /// 获取响应封包
    /// </summary>
    /// <typeparam name="TRespPacket"></typeparam>
    /// <param name="packet"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="TaskCanceledException"></exception>
    /// <exception cref="Exception"></exception>
    /// <returns></returns>
    public async ValueTask<ValueCommandResponse<TReplyPackage>> GetResponseAsync<TReplyPackage>(
        CommandPackage package,
        CancellationToken cancellationToken)
        where TReplyPackage : IMessage<TReplyPackage>
    {
        if (CancellationTokenSource == null)
            throw new Exception("没有连接到服务器");

        if (CancellationTokenSource.IsCancellationRequested)
            throw new TaskCanceledException("已经与服务器断开连接");

        cancellationToken.ThrowIfCancellationRequested();

        package.Identifier = _packetIdentifierProvider.GetNextPacketIdentifier();

        using var packetAwaitable = _packetDispatcher.AddAwaitable<TReplyPackage>(package.Identifier);
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancellationTokenSource!.Token);

        try
        {
            await _easyClient.SendAsync(package);
        }
        catch (Exception e)
        {
            packetAwaitable.Fail(e);
            throw new Exception("发送封包抛出一个异常", e);
        }

        try
        {
            return await packetAwaitable.WaitAsync(cancel.Token);
        }
        catch (Exception e)
        {
            if (e is TimeoutException)
                throw new TimeoutException($"等待封包调度超时命令：{package.Key}", e);

            throw new Exception("等待封包调度抛出一个异常", e);
        }
    }

    protected override async ValueTask OnPackageHandlerAsync(EasyClient<CommandPackage> sender, CommandPackage package)
    {
        if (package.RpcKey == CommandType.None)
        {
            await base.OnPackageHandlerAsync(sender, package);
            return;
        }

        try
        {
            await PackageCommandHandler.HandleAsync(this, package, package.RpcKey);//实际命令为rpc命令
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"命令：{package}");
        }
    }
}
