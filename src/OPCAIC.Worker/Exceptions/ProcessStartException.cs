using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace OPCAIC.Worker.Exceptions
{
    class ProcessStartException : Exception
    {
        public ProcessStartException(string process) : base(
            $"Process '{process}' could not be started.")
        {
        }

        protected ProcessStartException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}

