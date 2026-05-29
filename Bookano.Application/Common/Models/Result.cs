using System.Collections.Generic;

namespace Bookano.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? ErrorMessage { get; }
    public IEnumerable<ValidationError> Errors { get; }

    protected Result(bool isSuccess, string? errorMessage = null, IEnumerable<ValidationError>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors ?? [];
    }

    public static Result Success() => new(true);
    public static Result Failure(string errorMessage) => new(false, errorMessage);
    public static Result Failure(IEnumerable<ValidationError> errors) => new(false, null, errors);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue? Value => IsSuccess ? _value : default;

    protected internal Result(TValue? value, bool isSuccess, string? errorMessage = null, IEnumerable<ValidationError>? errors = null)
        : base(isSuccess, errorMessage, errors)
    {
        _value = value;
    }

    public static Result<TValue> Success(TValue value) => new(value, true);
    public static new Result<TValue> Failure(string errorMessage) => new(default, false, errorMessage);
    public static new Result<TValue> Failure(IEnumerable<ValidationError> errors) => new(default, false, null, errors);

    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
