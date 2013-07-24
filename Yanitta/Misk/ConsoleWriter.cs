using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Yanitta
{
    public delegate void ConsoleWriteEventHandler(object sender, ConsoleWriteEventArgs args);

    public static class ConsoleWriter
    {
        private class WriterImpl : TextWriter
        {
            private FileStream m_stream;
            private StreamWriter m_writer;

            public WriterImpl()
            {
                try
                {
                    m_stream = new FileStream(Assembly.GetEntryAssembly().GetName().Name + ".log", FileMode.Create);
                    m_writer = new StreamWriter(m_stream, this.Encoding);
                    m_writer.AutoFlush = true;
                }
                catch
                {
                    m_stream = null;
                    m_writer = null;
                }
            }

            #region Overrides

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
                    m_writer.Close();
                    m_writer = null;
                }

                if (m_stream != null)
                {
                    m_stream.Close();
                    m_stream = null;
                }
            }

            #endregion Overrides

            private void InternalWrite(string text)
            {
                if (text.StartsWith("debug", StringComparison.InvariantCultureIgnoreCase))
                    return;

                OnConsoleWrite(text);

                if (m_writer != null)
                    m_writer.WriteLine(string.Format("[{0:HH:mm:ss.fff}] {1}", DateTime.Now, text));
            }
        }

        private static WriterImpl m_impl;

        public static void Initialize(bool isRegisterUnhandledException = false)
        {
            if (m_impl != null)
                throw new InvalidOperationException("ConsoleWriter is already initialized.");

            m_impl = new WriterImpl();

            Console.SetOut(m_impl);
            Console.SetError(m_impl);

            if (isRegisterUnhandledException)
            {
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                // если необходимо, то тут можно задать более подробную разшифровку сообщения
                Console.WriteLine(exception);
            }
        }

        public static void Close()
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            m_impl.Close();
            m_impl = null;
        }

        private static void OnConsoleWrite(string message)
        {
            if (ConsoleWrite != null)
                ConsoleWrite(m_impl, new ConsoleWriteEventArgs(message));
        }

        public static event ConsoleWriteEventHandler ConsoleWrite;
    }

    public sealed class ConsoleWriteEventArgs : EventArgs
    {
        public ConsoleWriteEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }
    }
}