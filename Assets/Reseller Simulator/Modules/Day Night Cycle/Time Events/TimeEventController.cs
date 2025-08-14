using UnityEngine;

public class TimeEventController : MonoBehaviour
{
    public TimeEventSO[] timeEvents;
    private TimeManager timeManager;

    void Awake()
    {
        timeManager = GameManager.Instance.Time;
    }

    void Update()
    {
        if (GameManager.Instance == null || UIManager.Instance == null)
            return; // skip until ready

        int currentHour = Mathf.FloorToInt(GameManager.Instance.Time.currentTime);

        foreach (var evt in timeEvents)
            evt.TryTrigger(currentHour, GameManager.Instance.Time.currentDay);
    }


    public void ResetEvents()
    {
        foreach (var evt in timeEvents)
        {
            evt.ResetForNewDay();
        }
    }
}