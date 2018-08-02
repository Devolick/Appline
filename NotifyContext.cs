using System;

namespace Appline
{
    public class NotifyContext<TContext> : NotifyMessage
    {
        public NotifyContext() { }

        public event Action<TContext> ContextChanges;
        internal void InvokeContextChanges(TContext context)
        { ContextChanges?.Invoke(context); }

    }
}
