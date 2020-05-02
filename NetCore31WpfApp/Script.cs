using MARGO.Common;
using MARGO.UIServices;
using MARGO.ViewModels;
using System;
using System.Collections.Generic;

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
        void StartToPlay(Step name);
        void DoNextStep();
        IEnumerable<SampleGroupVM> SampleGroups { get; }
    }

    internal class Script : IScript
    {
        private readonly static LinkedListNode<KeyValuePair<Step, Action>> notifyFinishedStep = new LinkedListNode<KeyValuePair<Step, Action>>(new KeyValuePair<Step, Action>(Step.NotifyFinished, () => UIServices.ShowInfo("AutoPlay finished.")));
        private readonly static MsgBox UIServices = new MsgBox();
        private readonly IExceptionHandler myExceptionHandler = new ExceptionHandler();
        private readonly LinkedList<KeyValuePair<Step, Action>> mySteps;
        private Step myLastStep;
        private bool isRunning;
        private LinkedListNode<KeyValuePair<Step, Action>> myNextStep;
        public string Name { get; }
        public IEnumerable<SampleGroupVM> SampleGroups { get; private set; }



        internal Script(string name, IEnumerable<SampleGroupVM> sampleGroups, IReadOnlyDictionary<Step, Action> steps)
        {
            Name = name;
            SampleGroups = sampleGroups;
            mySteps = steps is null ? throw new ArgumentNullException(nameof(steps)) : new LinkedList<KeyValuePair<Step, Action>>(steps);
        }



        public void StartToPlay(Step name)
        {
            myLastStep = name;
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
                    myNextStep = null;
                    myExceptionHandler.Handle(ex);
                }
            }
        }

        public override string ToString() => this.Name;
    }
}