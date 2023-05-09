
namespace Core;

public interface IPacketAwaitable : IDisposable
{
    void Complete(CommandPackage packet);

    void Fail(Exception exception);

    void Cancel();
}