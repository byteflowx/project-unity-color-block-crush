namespace App.Scripts.Core.Common
{
    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public ServiceError Error { get; }

        private Result(bool isSuccess, T value, ServiceError error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(true, value, null);
        public static Result<T> Failure(ServiceError error) => new(false, default, error);
    }
}
