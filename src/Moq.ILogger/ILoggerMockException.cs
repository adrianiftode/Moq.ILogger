using System;
using System.Runtime.Serialization;

namespace Moq
{
    [Serializable]
    public class ILoggerMockException : Exception
    {
        public ILoggerMockException()
        {
        }

        public ILoggerMockException(string message) : base(message)
        {
        }

        public ILoggerMockException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ILoggerMockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}