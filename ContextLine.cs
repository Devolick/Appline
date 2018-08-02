using Newtonsoft.Json;
using System;

namespace Appline
{
    public class ContextLine<TContext>: MessageLine, IDisposable
        where TContext : class
    {
        public new NotifyContext<TContext> Notify {
            get { return base.Notify as NotifyContext<TContext>; }
            set { base.Notify = value; }
        }

        private ContextLine() { }
        internal ContextLine(NotifyContext<TContext> notify)
            : base(notify) { }

        public override void Receive()
        {
            string msg = string.Empty;
            while (IsConnected && IsRunning)
            {
                msg = reader.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    switch (msg[0])
                    {
                        case 'j':
                            Notify.InvokeContextChanges(JsonConvert.DeserializeObject<TContext>(Base64Decode(msg.UnMark())));
                            break;
                        default:
                            Notify.InvokeMessage(Base64Decode(msg.UnMark()));
                            break;
                    }
                }
            }
        }
        public bool SaveChanges(TContext context)
        {
            if (context != null)
            {
                string msg = $"{Markers.JSON}{Base64Encode(JsonConvert.SerializeObject(context))}";
                writer.WriteLine(msg);

                pipeOut.WaitForPipeDrain();
                return true;
            }
            return false;
        }
    }
}
