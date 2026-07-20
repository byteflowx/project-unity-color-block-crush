using System;

namespace App.Scripts.Core.Common
{
    public sealed class ServiceError
    {
        public string Code { get; }
        public string Message { get; }
        public ErrorSeverity Severity { get; }
        public Exception Underlying { get; }

        public ServiceError(string code, string message, ErrorSeverity severity = ErrorSeverity.Error, Exception underlying = null)
        {
            Code = code;
            Message = message;
            Severity = severity;
            Underlying = underlying;
        }

        public ServiceError With(Exception underlying) =>
            new(Code, Message, Severity, underlying);
    }
}
