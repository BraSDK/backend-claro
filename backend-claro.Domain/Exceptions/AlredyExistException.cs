using System;

namespace backend_claro.Domain.Exceptions;

public class AlredyExistException:Exception
{
    public AlredyExistException(String message) : base(message)
    {
        
    }
}