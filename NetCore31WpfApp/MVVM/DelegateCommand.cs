using System;
using System.Windows.Input;

namespace MARGO.MVVM
{
    public class DelegateCommand<T> : ICommand
    {
        #region Private fields
        private readonly Action<T> action = null;
        private readonly Predicate<T> predicate;
        private readonly bool isGeneric;
        #endregion

        public event EventHandler CanExecuteChanged;

        #region Constructors
        public DelegateCommand(Action<T> action, Predicate<T> predicate = null)
            : this(action, predicate, true)
        { }

        protected DelegateCommand(Action<T> action, Predicate<T> predicate, bool isGeneric)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action), $"The '{nameof(action)}' can't be null!");

            this.action = action;
            this.predicate = predicate;
            this.isGeneric = isGeneric;
        }
        #endregion

        public bool CanExecute(object parameter)
        {
            if (this.predicate != null)
            {
                if (this.isGeneric)
                    return this.predicate((T)parameter);
                else
                    return this.predicate((T)new object());
            }
            else
            {
                if (this.isGeneric)
                    return parameter != null && parameter is T;
                else
                    return true;
            }
        }

        public void Execute(object parameter)
        {
            if (this.isGeneric)
                this.action((T)parameter);
            else
                this.action((T)new object());
        }
    }

    internal class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action action, Func<bool> predicate = null)
            : base(action is null ? null : new Action<object>((obj) => action()),
                  predicate is null ? null : new Predicate<object>((obj) => predicate()),
                  false)
        { }
    }
}
