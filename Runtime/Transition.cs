using UnityStateTree.Internal;

namespace UnityStateTree
{
    public enum TransitionTrigger
    {
        OnStateCompleted,
        OnStateFailed,
        OnTick
    }

    [System.Serializable]
    public class Transition
    {
        public static Transition DefaultTransition = new TransitionSimple();

        public TransitionTrigger trigger;

        public virtual bool IsValid(IStateTreeContext context) => true;

        public virtual StateEntry ResolveTarget(StateTreeObject stateTree, StateEntry currentState, IStateTreeContext context)
        {
            return stateTree?.rootState?.TrySelect(context);
        }
    }

    [System.Serializable]
    public class TransitionSimple : Transition
    {
        public TransitionTargetType targetType = TransitionTargetType.ToRoot;

        public override StateEntry ResolveTarget(StateTreeObject stateTree, StateEntry currentState, IStateTreeContext context)
        {
            if (stateTree?.rootState == null) return null;

            return targetType switch
            {
                TransitionTargetType.ToRoot => stateTree.rootState.TrySelect(context),
                TransitionTargetType.ToParent => currentState?.parent?.TrySelect(context) ?? stateTree.rootState.TrySelect(context),
                TransitionTargetType.ToNextSibling => ResolveNextSibling(stateTree.rootState, currentState, context),
                _ => stateTree.rootState.TrySelect(context)
            };
        }

        private static StateEntry ResolveNextSibling(StateEntry rootState, StateEntry currentState, IStateTreeContext context)
        {
            var parent = currentState?.parent;
            if (parent == null) return rootState.TrySelect(context);

            var currentIndex = parent.children.IndexOf(currentState);
            if (currentIndex < 0) return rootState.TrySelect(context);

            for (var index = currentIndex + 1; index < parent.children.Count; index++)
            {
                var selected = parent.children[index].TrySelect(context);
                if (selected != null) return selected;
            }

            return parent.TrySelect(context) ?? rootState.TrySelect(context);
        }

        public enum TransitionTargetType
        {
            ToRoot,
            ToParent,
            ToNextSibling,
        }
    }

    [System.Serializable]
    public class TransitionConditionalWithTarget : Transition
    {
        public string targetState;
        public List<Condition> conditions = new();

        public override bool IsValid(IStateTreeContext context)
        {
            return conditions.AllFast(condition => condition.DoEvaluate(context));
        }

        public override StateEntry ResolveTarget(StateTreeObject stateTree, StateEntry currentState, IStateTreeContext context)
        {
            if (stateTree?.rootState == null) return null;
            if (string.IsNullOrWhiteSpace(targetState)) return stateTree.rootState.TrySelect(context);

            var found = FindByName(stateTree.rootState, targetState);
            return found?.TrySelect(context) ?? stateTree.rootState.TrySelect(context);
        }

        private static StateEntry FindByName(StateEntry node, string stateName)
        {
            if (node == null) return null;
            if (node.name == stateName) return node;

            for (var index = 0; index < node.children.Count; index++)
            {
                var found = FindByName(node.children[index], stateName);
                if (found != null) return found;
            }

            return null;
        }
    }
}
