using System.Collections.Concurrent;
using Google.Protobuf;

namespace Core;

public readonly struct ValueCommandResponse<TPackageContent>
    where TPackageContent : IMessage<TPackageContent>
{
    private static readonly ConcurrentDictionary<Type, MessageParser<TPackageContent>> MessageParsers = new();

    public ValueCommandResponse(in CommandPackage package)
    {
        SuccessFul = package.SuccessFul;
        ErrorCode = package.ErrorCode;
        ErrorMessage = package.ErrorMessage;

        var messageParser = MessageParsers.GetOrAdd(typeof(TPackageContent),
            type => new MessageParser<TPackageContent>(() => Activator.CreateInstance<TPackageContent>()!));

        Content = messageParser.ParseFrom(package.Content);
    }

    public bool SuccessFul { get; }

    public ErrorCode ErrorCode { get; }

    public string? ErrorMessage { get; }

    public TPackageContent Content { get; }
}