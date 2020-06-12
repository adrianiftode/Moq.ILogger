using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Moq
{
    /// <summary>
    /// Encapsulates Moq exceptions and provides extra instructions
    /// </summary>
    [Serializable]
    public class LoggerMockException : Exception
    {
        /// <summary>
        /// Default LoggerMockException constructor
        /// </summary>
        public LoggerMockException()
        {
        }

        /// <summary>
        /// LoggerMockException constructor from a string message
        /// </summary>
        /// <param name="message">The exception message</param>
        public LoggerMockException(string message) : base(message)
        {
        }

        /// <summary>
        /// LoggerMockException constructor from a string message and an inner exception
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception message</param>
        public LoggerMockException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// LoggerMockException used during deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected LoggerMockException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}