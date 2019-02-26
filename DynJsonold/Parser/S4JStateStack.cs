using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JStateStack
    {
        public List<S4JState> Stack { get; set; }
            = new List<S4JState>();

        public List<S4JState> History { get; set; }
            = new List<S4JState>();

        public S4JState Peek()
        {
            return this.Stack.LastOrDefault();
        }

        public void Push(S4JState State)
        {
            this.Stack.Add(State);
            this.History.Add(State);
        }

        public void Pop()
        {
            if (Stack.Count > 0)
                this.Stack.RemoveAt(this.Stack.Count - 1);
        }
    }
}
