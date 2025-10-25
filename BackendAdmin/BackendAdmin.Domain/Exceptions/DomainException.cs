namespace BackendAdmin.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message)
        : base($"Domain Exception: \"{message}\" levée depuis la couche Domaine.")
    {
    }
}

