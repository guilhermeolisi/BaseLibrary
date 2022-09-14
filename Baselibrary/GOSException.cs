using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary
{
    public class GOSException : Exception
    {
        public GOSException()
        {
        }

        public GOSException(string message) : base(message)
        {
        }

        public GOSException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GOSException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
