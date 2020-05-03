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
            bool canceled = exception is OperationCanceledException
                        || exception is System.Threading.Tasks.TaskCanceledException;

            string message;
            string caption;
            MessageBoxImage messageBoxImage;
            MessageBoxButton messageBoxButton;
            if (canceled)
            {
                caption = "Felhasználói megszakítás";
                message = "A folyamat leállt.";
                messageBoxButton = MessageBoxButton.OK;
                messageBoxImage = MessageBoxImage.Exclamation;
            }
            else
            {
                caption = "Váratlan hiba történt!";
                message = $"[{exception.Message}]\n\nKívánja a hiba technikai részleteit a vágólapra másolni?";
                messageBoxButton = MessageBoxButton.YesNo;
                messageBoxImage = MessageBoxImage.Error;
            }

            if (MessageBox.Show(message, caption, messageBoxButton, messageBoxImage, MessageBoxResult.Yes) == MessageBoxResult.Yes)
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
