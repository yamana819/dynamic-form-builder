 

 namespace DynamicFormBuilder.API.Exceptions;
 
 public class SchemaValidationException : AppException
{
    public SchemaValidationException(string message) : base(message,400) { }
}
