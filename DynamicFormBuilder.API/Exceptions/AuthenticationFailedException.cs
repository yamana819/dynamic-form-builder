

namespace DynamicFormBuilder.API.Exceptions;


public class AuthenticationFailedException : AppException
{
    public AuthenticationFailedException(string message):base(message,403){}
}