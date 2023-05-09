using Google.Protobuf;

namespace Core;

public sealed class PacketDispatcher : IDisposable
{
    private bool _isDisposed;
    private readonly Dictionary<uint, IPacketAwaitable> _waiters = new();

    public PacketAwaitable<TResponsePacket> AddAwaitable<TResponsePacket>(uint packetIdentifier)
        where TResponsePacket : IMessage<TResponsePacket>
    {
        var awaitable = new PacketAwaitable<TResponsePacket>(packetIdentifier, this);

        lock (_waiters)
            _waiters.TryAdd(packetIdentifier, awaitable);

        return awaitable;
    }

    public void CancelAll()
    {
        lock (_waiters)
        {
            using var enumerator = _waiters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var packetAwait = enumerator.Current.Value;

                packetAwait.Cancel();
            }

            _waiters.Clear();
        }
    }

    public void Dispose()
    {
        Dispose(new ObjectDisposedException(nameof(PacketDispatcher)));
    }

    public void Dispose(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        lock (_waiters)
        {
            FailAll(exception);

            // Make sure that no task can start waiting after this instance is already disposed.
            // This will prevent unexpected freezes.
            _isDisposed = true;
        }
    }

    public void FailAll(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        lock (_waiters)
        {
            using var enumerator = _waiters.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var packetAwait = enumerator.Current.Value;

                packetAwait.Fail(exception);
            }

            _waiters.Clear();
        }
    }

    public void RemoveAwaitable(uint identifier)
    {
        lock (_waiters)
        {
            _waiters.Remove(identifier);
        }
    }

    public bool TryDispatch(CommandPackage packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        var identifier = packet.RequestId;

        IPacketAwaitable? awaitable;

        lock (_waiters)
        {
            ThrowIfDisposed();

            if (!_waiters.TryGetValue(identifier, out awaitable))
                return false;

            _waiters.Remove(identifier);
        }

        awaitable.Complete(packet);

        return true;
    }

    void ThrowIfDisposed()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(PacketDispatcher));
    }
}