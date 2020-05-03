using MARGO.Common;
using MARGO.UIServices;
using MARGO.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MARGO
{
    public enum Step
    {
        Load,
        Compose,
        Cut,
        Variants,
        Minimas,
        Flood,
        Classify,
        NotifyFinished
    }

    public interface IScript
    {
        string Name { get; }
        void StartToPlay(Step name, CancellationTokenSource tokenSource);
        void DoNextStep();
        IEnumerable<SampleGroupVM> SampleGroups { get; }
    }

    internal class Script : IScript
    {


        private readonly static MsgBox UIServices = new MsgBox();
        private readonly IExceptionHandler myExceptionHandler = new ExceptionHandler();
        private readonly LinkedList<KeyValuePair<Step, Action>> mySteps;
        private readonly LinkedListNode<KeyValuePair<Step, Action>> notifyFinishedStep;
        private Step myLastStep;
        private bool isRunning;
        private LinkedListNode<KeyValuePair<Step, Action>> myNextStep;
        public string Name { get; }
        public IEnumerable<SampleGroupVM> SampleGroups { get; private set; }



        internal Script(string name, IEnumerable<SampleGroupVM> sampleGroups, Action deleteTokenSource, IReadOnlyDictionary<Step, Action> steps)
        {
            Name = name;
            SampleGroups = sampleGroups;
            myDeleteTokenSource = deleteTokenSource;
            mySteps = steps is null ? throw new ArgumentNullException(nameof(steps)) : new LinkedList<KeyValuePair<Step, Action>>(steps);
            notifyFinishedStep = new LinkedListNode<KeyValuePair<Step, Action>>(new KeyValuePair<Step, Action>(Step.NotifyFinished, () => this.LastStep()));
        }

        private Action myDeleteTokenSource;
        private CancellationTokenSource tokenSourceByRef;
        private CancellationToken myToken;
        public void StartToPlay(Step name, CancellationTokenSource tokenSource)
        {
            myLastStep = name;
            tokenSourceByRef = tokenSource;
            myToken = tokenSourceByRef.Token;
            myNextStep = mySteps.First;
            isRunning = true;
            DoNextStep();
        }

        public void DoNextStep()
        {

            if (isRunning && myNextStep != null)
            {
                try
                {
                    myToken.ThrowIfCancellationRequested();

                    var currentAction = myNextStep.Value.Value;

                    isRunning = Step.NotifyFinished != myNextStep.Value.Key;

                    if (isRunning)
                    {
                        if (myNextStep.Value.Key == myLastStep)
                        {
                            myNextStep = notifyFinishedStep;
                        }
                        else
                        {
                            myNextStep = myNextStep.Next;
                        }
                    }
                    else
                    {
                        myNextStep = null;
                    }

                    currentAction.Invoke();
                }
                catch (Exception ex)
                {
                    isRunning = false;
                    tokenSourceByRef?.Dispose();
                    myDeleteTokenSource();
                    myNextStep = null;
                    myExceptionHandler.Handle(ex);
                }
            }
        }

        private void LastStep()
        {
            tokenSourceByRef?.Dispose();
            myDeleteTokenSource();
            UIServices.ShowInfo("AutoPlay finished.");
        }

        public override string ToString() => this.Name;
    }
}