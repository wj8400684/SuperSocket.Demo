using Google.Protobuf;

namespace Core;

public sealed class PacketAwaitable<TPacket> : IPacketAwaitable where TPacket : IMessage<TPacket>
{
    private readonly TaskCompletionSource<CommandPackage> _promise = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly PacketDispatcher _owningPacketDispatcher;
    private readonly uint _packetIdentifier;
    
    public PacketAwaitable(uint packetIdentifier, PacketDispatcher owningPacketDispatcher)
    {
        _packetIdentifier = packetIdentifier;
        _owningPacketDispatcher = owningPacketDispatcher ?? throw new ArgumentNullException(nameof(owningPacketDispatcher));
    }
        
    public async Task<ValueCommandResponse<TPacket>> WaitAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await using var register = cancellationToken.Register(() => Fail(new TimeoutException()));
        var packet = await _promise.Task.ConfigureAwait(false);

        return new ValueCommandResponse<TPacket>(packet);
    }

    public void Complete(CommandPackage packet)
    {
        if (packet == null) 
            throw new ArgumentNullException(nameof(packet));

        _promise.TrySetResult(packet);
    }

    public void Fail(Exception exception)
    {
        if (exception == null) 
            throw new ArgumentNullException(nameof(exception));
            
        _promise.TrySetException(exception);
    }

    public void Cancel()
    {
        _promise.TrySetCanceled();
    }

    public void Dispose()
    {
        _owningPacketDispatcher.RemoveAwaitable(_packetIdentifier);
    }
}