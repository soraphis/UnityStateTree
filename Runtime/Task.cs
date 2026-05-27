namespace UnityStateTree
{
    public enum TaskStatus
    {
        Running,
        Success,
        Failure,
        Interrupted,
    }

    [System.Serializable]
    public class Task
    {
        public virtual TaskStatus OnEnterState(IStateTreeContext context)
        {
            return TaskStatus.Running;
        }

        public virtual TaskStatus OnTick(IStateTreeContext context)
        {
            return TaskStatus.Success;
        }

        public virtual void OnExitState(IStateTreeContext context)
        {
        }
    }

    [System.Serializable]
    public class PassiveTask
    {
        public virtual TaskStatus OnTick(IStateTreeContext context)
        {
            return TaskStatus.Success;
        }
    }

    [System.Serializable]
    public class Task<TContext> : Task
    {
        public sealed override TaskStatus OnEnterState(IStateTreeContext context)
        {
            return OnEnterState(((IStateTreeContext<TContext>)context).TypedContext);
        }

        public sealed override TaskStatus OnTick(IStateTreeContext context)
        {
            return OnTick(((IStateTreeContext<TContext>)context).TypedContext);
        }

        public sealed override void OnExitState(IStateTreeContext context)
        {
            OnExitState(((IStateTreeContext<TContext>)context).TypedContext);
        }

        public virtual TaskStatus OnEnterState(TContext context)
        {
            return TaskStatus.Running;
        }

        public virtual TaskStatus OnTick(TContext context)
        {
            return TaskStatus.Success;
        }

        public virtual void OnExitState(TContext context)
        {
        }
    }

    [System.Serializable]
    public class PassiveTask<TContext> : PassiveTask
    {
        public sealed override TaskStatus OnTick(IStateTreeContext context)
        {
            return OnTick(((IStateTreeContext<TContext>)context).TypedContext);
        }

        public virtual TaskStatus OnTick(TContext context)
        {
            return TaskStatus.Success;
        }
    }
}
