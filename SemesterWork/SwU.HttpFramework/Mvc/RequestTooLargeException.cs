namespace SwU.HttpFramework.Mvc;

public class RequestTooLargeException : Exception
{
    public int MaxBytes { get; }

    public RequestTooLargeException(int maxBytes)
        : base($"Размер тела запроса превышает допустимый лимит ({maxBytes} байт).")
    {
        MaxBytes = maxBytes;
    }
}
