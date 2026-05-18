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
    }
}
