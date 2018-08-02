using System;

namespace Appline
{
    public class NotifyContext<TContext> : NotifyMessage
    {
        public NotifyContext() { }
        /// <summary>
        /// It will be called when the context object is received.
        /// </summary>
        public event Action<TContext> ContextChanges;
        internal void InvokeContextChanges(TContext context)
        { ContextChanges?.Invoke(context); }

    }
}
