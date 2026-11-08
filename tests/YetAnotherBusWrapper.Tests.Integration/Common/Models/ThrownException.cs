namespace YetAnotherBusWrapper.Tests.Integration.Common.Models;

internal record ThrownException(string Message, string TypeName)
{
    public static ThrownException From(Exception exception)
    {
        const string indent = "      ";
        string message = exception.Message;
        if (message.Contains('\n'))
        {
            message = indent + message.Replace("\n", "\n" + indent);
        }

        return new ThrownException(message, exception.GetType().Name);
    }
}
