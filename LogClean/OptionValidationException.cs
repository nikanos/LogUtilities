using System;
using System.Runtime.Serialization;

namespace LogClean
{
    [Serializable]
    public class OptionValidationException : Exception
    {
        public OptionValidationException()
        {
        }

        public OptionValidationException(string message) : base(message)
        {
        }

        public OptionValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OptionValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}