using System.Collections;
using UnityEngine;

public class DayTransitionController : MonoBehaviour
{
    private TimeManager timeManager;
    public ScreenFader screenFader;

    public void Awake()
    {
        timeManager = GameManager.Instance.Time;
    }

    public void HandleDayEnd()
    {
        StartCoroutine(StartNewDayRoutine());
    }

    private IEnumerator StartNewDayRoutine()
    {
        yield return screenFader.FadeOut();
        yield return timeManager.StartNewDay();
        yield return screenFader.FadeIn();
    }
}
