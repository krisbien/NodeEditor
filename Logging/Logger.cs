using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkiaSharpTestApp.Logging
{
    public static class Logger
    {
        private static StreamWriter? _stream = null;

        enum LogLevel { Trace, Info, Error };

        public static void Trace(string msg)
        {
            LogMessage(LogLevel.Trace, msg);
        }

        private static void LogMessage(LogLevel trace, string msg)
        {
            EnsureOpened();
            _stream?.WriteLine(trace.ToString() + ": " + msg);
            _stream?.Flush();
        }

        private static void EnsureOpened()
        {
            if (_stream != null)
                return;

            var file = GetLoggerFilePath();
            _stream = File.CreateText(file);

            //if (!File.Exists(file))
            //{
            //    _stream = File.CreateText(file);
            //}
            //else
            //{
            //    _stream = new StreamWriter(File.Open(file, FileMode.Append, FileAccess.Write));
            //}
        }

        private static string GetLoggerFilePath()
        {
            return Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"node.editor.log");
        }
    }
}
