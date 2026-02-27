using UnityStateTree.Internal;


namespace UnityStateTree{
    
    [System.Serializable]
    public class Condition
    {

        internal virtual bool DoEvaluate(IStateTreeContext context)
        {
            return Evaluate(context);
        }

        public virtual bool Evaluate(IStateTreeContext context)
        {
            return true;
        }
    }

    [System.Serializable]
    public sealed class NotCondition : Condition
    {
        public Condition condition;

        public NotCondition(Condition condition)
        {
            this.condition = condition;
        }

        public override bool Evaluate(IStateTreeContext context)
        {
            return !condition.DoEvaluate(context);
        }
    }
}