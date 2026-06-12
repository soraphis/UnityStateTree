namespace UnityStateTree
{
    [System.Serializable]
    public class StateTreeObject
    {
        [UnityEngine.SerializeReference] public StateEntry rootState = new SelectInOrder(){name = "Root", depth = 0};
    }
}
