using Core;
using SuperSocket;
using SuperSocket.Server;

namespace Server;

internal sealed class CommandRpcPackageHandlingScheduler : SerialPackageHandlingScheduler<CommandPackage>
{
    public override ValueTask HandlePackage(IAppSession session, CommandPackage package)
    {
        if (!string.IsNullOrEmpty(package.RpcKey))
            package.Key = package.RpcKey;//替换rpc为command key这样command执行才可以正常运行

        return base.HandlePackage(session, package);
    }
}
