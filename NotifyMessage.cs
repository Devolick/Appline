using System;

namespace Appline
{
    /// <summary>
    /// This object registers events for listening.
    /// </summary>
    public class NotifyMessage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NotifyMessage() { }

        /// <summary>
        /// It will be called if the start fails for the allotted time.
        /// </summary>
        public event Action Timeout;
        internal void InvokeTimeout()
        { Timeout?.Invoke(); }
        /// <summary>
        /// It will be called when the connection is successful.
        /// </summary>
        public event Action Connected;
        internal void InvokeConnected()
        { Connected?.Invoke(); }
        /// <summary>
        /// It will be called when the connection fails.
        /// </summary>
        public event Action Disconnected;
        internal void InvokeDisconnected()
        { Disconnected?.Invoke(); }
        /// <summary>
        /// It will be called when a string is received.
        /// </summary>
        public event Action<string> Message;
        internal void InvokeMessage(string msg)
        { Message?.Invoke(msg); }
        /// <summary>
        /// It will be called when the line throw any exception
        /// </summary>
        public event Action<Exception> Exception;
        internal void InvokeException(Exception ex)
        { Exception?.Invoke(ex); }
    }
}
