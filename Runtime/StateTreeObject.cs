namespace UnityStateTree
{
    [System.Serializable]
    public class StateTreeObject
    {
        public StateEntry rootState = new(){name = "Root", depth = 0, selectionBehavior = SelectionBehavior.SelectChildrenInOrder};
    }
}
