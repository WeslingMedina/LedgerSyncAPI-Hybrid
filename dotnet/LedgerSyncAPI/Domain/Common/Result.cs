using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, Error error)
            : base(isSuccess, error) => Value = value;
    }

    public class Result
    {
        public bool IsSuccess { get; }
        public Error Error { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Success result cannot have an error");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("Failure result must have an error");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result<T> Success<T>(T value) => new(value, true, Error.None);
        public static Result Failure(Error error) => new(false, error);
        public static Result<T> Failure<T>(Error error) => new(default!, false, error);
    }

    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);

        public static Error Validation(List<ValidationFailure> failures)
            => new("Validation.Error", string.Join(", ", failures.Select(f => f.ErrorMessage)));
    }
}
