using System;
using System.Collections.Generic;
using System.Text;

namespace Appline
{
    /// <summary>
    /// The exception that is throw when there is a line fault.
    /// </summary>
    public class LineException:Exception
    {
        /// <summary>
        /// Exception code.
        /// </summary>
        public ExCode Code { get; private set; }

        /// <summary>
        /// Initializes a new instance of the LineException class to handle exceptions for working with a line.
        /// </summary>
        /// <param name="msg">A string that describes that error.</param>
        /// <param name="ex">The exception that is cause current exception. If the innerException parameter is not, the current exception is raised in a catch block that handles the inner exception.</param>
        /// <param name="code">Exception code.</param>
        public LineException(string msg, Exception ex, ExCode code)
            : this(msg, ex) {
            Code = code;
        }
        private LineException() { }
        private LineException(string msg)
            : base(msg) { }
        private LineException(string msg, Exception ex)
            : base(msg, ex) { }
    }
}
