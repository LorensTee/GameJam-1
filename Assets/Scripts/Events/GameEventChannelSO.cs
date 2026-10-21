
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "NewGameEvent",
    menuName = "Game/Events/Game Event Channel"
)]
public class GameEventChannelSO : ScriptableObject
{
    private UnityAction listeners;

    public void RegisterListener(UnityAction listener)
    {
        listeners += listener;
    }

    public void UnregisterListener(UnityAction listener)
    {
        listeners -= listener;
    }

    public void RaiseEvent()
    {
        listeners?.Invoke();
    }
}
