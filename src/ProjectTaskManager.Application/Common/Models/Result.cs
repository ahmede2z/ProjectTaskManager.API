namespace ProjectTaskManager.Application.Common.Models;

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null && error != Error.None)
            throw new ArgumentException("A successful result cannot have an error.", nameof(error));

        if (!isSuccess && (error is null || error == Error.None))
            throw new ArgumentException("A failed result must have an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}
