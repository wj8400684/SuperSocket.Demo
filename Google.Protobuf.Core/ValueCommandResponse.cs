using System.Collections.Concurrent;
using Google.Protobuf;

namespace Core;

public readonly struct ValueCommandResponse
{
    public static ValueCommandResponse<TPackageContent> Error<TPackageContent>(ErrorCode errorCode, string? errorMessage)
        where TPackageContent : IMessage<TPackageContent>
    {
        return new ValueCommandResponse<TPackageContent>(false, errorCode, errorMessage);
    }
}

public readonly struct ValueCommandResponse<TPackageContent>
    where TPackageContent : IMessage<TPackageContent>
{
    private static readonly ConcurrentDictionary<Type, MessageParser<TPackageContent>> MessageParsers = new();

    public ValueCommandResponse(bool successFul, ErrorCode errorCode, string? errorMessage)
    {
        SuccessFul = successFul;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

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

    public TPackageContent? Content { get; }
}