namespace Google.Protobuf;

public static class ProtoMessageExtensions
{
    /// <summary>
    /// Converts a message from one type into another. The two types must be compatible
    /// and generated from the same .proto file, but may have different namespaces.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    public static T As<T>(this IMessage message)
        where T : IMessage, new() {
        var target = new T();
        target.MergeFrom(message.ToByteString());

        return target;
    }
}