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
        [UnityEngine.SerializeReference] public Condition condition;

        public NotCondition(){}
        
        public NotCondition(Condition condition)
        {
            this.condition = condition;
        }

        public override bool Evaluate(IStateTreeContext context)
        {
            return !condition.DoEvaluate(context);
        }
    }

    [System.Serializable]
    public class Condition<TContext> : Condition
    {
        public sealed override bool Evaluate(IStateTreeContext context)
        {
            return Evaluate(((IStateTreeContext<TContext>)context).TypedContext);
        }

        public virtual bool Evaluate(TContext context)
        {
            return true;
        }
    }
}