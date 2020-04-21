using System;
using System.Windows;
using System.Windows.Threading;

namespace MARGO.Common
{
    public interface IExceptionHandler
    {
        void Handle(Exception exception);
        //ILoggerImplementation Logger { get; set; }
    }

    internal class ExceptionHandler : IExceptionHandler
    {
        private readonly Dispatcher myDispatcher = Dispatcher.CurrentDispatcher;

        public bool Throws { get; set; }


        public ExceptionHandler(bool throws = false)
        {
            this.Throws = throws;
        }


        public void Handle(Exception exception)
        {
            if (MessageBox.Show($"[{exception.Message}]\nKívánja a hiba technikai részleteit a vágólapra másolni?", "Váratlan hiba történt!", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.No) == MessageBoxResult.Yes)
                myDispatcher.Invoke(() => this.CopyToClipBoard(exception));

            if (this.Throws)
                throw exception;
        }

        protected void CopyToClipBoard(Exception exception)
        {
            string message = exception.ToString();
            Clipboard.Clear();
            Clipboard.SetText(message);
            Clipboard.Flush();
        }
    }
}
