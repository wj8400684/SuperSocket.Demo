using Core;
using Microsoft.Extensions.Options;
using SuperSocket;
using SuperSocket.Server;

namespace Server;

public sealed class CommandServer : SuperSocketService<CommandPackage>
{
    public CommandServer(IServiceProvider serviceProvider, IOptions<ServerOptions> serverOptions) : base(
        serviceProvider, serverOptions)
    {
    }
}