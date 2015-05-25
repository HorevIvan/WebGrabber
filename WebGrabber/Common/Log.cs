using System;

// This framework of log is extendable and it wrapper.
// In future may be wrap log4net, nlog or e.t.c.
using System.Diagnostics;
using System.Threading;
using Microsoft.SqlServer.Server;

namespace WebGrabber
{
    public static class Log
    {
        public delegate void Handler(String message, Type type, Level level);

        public static Handler Out { set; get; }

        static Log()
        {
            Out = OutHandlers.VisualStudioHandler.DtThTyLvMs_Out;
        }

        public enum Type
        {
            Information,

            Warning,

            Error,
        }

        public enum Level
        {
            Debug,

            Lowered,

            Middle,

            Increased,

            Hight,

            Critical,
        }

        public static class OutHandlers
        {
            public static String DtThTyLvMs_Pattern { set; get; }

            static OutHandlers()
            {
                DtThTyLvMs_Pattern = "[{0:HH:mm:ss.ff}] [{1}] [{2}] [{3}] {4}";
            }

            public static class ConsoleHandler
            {
                public static void DtThTyLvMs_Out(String message, Type type, Level level)
                {
                    Console.WriteLine(DtThTyLvMs_Pattern, DateTime.Now, Thread.CurrentThread.ManagedThreadId, type, level, message);
                }
            }

            public static class VisualStudioHandler
            {
                public static void DtThTyLvMs_Out(String message, Type type, Level level)
                {
                    Debug.WriteLine(DtThTyLvMs_Pattern, DateTime.Now, Thread.CurrentThread.ManagedThreadId, type, level, message);
                }
            }
        }
    }

    public static class LogExtensions
    {
        public static void Log(this String message, WebGrabber.Log.Type type = WebGrabber.Log.Type.Information, WebGrabber.Log.Level level =WebGrabber.Log.Level.Debug)
        {
            WebGrabber.Log.Out(message, type, level);
        }

        public static void Log(this Exception exception, WebGrabber.Log.Level level)
        {
            "{0}\n\r{1}"
                .Set(exception.Message, exception.StackTrace)
                .Log(WebGrabber.Log.Type.Error, level);
        }

        public static void Log(this String message, Exception exception, WebGrabber.Log.Level level)
        {
            message.Log(WebGrabber.Log.Type.Error, level);

            exception.Log(level);
        }
    }
}
