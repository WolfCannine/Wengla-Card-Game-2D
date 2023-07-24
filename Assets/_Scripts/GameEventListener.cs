using UnityEngine;
using UnityEngine.Events;


public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;
    public UnityEvent onEventTriggerd;


    private void OnEnable()
    {
        gameEvent.AddListener(this);
    }

    public void OnEventTriggered()
    {
        onEventTriggerd.Invoke();
    }

    private void OnDisable()
    {
        gameEvent.RemoveListener(this);
    }
}
