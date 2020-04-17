using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MARGO.Common
{
    public interface IDelegateCommand : ICommand
    {
        IExceptionHandler ExceptionHandler { get; set; }
        Action Finaly { get; set; }
        //ILoggerImplementation Logger { get; set; }
        //Action CancelAction { get; set; }
        //void RaiseCanExecuteChanged(Dispatcher dispatcher = null, bool asynchronously = false);
    }

    public interface IDelegateCommand<T> : IDelegateCommand { }
    public class DelegateCommand<T> : IDelegateCommand<T>
    {
        #region Private fields
        private readonly bool isAsync;
        private readonly Func<T, Task> asyncAction = null;
        private readonly Action<T> action = null;
        private readonly Predicate<T> predicate;
        private readonly bool isGeneric;
        #endregion

        public event EventHandler CanExecuteChanged;

        public IExceptionHandler ExceptionHandler { get; set; } = new ExceptionHandler();
        public Action Finaly { get; set; }


        #region Constructors
        public DelegateCommand(Action<T> action, Predicate<T> predicate = null)
            : this(action, predicate, true)
        { }

        protected DelegateCommand(Action<T> action, Predicate<T> predicate, bool isGeneric)
            : this(false, predicate, isGeneric)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action), $"The '{nameof(action)}' can't be null!");

            this.action = action;
        }

        protected DelegateCommand(Func<T, Task> asyncAction, Predicate<T> predicate, bool isGeneric)
            : this(true, predicate, isGeneric)
        {
            if (asyncAction is null)
                throw new ArgumentNullException(nameof(asyncAction), $"The '{nameof(asyncAction)}' can't be null!");

            this.asyncAction = asyncAction;
        }

        private DelegateCommand(bool isAsync, Predicate<T> predicate, bool isGeneric)
        {
            if (!isGeneric && (typeof(T) != typeof(Object)))
                throw new ArgumentException($"Type parameter must be 'Object' when '{nameof(isGeneric)}' is true!", nameof(action));

            this.isAsync = isAsync;
            this.predicate = predicate;
            this.isGeneric = isGeneric;
        }
        #endregion

        public bool CanExecute(object parameter)
        {
            bool canExecute = false;

            try
            {
                if (this.predicate != null)
                {
                    if (this.isGeneric)
                        canExecute = this.predicate((T)parameter);
                    else
                        canExecute = this.predicate((T)new object());
                }
                else
                {
                    if (this.isGeneric)
                        canExecute = parameter is T;
                    else
                        canExecute = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler?.Handle(ex);
            }
            finally { Finaly?.Invoke(); }

            return canExecute;
        }

        public async void Execute(object parameter)
        {
            Task t = null;
            try
            {
                if (this.isAsync)
                {

                    if (this.isGeneric)
                    {
                        t = this.asyncAction((T)parameter);
                    }
                    else
                    {
                        t = this.asyncAction((T)new Object());
                    }

                    await t;
                }
                else
                {

                    if (this.isGeneric)
                    {
                        this.action((T)parameter);
                    }
                    else
                    {
                        this.action((T)new Object());
                    }
                }
            }
            catch (Exception exception)
            {
                if (this.ExceptionHandler != null)
                {
                    if (t?.Exception != null)
                        this.ExceptionHandler.Handle(t.Exception);
                    else
                        this.ExceptionHandler.Handle(exception);
                }
                else
                    throw;
            }
            finally { Finaly?.Invoke(); }
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action action, Func<bool> predicate = null)
            : base(action is null ? null : new Action<object>((obj) => action()),
                  predicate is null ? null : new Predicate<object>((obj) => predicate()),
                  false)
        { }
    }

    public class DelegateCommandAsync<T> : DelegateCommand<T>
    {
        public DelegateCommandAsync(Func<T, Task> asyncAction, Predicate<T> predicate = null)
            : base(asyncAction, predicate, true)
        { }

        protected DelegateCommandAsync(Func<T, Task> asyncAction, Predicate<T> predicate, bool isGeneric)
            : base(asyncAction, predicate, isGeneric)
        { }
    }

    public class DelegateCommandAsync : DelegateCommandAsync<object>
    {
        public DelegateCommandAsync(Func<Task> asyncAction, Func<bool> predicate = null)
            : base(asyncAction is null ? null : new Func<object, Task>(async (obj) => await asyncAction()),
                  predicate is null ? null : new Predicate<object>((obj) => predicate()),
                  false)
        { }
    }
}
