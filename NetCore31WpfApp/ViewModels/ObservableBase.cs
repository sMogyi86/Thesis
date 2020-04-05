using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace MARGO.ViewModels
{
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Dispatcher for raising the PropertyChanged event
        /// </summary>
        public Dispatcher Dispatcher { get; set; } // = Dispatcher.CurrentDispatcher;

        /// <summary>
        /// If Dispatcher is not null, call the handler with Dispatcher.BeginInvoke (instead of Invoke)
        /// default: false
        /// </summary>
        public bool Asynchronously { get; set; } = false;

        /// <summary>
        /// Property to switch on or off the PropertyChanged event raising. (e.g. massive calculations)
        /// default: true
        /// </summary>
        public bool Notifies { get; set; } = true;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.Notifies)
            {
                var handler = this.PropertyChanged;

                if (handler != null)
                {
                    if (this.Dispatcher is null)
                        handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    else if (this.Asynchronously)
                        this.Dispatcher.BeginInvoke(handler, this, new PropertyChangedEventArgs(propertyName));
                    else
                        this.Dispatcher.Invoke(handler, this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        protected virtual void RaiseForAll() => this.RaisePropertyChanged(String.Empty);
    }
}
