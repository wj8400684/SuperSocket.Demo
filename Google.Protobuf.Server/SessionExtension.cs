using Core;
using Google.Protobuf;

namespace Server;

internal static class SessionExtension
{
    //public static ValueTask<ValueCommandResponse<TReplyPackage>> InvokerAsync<TPackageContent, TReplyPackage>(this CommandSession session,
    //    TPackageContent content,
    //    CancellationToken cancellationToken)
    //    where TPackageContent : IMessage
    //    where TReplyPackage : IMessage<TReplyPackage>
    //{
    //   var command = CommandPackageFactory.CreateRequest(content);

    //   return session.GetResponsePacketAsync<TReplyPackage>(command, cancellationToken);
    //}

    public static ValueTask<ValueCommandResponse<TReplyPackage>> InvokerRpcAsync<TPackageContent, TReplyPackage>(this CommandSession session,
        TPackageContent content,
        CancellationToken cancellationToken)
        where TPackageContent : IMessage
        where TReplyPackage : IMessage<TReplyPackage>
    {
        var command = CommandPackageFactory.CreateRpcRequest(content);

        return session.GetResponsePacketAsync<TReplyPackage>(command, cancellationToken);
    }
}
