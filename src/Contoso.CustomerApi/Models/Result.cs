namespace Contoso.CustomerApi.Models;

/// <summary>
/// Result of a service operation.
///
/// NOTE: the error channel here is a bare string. Every consumer therefore compares
/// error codes by string literal, which means a typo is a runtime bug rather than a
/// compile error. Replacing this with a typed error is the refactor used in Demo 02.
/// </summary>
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(string errorCode, string errorMessage) =>
        new(false, default, errorCode, errorMessage);
}

/// <summary>
/// The error codes the service layer can return.
///
/// These are plain constants, so nothing stops a caller inventing a code that no
/// handler knows about. The endpoint layer maps these to status codes by string
/// comparison - see <c>CustomerEndpoints.ToProblem</c>.
/// </summary>
public static class ErrorCodes
{
    public const string NotFound = "customer_not_found";
    public const string ValidationFailed = "validation_failed";
    public const string DuplicateEmail = "duplicate_email";
    public const string Forbidden = "forbidden";
}
