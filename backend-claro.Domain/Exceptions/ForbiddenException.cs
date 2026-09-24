using System;

namespace backend_claro.Domain.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException (string message) : base(message)
    {
        
    }
}