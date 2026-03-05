using System.Collections.Generic;
using UnityStateTree.Internal;

namespace UnityStateTree
{
    public enum SelectionBehavior
    {
        None,
        SelectChildrenInOrder = 1
    }

    [System.Serializable]
    public class StateEntry
    {
        public string name;
        public SelectionBehavior selectionBehavior = SelectionBehavior.SelectChildrenInOrder;
        public List<StateEntry> children = new();
        [UnityEngine.SerializeReference] public List<Condition> entryConditions = new();
        [UnityEngine.SerializeReference] public List<Task> tasks = new();
        [UnityEngine.SerializeReference] public List<Transition> transitions = new();

        [UnityEngine.SerializeReference] public StateEntry parent;
        public int depth = -1;

        public StateEntry WithChild(StateEntry child)
        {
            children.Add(child);
            child.parent = this;
            child.depth = this.depth + 1;
            return this;
        }

        private bool EvaluateConditions(IStateTreeContext context)
        {
            return entryConditions.AllFast(entryCondition => entryCondition.DoEvaluate(context));
        }

        public bool TryEvaluate()
        {
            return selectionBehavior switch
            {
                SelectionBehavior.SelectChildrenInOrder => TryEvaluateChildrenInOrder(),
                SelectionBehavior.None => true,
                _ => false
            };
        }

        private bool TryEvaluateChildrenInOrder()
        {
            return children.AnyFast(stateEntry => stateEntry.TryEvaluate());
        }

        public StateEntry TrySelect(IStateTreeContext context)
        {
            if (!EvaluateConditions(context)) return null;
            if (selectionBehavior == SelectionBehavior.None) return this;
            foreach (var child in children)
            {
                var selected = child.TrySelect(context);
                if (selected != null) return selected;
            }
            if (selectionBehavior == SelectionBehavior.SelectChildrenInOrder && children.Count == 0) return this;
            return null;
        }
    }


}
