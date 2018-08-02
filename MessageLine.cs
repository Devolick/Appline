using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace Appline
{
    public class MessageLine : IDisposable
    {
        /// <summary>
        /// The notification object.
        /// </summary>
        public NotifyMessage Notify { get; internal set; }

        internal PipeStream pipeIn;
        internal PipeStream pipeOut;
        internal StreamWriter writer;
        internal StreamReader reader;

        private Thread thread;
        internal Process process;
        private System.Timers.Timer timer;
        private bool isOutOfTime;
        internal int timeout;

        /// <summary>
        /// Indicates the status of the line.
        /// </summary>
        public bool IsConnected {
            get
            {
                bool inValue = pipeIn == null ? false : pipeIn.IsConnected;
                bool outValue = pipeOut == null ? false : pipeOut.IsConnected;
                return inValue && outValue;
            }
        }
        /// <summary>
        /// Indicates whether the process is running.
        /// </summary>
        public bool IsRunning { get; internal set; }
        /// <summary>
        /// Indicates whether this line is the main line.
        /// </summary>
        public bool IsLauncher { get; internal set; }

        protected MessageLine()
        {
            IsLauncher = false;
            IsRunning = false;
            isOutOfTime = false;
        }
        internal MessageLine(NotifyMessage notify)
            :this()
        {
            Notify = notify;
        }

        /// <summary>
        /// Sends the string to the other end of the line.
        /// </summary>
        /// <param name="msg">String message.</param>
        public virtual void Send(string msg)
        {
            if (IsRunning)
            {
                if(msg.Length < 1)
                {
                    writer.WriteLine(Markers.EMPTY);
                }
                else
                {
                    writer.WriteLine($"{Markers.MSG}{Base64Encode(msg)}");
                }
                pipeOut.WaitForPipeDrain();
            }
        }
        protected virtual void Receive() {
            string msg = string.Empty;
            while (IsConnected && IsRunning)
            {
                msg = reader.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    Notify.InvokeMessage(Base64Decode(msg.UnMark()));
                }
            }
        }
        /// <summary>
        /// Closes the line.
        /// </summary>
        public void Close()
        {
            if (IsRunning)
            {
                IsRunning = false;
                timer.Stop();
                writer.Close();
                reader.Close();
                pipeOut.Close();
                pipeIn.Close();
                Notify.InvokeDisconnected();
            }
        }

        internal void Timer()
        {
            timer = new System.Timers.Timer(timeout);
            timer.Elapsed += (sender, e) =>
            {
                timer.Stop();
                isOutOfTime = true;
                if (!IsConnected)
                {
                    Notify.InvokeTimeout();
                    Close();
                }
            };
            timer.Start();
        }
        internal void Await()
        {
            if (IsLauncher)
            {
                pipeOut.WaitForPipeDrain();
            }
            else
            {
                string msg;
                do
                {
                    msg = reader.ReadLine();
                }
                while (!msg.StartsWith(Markers.SYNC) && IsRunning) ;
            }
        }
        internal void Work()
        { 
            if (!isOutOfTime)
            {
                if (IsConnected)
                {
                    timer.Stop();
                    Notify.InvokeConnected();
                };

                Receive();

                Close();
            }
        }
        internal void Run(ThreadStart action)
        {
            IsRunning = true;
            thread = new Thread(action);
            thread.IsBackground = false;
            thread.Start();
        }

        protected string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        protected string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
