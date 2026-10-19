using System;

namespace backend_claro.Domain.Exceptions;

public class AlreadyExistException:Exception
{
    public AlreadyExistException(String message) : base(message)
    {
        
    }
}