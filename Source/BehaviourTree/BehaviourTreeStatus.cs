using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentBehaviourTree
{
    /// <summary>
    /// The return type when invoking behaviour tree nodes.
    /// </summary>
    public enum BehaviourTreeStatus
    {
        None,

        Success,
        Failure,
        Running,

        SuccessWithStop,
        FailureWithStop
    }
}
