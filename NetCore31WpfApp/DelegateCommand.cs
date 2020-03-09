using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace NetCore31WpfApp
{
    public enum LogEventType
    {
        Fatal = 1,
        Error = 2,
        Warning = 3,
        Info = 4,
        Debug = 5,
        Trace = 6
    }

    public class LogEntry
    {
        public LogEventType LogEventType { get; protected set; }
        public String Message { get; protected set; }
        public object[] Args { get; protected set; }

        public LogEntry(LogEventType logEventType, String message, params object[] args)
        {
            this.LogEventType = logEventType;
            this.Message = message;
            this.Args = args;
        }
    }

    public interface ILoggerImplementation
    {
        void Log(LogEntry logEntry);
    }

    public static class LoggerExtensions
    {
        public static void Fatal(this ILoggerImplementation logger, String message, params object[] args)
        {
            logger.Log(new LogEntry(LogEventType.Fatal, message, args));
        }
        public static void Error(this ILoggerImplementation logger, String message, params object[] args)
        {
            logger.Log(new LogEntry(LogEventType.Error, message, args));
        }
        public static void Warn(this ILoggerImplementation logger, String message, params object[] args)
        {
            logger.Log(new LogEntry(LogEventType.Warning, message, args));
        }
        public static void Info(this ILoggerImplementation logger, String message, params object[] args)
        {
            logger.Log(new LogEntry(LogEventType.Info, message, args));
        }
        public static void Debug(this ILoggerImplementation logger, String message, params object[] args)
        {
            logger.Log(new LogEntry(LogEventType.Debug, message, args));
        }
        public static void Trace(this ILoggerImplementation logger, String message, params object[] args)
        {
            logger.Log(new LogEntry(LogEventType.Trace, message, args));
        }
    }

    public interface IExceptionHandler
    {
        void Handle(Exception exception);
        ILoggerImplementation Logger { get; set; }
    }

    public interface IDelegateCommand : ICommand
    {
        Action CancelAction { get; set; }
        void RaiseCanExecuteChanged(Dispatcher dispatcher = null, bool asynchronously = false);
        IExceptionHandler ExceptionHandler { get; set; }
        ILoggerImplementation Logger { get; set; }
    }

    public interface IDelegateCommand<T> : IDelegateCommand { }

    public class DelegateCommand<T> : IDelegateCommand<T>
    {
        #region Private fields
        private readonly bool isAsync;
        private readonly Func<T, Task> asyncAction = null;
        private readonly Action<T> action = null;
        private readonly Predicate<T> predicate;
        private readonly bool isTypedParameterLess;
        #endregion

        public event EventHandler CanExecuteChanged;

        public IExceptionHandler ExceptionHandler { get; set; }
        public ILoggerImplementation Logger { get; set; }
        public Action CancelAction { get; set; }

        #region Constructors
        public DelegateCommand(Action<T> action, Predicate<T> predicate = null)
            : this(action, predicate, false)
        { }

        protected DelegateCommand(Action<T> action, Predicate<T> predicate, bool isTypedParameterLess)
            : this(false, predicate, isTypedParameterLess)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action), $"The '{nameof(action)}' can't be null!");

            this.action = action;
        }

        protected DelegateCommand(Func<T, Task> asyncAction, Predicate<T> predicate, bool isTypedParameterLess)
            : this(true, predicate, isTypedParameterLess)
        {
            if (asyncAction is null)
                throw new ArgumentNullException(nameof(asyncAction), $"The '{nameof(asyncAction)}' can't be null!");

            this.asyncAction = asyncAction;
        }

        private DelegateCommand(bool isAsync, Predicate<T> predicate, bool isTypedParameterLess)
        {
            if (isTypedParameterLess && (typeof(T) != typeof(Object)))
                throw new ArgumentException($"Type parameter must be 'Object' when '{nameof(isTypedParameterLess)}' is true!", nameof(action));

            this.isAsync = isAsync;
            this.predicate = predicate;
            this.isTypedParameterLess = isTypedParameterLess;
        }
        #endregion

        #region IDelegateCommand implementation
        public bool CanExecute(object parameter)
        {
            if (this.predicate != null)
            {
                if (this.isTypedParameterLess)
                    return this.predicate((T)new Object());
                else
                    return this.predicate((T)parameter);
            }
            else
            {
                if (this.isTypedParameterLess)
                    return true;
                else
                    return parameter != null && parameter is T;
            }
        }

        protected Task CurrentTask { get; private set; }

        public void Execute(object parameter)
        {
            CurrentTask = this.ExecutePrivate(parameter);
        }

        private async Task ExecutePrivate(object parameter)
        {
            try
            {
                if (this.isAsync)
                {
                    if (!this.isTypedParameterLess)
                    {
                        this.Logger?.Info("Command executing asynchronously, with parameter.", parameter);

                        await this.asyncAction((T)parameter);
                    }
                    else
                    {
                        this.Logger?.Info("Command executing asynchronously.");

                        await this.asyncAction((T)new Object());
                    }
                }
                else
                {
                    if (!this.isTypedParameterLess)
                    {
                        this.Logger?.Info("Command executing synchronously, with parameter.", parameter);

                        this.action((T)parameter);
                    }
                    else
                    {
                        this.Logger?.Info($"Command executing synchronously.");

                        this.action((T)new Object());
                    }
                }

                this.Logger?.Info($"Command execution finished.");
            }
            catch (Exception exception)
            {
                if (this.ExceptionHandler is null)
                {
                    this.Logger?.Error($"Command execution faild.");

                    throw;
                }

                this.ExceptionHandler.Handle(exception);
            }
        }


        public void RaiseCanExecuteChanged(Dispatcher dispatcher = null, bool asynchronously = false)
        {
            var handler = this.CanExecuteChanged;

            if (!(handler is null))
            {
                if (dispatcher is null)
                    handler.Invoke(this, new EventArgs());
                else
                {
                    if (asynchronously)
                        dispatcher.BeginInvoke(handler, this, EventArgs.Empty);
                    else
                        dispatcher.Invoke(handler, this, EventArgs.Empty);
                }
            }
        }

        public void CancelMethod() => this.CancelAction?.Invoke();
        #endregion
    }

    public class DelegateCommand : DelegateCommand<Object>
    {
        public DelegateCommand(Action action, Func<bool> predicate = null)
            : base(action is null ? null : new Action<object>((obj) => action()),
                  predicate is null ? null : new Predicate<object>((obj) => predicate()),
                  true)
        { }
    }

    public class DelegateCommandAsync<T> : DelegateCommand<T>
    {
        new public Task CurrentTask => base.CurrentTask;

        public DelegateCommandAsync(Func<T, Task> asyncAction, Predicate<T> predicate = null)
            : base(asyncAction, predicate, false)
        { }

        protected DelegateCommandAsync(Func<T, Task> asyncAction, Predicate<T> predicate, bool isTypedParameterLess)
            : base(asyncAction, predicate, isTypedParameterLess)
        { }
    }

    public class DelegateCommandAsync : DelegateCommandAsync<Object>
    {
        public DelegateCommandAsync(Func<Task> asyncAction, Func<bool> predicate = null)
            : base(asyncAction is null ? null : new Func<object, Task>(async (obj) => await asyncAction()),
                  predicate is null ? null : new Predicate<object>((obj) => predicate()),
                  true)
        { }
    }
}