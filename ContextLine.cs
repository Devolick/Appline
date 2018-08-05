using Newtonsoft.Json;
using System;

namespace Appline
{
    /// <summary>
    /// Line for the transfer of complex objects.
    /// </summary>
    /// <typeparam name="TContext">Type of the transmitted object</typeparam>
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

        /// <summary>
        /// Contains the cycle for receiving a message
        /// </summary>
        protected override void Receive()
        {
            string msg = string.Empty;
            try
            {
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
            catch (Exception ex)
            {
                Notify.InvokeException(new LineException("An error occurred while receive data on the line.", ex));
                try { Close(); } catch { }
            }
        }
        /// <summary>
        /// Sends the object to the other end of the line.
        /// </summary>
        /// <param name="context"></param>
        [Obsolete("SaveChanges has an invalid name, please use method Send instead.")]
        public void SaveChanges(TContext context)
        {
            Send(context);
        }
        /// <summary>
        /// Sends the string to the other end of the line.
        /// </summary>
        /// <param name="context">Complex object.</param>
        public void Send(TContext context)
        {
            try
            {
                string msg = $"{Markers.JSON}{Base64Encode(JsonConvert.SerializeObject(context))}";
                writer.WriteLine(msg);

                pipeOut.WaitForPipeDrain();
            }
            catch(Exception ex)
            {
                Notify.InvokeException(new LineException("An error occurred while sending data on the line.", ex));
                try { Close(); } catch { }
            }
        }
    }
}
