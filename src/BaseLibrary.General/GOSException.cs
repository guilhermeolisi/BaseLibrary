using System.Runtime.Serialization;

namespace BaseLibrary
{
    public class GOSException : Exception
    {
        public bool CanContinue { get; private set; } = false;

        public GOSException(bool canContinue)
        {
            CanContinue = canContinue;
        }

        public GOSException(bool canContinue, string message) : base(message)
        {
            CanContinue = canContinue;
        }

        public GOSException(bool canContinue, string message, Exception innerException) : base(message, innerException)
        {
            CanContinue = canContinue;
        }

        protected GOSException(bool canContinue, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CanContinue = canContinue;
        }
    }
}
