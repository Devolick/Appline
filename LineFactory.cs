using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.RegularExpressions;

namespace Appline
{
    /// <summary>
    /// Factory for creating lines
    /// </summary>
    public static class LineFactory
    {
        private static T Launcher<T>(T connectLine, string filePath, bool dotnet = false, string args = "")
            where T : MessageLine
        {
            connectLine.IsLauncher = true;
            var pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            
            string connectHandlesArgs = $"-connectline {pipeOut.GetClientHandleAsString()}:{pipeIn.GetClientHandleAsString()}";
            connectLine.process = new Process();
            if (dotnet)
            {
                connectLine.process.StartInfo.FileName = "dotnet";
                connectLine.process.StartInfo.Arguments = $"{filePath} {connectHandlesArgs} {args}".Trim();
            }
            else
            {
                connectLine.process.StartInfo.FileName = filePath;
                connectLine.process.StartInfo.Arguments = $"{connectHandlesArgs} {args}".Trim();
            }
            connectLine.process.StartInfo.UseShellExecute = false;

            connectLine.pipeIn = pipeIn;
            connectLine.pipeOut = pipeOut;
            connectLine.reader = new StreamReader(pipeIn);
            connectLine.writer = new StreamWriter(pipeOut);
            connectLine.writer.AutoFlush = true;
            connectLine.writer.WriteLine(Markers.SYNC);

            connectLine.Run(() => {
                connectLine.Timer();
                connectLine.process.Start();
                pipeOut.DisposeLocalCopyOfClientHandle();
                pipeIn.DisposeLocalCopyOfClientHandle();
                connectLine.Await();
                connectLine.Work();
            });

            return connectLine;
        }
        private static T Application<T>(T connectLine, string args)
            where T : MessageLine
        {
            connectLine.IsLauncher = false;
            Regex regex = new Regex(@"(?<=-connectline |^)\S+?:\S+?(?= |$)");
            if (!regex.IsMatch(args)) return connectLine;
            string[] connectHandles = regex.Match(args).Value.Split(':');
            connectLine.pipeIn = new AnonymousPipeClientStream(PipeDirection.In, connectHandles[0]);
            connectLine.pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, connectHandles[1]);
            connectLine.reader = new StreamReader(connectLine.pipeIn);
            connectLine.writer = new StreamWriter(connectLine.pipeOut);
            connectLine.writer.AutoFlush = true;

            connectLine.Run(() => {
                connectLine.Timer();
                connectLine.Await();
                connectLine.Work();
            });

            return connectLine;
        }

        /// <summary>
        /// Generates the main line object and starts the second process.
        /// </summary>
        /// <param name="notify">The notification object.</param>
        /// <param name="filePath">The path of the second process.</param>
        /// <param name="timeout">Time waiting for the connection response. Example 3000ms</param>
        /// <param name="dotnet">Run as dotnet.</param>
        /// <param name="args">Arguments for second process.</param>
        /// <returns>Returns main line.</returns>
        public static MessageLine Launcher(NotifyMessage notify, string filePath, int timeout, bool dotnet = false, string args = "")
        {
            return Launcher(new MessageLine(notify, timeout), filePath, dotnet, args);
        }
        /// <summary>
        /// Generates the main line object and starts the second process.
        /// </summary>
        /// <param name="filePath">The path of the second process.</param>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="dotnet">Run as dotnet.</param>
        /// <param name="args">Arguments for second process.</param>
        /// <returns>Returns main line.</returns>
        public static MessageLine Launcher(string filePath, int timeout, bool dotnet = false, string args = "")
        { return Launcher(new NotifyMessage(), filePath, timeout, dotnet, args); }
        /// <summary>
        /// Generates the main line object and starts the second process.
        /// </summary>
        /// <typeparam name="TContext">Type of the transmitted context.</typeparam>
        /// <param name="notify">The notification object.</param>
        /// <param name="filePath">The path of the second process.</param>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="dotnet">Run as dotnet.</param>
        /// <param name="args">Arguments for second process.</param>
        /// <returns>Returns main line.</returns>
        public static ContextLine<TContext> Launcher<TContext>(NotifyContext<TContext> notify, string filePath, int timeout, bool dotnet = false, string args = "")
            where TContext : class
        {
            return Launcher(new ContextLine<TContext>(notify, timeout), filePath, dotnet, args);
        }

        /// <summary>
        /// Generates the other side object of the line.
        /// </summary>
        /// <param name="notify">The notification object.</param>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="args">Pass entry point args for connect.</param>
        /// <returns>Returns the other side of the line.</returns>
        public static MessageLine Application(NotifyMessage notify, int timeout, string args)
        {
            return Application(new MessageLine(notify, timeout), args);
        }
        /// <summary>
        /// Generates the other side object of the line.
        /// </summary>
        /// <typeparam name="TContext">Type of the transmitted context.</typeparam>
        /// <param name="notify">The notification object.</param>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="args">Pass entry point args for connect.</param>
        /// <returns>Returns the other side of the line.</returns>
        public static ContextLine<TContext> Application<TContext>(NotifyContext<TContext> notify, int timeout, string args)
            where TContext : class
        {
            return Application(new ContextLine<TContext>(notify, timeout), args);
        }
        /// <summary>
        /// Generates the other side object of the line.
        /// </summary>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="args">Pass entry point args for connect.</param>
        /// <returns>Returns the other side of the line.</returns>
        public static MessageLine Application(int timeout, string args)
        { return Application(new NotifyMessage(), timeout, args); }
        /// <summary>
        /// Generates the other side object of the line.
        /// </summary>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="args">Pass entry point args for connect.</param>
        /// <returns>Returns the other side of the line.</returns>
        public static MessageLine Application(int timeout, string[] args)
        { return Application(new NotifyMessage(), timeout, string.Join(" ", args)); }
        /// <summary>
        /// Generates the other side object of the line.
        /// </summary>
        /// <param name="notify">The notification object.</param>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="args">Pass entry point args for connect.</param>
        /// <returns>Returns the other side of the line.</returns>
        public static MessageLine Application(NotifyMessage notify, int timeout, string[] args)
        { return Application(notify, timeout, string.Join(" ", args)); }
        /// <summary>
        /// Generates the other side object of the line.
        /// </summary>
        /// <typeparam name="TContext">Type of the transmitted context.</typeparam>
        /// <param name="notify">The notification object.</param>
        /// <param name="timeout">Time waiting for the connection response.</param>
        /// <param name="args">Pass entry point args for connect.</param>
        /// <returns>Returns the other side of the line.</returns>
        public static ContextLine<TContext> Application<TContext>(NotifyContext<TContext> notify, int timeout, string[] args)
            where TContext : class
        { return Application(new ContextLine<TContext>(notify, timeout), string.Join(" ",args)); }
    }
}
