﻿using System;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
namespace Moq
{
    /// <summary>
    /// Encapsulates Moq exceptions and provides extra instructions
    /// </summary>
    [Serializable]
    public class VerifyLogException : Exception
    {
        /// <summary>
        /// Default VerifyLogException constructor
        /// </summary>
        public VerifyLogException()
        {
        }

        /// <summary>
        /// VerifyLogException constructor from a string message
        /// </summary>
        /// <param name="message">The exception message</param>
        public VerifyLogException(string message) : base(message)
        {
        }

        /// <summary>
        /// VerifyLogException constructor from a string message and an inner exception
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="innerException">The inner exception message</param>
        public VerifyLogException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// VerifyLogException used during deserialization
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Serialization context</param>
        protected VerifyLogException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}