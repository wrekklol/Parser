using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// Runs child nodes in sequence, until one fails.
    /// </summary>
    public class SequenceNode : IParentBehaviourTreeNode
    {
        /// <summary>
        /// Name of the node.
        /// </summary>
        private string name;
        private string SkipToName = "";

        /// <summary>
        /// List of child nodes.
        /// </summary>
        private List<IBehaviourTreeNode> children = new List<IBehaviourTreeNode>(); //todo: this could be optimized as a baked array.

        public SequenceNode(string name)
        {
            this.name = name;
        }

        public string GetName()
        {
            return name;
        }

        public void SkipTo(string InNodeName)
        {
            SkipToName = InNodeName;
        }

        public BehaviourTreeStatus Tick()
        {
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child.GetName() != SkipToName)
                    continue;

                var childStatus = child.Tick();
                switch (childStatus)
                {
                    case BehaviourTreeStatus.None:
                    case BehaviourTreeStatus.Failure:
                    case BehaviourTreeStatus.FailureWithStop:
                        return BehaviourTreeStatus.FailureWithStop; //todo: add this to the other nodes
                    case BehaviourTreeStatus.SuccessWithStop:
                        return BehaviourTreeStatus.SuccessWithStop;
                    case BehaviourTreeStatus.Running:
                        i--;
                        break;
                }
            }
            //foreach (var child in children)
            //{
            //    var childStatus = child.Tick();
            //    if (childStatus != BehaviourTreeStatus.Success)
            //    {
            //        return childStatus;
            //    }
            //}

            return BehaviourTreeStatus.Success;
        }

        /// <summary>
        /// Add a child to the sequence.
        /// </summary>
        public void AddChild(IBehaviourTreeNode child)
        {
            children.Add(child);
        }
    }
}
