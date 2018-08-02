using System;

namespace Appline
{
    public class NotifyMessage
    {
        public NotifyMessage() { }

        public event Action Timeout;
        internal void InvokeTimeout()
        { Timeout?.Invoke(); }
        public event Action Connected;
        internal void InvokeConnected()
        { Connected?.Invoke(); }
        public event Action Disconnected;
        internal void InvokeDisconnected()
        { Disconnected?.Invoke(); }
        public event Action<string> Message;
        internal void InvokeMessage(string msg)
        { Message?.Invoke(msg); }

    }
}
