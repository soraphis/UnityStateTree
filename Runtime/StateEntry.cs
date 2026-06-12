using System.Collections.Generic;
using UnityStateTree.Internal;

namespace UnityStateTree
{
    [System.Serializable]
    public abstract class StateEntry
    {
        public string name;
        [UnityEngine.SerializeReference] public List<StateEntry> children = new();
        [UnityEngine.SerializeReference] public List<Condition> entryConditions = new();
        [UnityEngine.SerializeReference] public List<Task> tasks = new();
        [UnityEngine.SerializeReference] public List<Transition> transitions = new();

        [UnityEngine.SerializeReference] public StateEntry parent;
        public int depth = -1;

        public bool IsLeaf => this is ActionState;

        private bool EvaluateConditions(IStateTreeContext context)
        {
            for (var i = 0; i < entryConditions.Count; i++)
            {
                if (!entryConditions[i].DoEvaluate(context)) return false;
            }
            return true;
        }

        public bool TryEvaluate()
        {
            if (IsLeaf) return true;
            return children.AnyFast(stateEntry => stateEntry.TryEvaluate());
        }

        public StateEntry TrySelect(IStateTreeContext context)
        {
            if (!EvaluateConditions(context)) return null;
            if (IsLeaf) return this;
            return SelectChild(context);
        }

        protected virtual StateEntry SelectChild(IStateTreeContext context) => null;
    }

    /// <summary>
    /// A leaf/action node that executes tasks. Cannot have children.
    /// </summary>
    [System.Serializable]
    public sealed class ActionState : StateEntry { }

    /// <summary>
    /// Base for selector nodes that pick among children.
    /// </summary>
    [System.Serializable]
    public abstract class SelectorState : StateEntry
    {
        public SelectorState WithChild(StateEntry child)
        {
            children.Add(child);
            child.parent = this;
            child.depth = this.depth + 1;
            return this;
        }
    }

    /// <summary>
    /// Selects the first child whose conditions pass, evaluated in order.
    /// </summary>
    [System.Serializable]
    public sealed class SelectInOrder : SelectorState
    {
        protected override StateEntry SelectChild(IStateTreeContext context)
        {
            foreach (var child in children)
            {
                var selected = child.TrySelect(context);
                if (selected != null) return selected;
            }
            return children.Count == 0 ? this : null;
        }
    }

    /// <summary>
    /// Shuffles children and selects the first one whose conditions pass.
    /// </summary>
    [System.Serializable]
    public sealed class SelectAtRandom : SelectorState
    {
        protected override StateEntry SelectChild(IStateTreeContext context)
        {
            if (children.Count == 0) return this;

            // Fisher-Yates in-place on a temp span would be ideal,
            // but List index swap is fine for small child counts.
            var count = children.Count;
            for (var i = count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (children[i], children[j]) = (children[j], children[i]);
            }

            foreach (var child in children)
            {
                var selected = child.TrySelect(context);
                if (selected != null) return selected;
            }
            return null;
        }
    }
}
