



namespace DynamicFormBuilder.API.Exceptions;

public class ResourceNotFoundException : AppException
{
    public ResourceNotFoundException(string message):base(message,404){}
}