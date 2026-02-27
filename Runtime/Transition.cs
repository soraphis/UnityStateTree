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
    }

    [System.Serializable]
    public class TransitionSimple : Transition
    {
        public TransitionTargetType targetType = TransitionTargetType.ToRoot;

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
    }
}
