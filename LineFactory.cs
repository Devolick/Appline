using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.RegularExpressions;

namespace Appline
{
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

        public static MessageLine Launcher(NotifyMessage notify, string filePath, bool dotnet = false, string args = "")
        {
            return Launcher(new MessageLine(notify), filePath, dotnet, args);
        }
        public static MessageLine Launcher(string filePath, bool dotnet = false, string args = "")
        { return Launcher(new NotifyMessage(), filePath, dotnet, args); }
        public static ContextLine<TContext> Launcher<TContext>(NotifyContext<TContext> notify, string filePath, bool dotnet = false, string args = "")
            where TContext : class
        {
            return Launcher(new ContextLine<TContext>(notify), filePath, dotnet, args);
        }

        public static MessageLine Application(NotifyMessage notify, string args)
        {
            return Application(new MessageLine(notify), args);
        }
        public static ContextLine<TContext> Application<TContext>(NotifyContext<TContext> notify, string args)
            where TContext : class
        {
            return Application(new ContextLine<TContext>(notify), args);
        }
        public static MessageLine Application(string args)
        { return Application(new NotifyMessage(), args); }
        public static MessageLine Application(string[] args)
        { return Application(new NotifyMessage(), string.Join(" ", args)); }
        public static MessageLine Application(NotifyMessage notify, string[] args)
        { return Application(notify, string.Join(" ", args)); }
        public static ContextLine<TContext> Application<TContext>(NotifyContext<TContext> notify, string[] args)
            where TContext : class
        { return Application(new ContextLine<TContext>(notify), string.Join(" ",args)); }
    }
}
