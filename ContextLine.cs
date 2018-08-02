using Newtonsoft.Json;
using System;

namespace Appline
{
    public class ContextLine<TContext>: MessageLine, IDisposable
        where TContext : class
    {
        /// <summary>
        /// The notification object.
        /// </summary>
        public new NotifyContext<TContext> Notify {
            get { return base.Notify as NotifyContext<TContext>; }
            set { base.Notify = value; }
        }

        private ContextLine() { }
        internal ContextLine(NotifyContext<TContext> notify)
            : base(notify) { }

        protected override void Receive()
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
        /// <summary>
        /// Sends the object to the other end of the line.
        /// </summary>
        /// <param name="context"></param>
        public void SaveChanges(TContext context)
        {
            string msg = $"{Markers.JSON}{Base64Encode(JsonConvert.SerializeObject(context))}";
            writer.WriteLine(msg);

            pipeOut.WaitForPipeDrain();
        }
    }
}
