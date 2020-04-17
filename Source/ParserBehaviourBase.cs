using FluentBehaviourTree;
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

        protected Random r { get; } = new Random();

        public void Init(object InData)
        {
            Data = InData;

            BuildTree();
            Tree = Builder.Build();
        }

        public void Start()
        {
            //Thread thread1 = new Thread(() =>
            Task.Run(async () =>
            {
                OnStart();
                Logger.WriteLine($"BehaviourTree: Behaviour \"{GetType().Name}\" was started.");

                while (true)
                {
                    Tree.Tick();
                    OnTick();
                    await Task.Delay(0).ConfigureAwait(false);

                    if (bWantsToStop)
                    {
                        OnStop();
                        Logger.WriteLine($"BehaviourTree: Behaviour \"{GetType().Name}\" was stopped.");

                        break;
                    }
                }
            });
            //);
            //thread1.Start();
        }

        public void Stop()
        {
            bWantsToStop = true;
        }

        protected abstract void BuildTree();
        protected virtual void OnStart() { }
        protected virtual void OnStop() { }
        protected virtual void OnTick() { }
    }
}
