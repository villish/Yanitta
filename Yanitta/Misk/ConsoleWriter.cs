using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Yanitta
{
    public class ConsoleWriter : TextWriter
    {
        private static ConsoleWriter Instance;
        private StreamWriter m_writer;

        public ConsoleWriter(string fileName, bool isRegisterUnhandledException)
        {
            m_writer = new StreamWriter(fileName, false);
            m_writer.AutoFlush = true;
            Console.SetOut(this);
            Debug.Listeners.Add(new TextWriterTraceListener(this));

            if (isRegisterUnhandledException)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            }
        }

        public static void Initialize(string fileName, bool isRegisterUnhandledException = false)
        {
            if (Instance != null)
                throw new InvalidOperationException("ConsoleWriter is already initialized.");

            Instance = new ConsoleWriter(fileName, isRegisterUnhandledException);
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }

        public override void WriteLine(string value)
        {
            InternalWrite(value + Environment.NewLine);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            InternalWrite(string.Format(format, arg) + Environment.NewLine);
        }

        public override void WriteLine()
        {
            InternalWrite(Environment.NewLine);
        }

        public override void Write(string value)
        {
            InternalWrite(value);
        }

        public override void Write(string format, params object[] arg)
        {
            InternalWrite(string.Format(format, arg));
        }

        public override void Close()
        {
            base.Close();
            if (m_writer != null)
            {
                Debug.Listeners.Clear();
                m_writer.Close();
                m_writer = null;
            }
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        }

        private void InternalWrite(string text)
        {
            if (m_writer != null)
                m_writer.Write("[{0:HH:mm:ss.fff}] {1}", DateTime.Now, text);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
                Console.WriteLine(exception);
        }

        public static void CloseWriter()
        {
            if (Instance != null)
                Instance.Close();
        }
    }
}