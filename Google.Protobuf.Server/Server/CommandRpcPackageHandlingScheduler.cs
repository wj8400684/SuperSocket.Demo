using Core;
using SuperSocket;
using SuperSocket.Server;

namespace Server;

internal sealed class CommandRpcPackageHandlingScheduler : SerialPackageHandlingScheduler<CommandPackage>
{
    public override ValueTask HandlePackage(IAppSession session, CommandPackage package)
    {
        if (package.RpcKey != CommandType.None)
            package.Key = package.RpcKey;//替换rpc为command key这样command执行才可以正常运行

        return base.HandlePackage(session, package);
    }
}
