using UnityStateTree.Internal;

namespace UnityStateTree
{
    /// <summary>
    /// The StateTreeRunner is responsible for managing the execution of a StateTreeObject. It
    /// handles state transitions, task execution, and ensures that the correct tasks are active based on the current state.
    /// - it contains the full state, everything else is stateless
    /// - it is not a MonoBehaviour, so it can be tested more easily
    /// </summary>
    public class StateTreeRunner
    {
        public StateTreeObject stateTree;
        public IStateTreeContext context;

        private StateEntry currentState;
        public StateEntry CurrentState => currentState;

        public void OnEnable(StateTreeObject stateTree, IStateTreeContext context)
        {
            this.stateTree = stateTree ?? throw new ArgumentNullException(nameof(stateTree));
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            EnterState(stateTree.rootState.TrySelect(context));
        }

        private void ExitStateUntilCommonAncestor(StateEntry newState)
        {
            if (newState == null) return;
            while (currentState != null && !IsAncestorOf(currentState, newState))
            {
                currentState.tasks.ForEach(task => task.OnExitState(context));
                currentState = currentState.parent;
            }

            bool IsAncestorOf(StateEntry ancestor, StateEntry descendant)
            {
                var parent = descendant.parent;
                while (parent != null)
                {
                    if (parent == ancestor) return true;
                    parent = parent.parent;
                }
                return false;
            }
        }

        private void EnterState(StateEntry newState)
        {
            if (newState == null) return;
            ExitStateUntilCommonAncestor(newState);
            EnterStateRecursive(newState);

            void EnterStateRecursive(StateEntry state)
            {
                if (state == currentState) return;
                if (state.parent != null && state.parent != currentState)
                    EnterStateRecursive(state.parent);
                currentState = state;
                foreach (var task in currentState.tasks)
                {
                    switch (task.OnEnterState(context))
                    {
                        case TaskStatus.Running: break;
                        case TaskStatus.Success: CompleteState(TransitionTrigger.OnStateCompleted); break;
                        case TaskStatus.Failure: CompleteState(TransitionTrigger.OnStateFailed); break;
                        case TaskStatus.Interrupted: CompleteState(TransitionTrigger.OnStateFailed); break;
                        default: throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private Transition queuedTransition = null;
        private void CompleteState(TransitionTrigger trigger)
        {
            queuedTransition = currentState.transitions
                               .FirstOrDefaultFast(t => t.trigger == trigger && t.IsValid(context))
                               ?? Transition.DefaultTransition;
        }

        public void Update()
        {
            if (currentState == null) return;

            if (queuedTransition != null)
            {
                return;
            }

            foreach (var currentStateTask in currentState.tasks)
            {
                currentStateTask.OnTick(context);
            }
        }
    }
}
