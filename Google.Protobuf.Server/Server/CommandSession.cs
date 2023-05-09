using Core;
using SuperSocket.ProtoBase;
using SuperSocket.Server;

namespace Server;

public sealed class CommandSession : AppSession
{
    private readonly IPackageEncoder<CommandPackage> _encoder;

    public CommandSession(IServiceProvider serviceProvider)
    {
        _encoder = serviceProvider.GetRequiredService<IPackageEncoder<CommandPackage>>();
    }
    
    internal ValueTask SendPackageAsync(CommandPackage package)
    {
        return Channel.IsClosed ? ValueTask.CompletedTask : Channel.SendAsync(_encoder, package);
    }
}