using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Moq
{
    /// <summary>
    /// Encapsulates Moq exceptions and provides extra instructions
    /// </summary>
    [Serializable]
    public class VerifyLogUnexpectedException : Exception
    {
        /// <summary>
        /// Default VerifyLogUnexpectedException constructor
        /// </summary>
        public VerifyLogUnexpectedException()
        {
        }

        /// <summary>
        /// VerifyLogUnexpectedException constructor from a string message
        /// </summary>
        /// <param name="message">The exception message</param>
        public VerifyLogUnexpectedException(string message) : base(message)
        {
        }

        /// <summary>
        /// VerifyLogUnexpectedException constructor from a string message and an inner exception
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception message</param>
        public VerifyLogUnexpectedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// VerifyLogUnexpectedException used during deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected VerifyLogUnexpectedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}