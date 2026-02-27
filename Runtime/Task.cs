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
}
