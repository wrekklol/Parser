using FluentBehaviourTree;
using Humanizer;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Parser
{
    public abstract class ParserBehaviourBase
    {
        public IBehaviourTreeNode Tree { get; private set; }
        public BehaviourTreeBuilder Builder { get; private set; } = new BehaviourTreeBuilder();

        public object Data { get; private set; }
        public bool bWantsToStop { get; private set; } = false;
        public BehaviourTreeStatus StopStatus { get; private set; } = BehaviourTreeStatus.None;

        protected Random r { get; } = new Random();

        public void Init(object InData)
        {
            Data = InData;

            BuildTree();
            Tree = Builder.Build();
        }

        public void Start()
        {
            bWantsToStop = false;
            StopStatus = BehaviourTreeStatus.None;

            Task.Run(async () =>
            {
                OnStart();
                Logger.WriteLine($"BehaviourTree: Behaviour \"{GetType().Name}\" was started.");

                while (true)
                {
                    BehaviourTreeStatus TreeStatus = Tree.Tick();
                    OnTick();

                    if (TreeStatus == BehaviourTreeStatus.FailureWithStop || TreeStatus == BehaviourTreeStatus.SuccessWithStop)
                        Stop(TreeStatus);

                    await Task.Delay(0).ConfigureAwait(false);
                    if (bWantsToStop)
                    {
                        OnStop();
                        Logger.WriteLine($"BehaviourTree: Behaviour \"{GetType().Name}\" was stopped and it was a {StopStatus.ToString().Humanize(LetterCasing.LowerCase)}.");

                        break;
                    }
                }
            });
        }

        public void Stop(BehaviourTreeStatus InStopStatus = BehaviourTreeStatus.Success)
        {
            bWantsToStop = true;
            StopStatus = InStopStatus;
        }

        public bool IsRunning()
        {
            return !bWantsToStop && StopStatus != BehaviourTreeStatus.None;
        }

        protected abstract void BuildTree();
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnTick() { }
    }
}
