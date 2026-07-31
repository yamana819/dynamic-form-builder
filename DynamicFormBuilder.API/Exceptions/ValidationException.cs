namespace DynamicFormBuilder.API.Exceptions;

public class ValidationAppException : AppException
{
    public IEnumerable<string> Errors { get; }

    public ValidationAppException(IEnumerable<string> errors) 
        : base("Doğrulama hataları oluştu.", 400)
    {
        Errors = errors;
    }
}