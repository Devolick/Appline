using System;
using System.Collections.Generic;
using System.Text;

namespace Appline
{
    /// <summary>
    /// Error code for LineException exception.
    /// </summary>
    public enum ExCode
    {
        /// <summary>
        /// Error code for sending data.
        /// </summary>
        Send,
        /// <summary>
        /// Error code for receiving data.
        /// </summary>
        Receive,
        /// <summary>
        /// Error code for connection.
        /// </summary>
        Connect
    }
}
