using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Yanitta
{
    public class ConsoleWriter : TextWriter
    {
        static ConsoleWriter Instance;
        StreamWriter m_writer;

        public ConsoleWriter(string fileName, bool isRegisterUnhandledException)
        {
            m_writer = new StreamWriter(Path.Combine(Environment.CurrentDirectory, fileName), false);
            m_writer.AutoFlush = true;
            Console.SetOut(this);
            Debug.Listeners.Add(new TextWriterTraceListener(this));
            Trace.Listeners.Add(new TextWriterTraceListener(this));

            if (isRegisterUnhandledException)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            }

            m_writer.WriteLine($"===================== START {DateTime.Now:dd.MM.yyyy} =====================");
        }

        public static void Initialize(string fileName, bool isRegisterUnhandledException = false)
        {
            if (Instance != null)
                throw new InvalidOperationException("ConsoleWriter is already initialized.");

            Instance = new ConsoleWriter(fileName, isRegisterUnhandledException);
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value) => InternalWrite(value + Environment.NewLine);

        public override void WriteLine(string format, params object[] arg) => InternalWrite(string.Format(format, arg) + Environment.NewLine);

        public override void WriteLine() => InternalWrite(Environment.NewLine);

        public override void Write(string value) => InternalWrite(value);

        public override void Write(string format, params object[] arg) => InternalWrite(string.Format(format, arg));

        void InternalWrite(string text) => m_writer?.Write($"[{DateTime.Now:HH:mm:ss.fff}] {text}");

        public static void CloseWriter() => Instance?.Close();

        void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
                Console.WriteLine(exception);
        }

        public override void Close()
        {
            base.Close();
            if (m_writer != null)
            {
                m_writer.WriteLine($"====================== END {DateTime.Now:dd.MM.yyyy} ======================");
                Debug.Listeners.Clear();
                Trace.Listeners.Clear();
                m_writer.Close();
                m_writer = null;
            }
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }
    }
}