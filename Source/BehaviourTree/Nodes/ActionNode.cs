using Parser;
using Parser.Globals;
using Parser.StaticLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// A behaviour tree leaf node for running an action.
    /// </summary>
    public class ActionNode : IBehaviourTreeNode
    {
        private BehaviourTreeStatus PrevTreeStatus = BehaviourTreeStatus.None;

        /// <summary>
        /// The name of the node.
        /// </summary>
        private string name;

        /// <summary>
        /// Function to invoke for the action.
        /// </summary>
        private Func<BehaviourTreeStatus> fn;
        

        public ActionNode(string name, Func<BehaviourTreeStatus> fn)
        {
            this.name=name;
            this.fn=fn;
        }

        public string GetName()
        {
            return name;
        }

        public BehaviourTreeStatus Tick()
        {
            BehaviourTreeStatus r = fn();
            if (r != PrevTreeStatus || GlobalStatics.PDebug.bShouldPrintBTConstantly)
            {
                switch (r)
                {
                    case BehaviourTreeStatus.Success:
                        Logger.WriteLine($"BehaviourTree: Behaviour \"{name}\" has succeeded.");
                        break;
                    case BehaviourTreeStatus.Failure:
                        Logger.WriteLine($"BehaviourTree: Behaviour \"{name}\" has failed.");
                        break;
                    case BehaviourTreeStatus.Running:
                        Logger.WriteLine($"BehaviourTree: Behaviour \"{name}\" is running.");
                        break;
                }
            }

            PrevTreeStatus = r;
            return r;
        }
    }
}
