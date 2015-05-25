using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebGrabber
{
    public class WebOperationException : Exception
    {
        public Log.Level Level { private set; get; }

        public WebOperationException(String message, Log.Level level)
            : base(message)
        {
            Level = level;
        }
    }

    public static class WebOperationExceptionExtensions
    {
        public static void ThrowWebOperationException(this String message, Log.Level level)
        {
            throw new WebOperationException(message, level);
        }
    }
}
