namespace StockApp.Exceptions
{
    using System;

    public class ProfileNotFoundException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }

    public class ProfilePersistenceException(string message, Exception? innerException = null) : Exception(message, innerException)
    {
    }
} 