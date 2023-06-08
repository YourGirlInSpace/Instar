namespace PaxAndromeda.Instar;

public class InvalidStateException : Exception
{
    public InvalidStateException(string message) : base(message)
    {
    }
}