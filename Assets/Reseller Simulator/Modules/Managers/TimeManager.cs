using System;
using System.Collections;
using Pinwheel.Jupiter;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("Day Settings")]
    public int currentDay = 1;
    public float startHour = 7f;
    public float endHour = 24f;
    public float dayLengthMinutes = 20f;

    [Header("References")]
    public JDayNightCycle skyController;

    public event Action OnDayStart;
    public event Action<int> OnHourChanged; // Pass the current hour when fired

    private int lastHour = -1;

    public float currentTime;
    public float timeSpeed;
    private bool dayRunning = false;

    private void Start()
    {
        timeSpeed = (endHour - startHour) / (dayLengthMinutes * 60f);
        currentTime = startHour;

        UpdateSkyTime();
        dayRunning = true;
        OnDayStart?.Invoke();
    }

    private void Update()
    {
        if (!dayRunning) return;

        currentTime += Time.deltaTime * timeSpeed;

        int currentHourInt = Mathf.FloorToInt(currentTime);
        if (currentHourInt != lastHour)
        {
            lastHour = currentHourInt;
            OnHourChanged?.Invoke(currentHourInt);
        }
        if (currentTime >= endHour)
        {

            currentTime = endHour;
            dayRunning = false;
        }
        else
        {
            UpdateSkyTime();
        }
    }

    public IEnumerator StartNewDay()
    {
        currentDay++;
        currentTime = startHour;
        UpdateSkyTime();
        dayRunning = true;
        OnDayStart?.Invoke();
        yield return null;
    }

    private void UpdateSkyTime()
    {
        if (skyController != null)
        {
            skyController.Time = currentTime;
        }
    }

    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60f);
        return $"{hours:D2}:{minutes:D2}";
    }
}