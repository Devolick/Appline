using System;

namespace Appline
{
    /// <summary>
    /// This object registers events for listening.
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class NotifyContext<TContext> : NotifyMessage
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NotifyContext() { }
        /// <summary>
        /// It will be called when the context object is received.
        /// </summary>
        public event Action<TContext> ContextChanges;
        internal void InvokeContextChanges(TContext context)
        { ContextChanges?.Invoke(context); }

    }
}
