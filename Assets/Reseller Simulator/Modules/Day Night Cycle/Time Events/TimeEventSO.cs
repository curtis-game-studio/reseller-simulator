using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Time Event", menuName = "Time/Time Event")]
public class TimeEventSO : ScriptableObject
{
    public string eventName;

    [Header("Timing")]
    [Range(0, 24)] public int startHour;
    public bool repeatDaily = true;

    public bool useEndTime = false;
    [Range(0, 24)] public int endHour;

    [Header("Specific Day Settings")]
    public bool triggerOnSpecificDay = false;
    public int specificDayNumber = 1;

    [Header("Target Scene Object ID")]
    public string targetObjectID; // Must match SceneObjectID.objectID

    [Header("Methods to Call on Target")]
    public string startMethodName = "OnStartEvent";
    public string endMethodName = "OnEndEvent";

    [NonSerialized] public bool startTriggeredToday = false;
    [NonSerialized] public bool endTriggeredToday = false;

    public void ResetForNewDay()
    {
        startTriggeredToday = false;
        endTriggeredToday = false;
    }

    public void TryTrigger(int currentHour, int currentDay)
    {
        // Specific day check
        if (triggerOnSpecificDay && currentDay != specificDayNumber) return;

        // START EVENT
        if (!startTriggeredToday &&
            currentHour == startHour &&
            (repeatDaily || !startTriggeredToday))
        {
            CallMethodOnTarget(startMethodName);
            startTriggeredToday = true;
        }

        // END EVENT
        if (useEndTime &&
            !endTriggeredToday &&
            currentHour == endHour &&
            (repeatDaily || !endTriggeredToday))
        {
            CallMethodOnTarget(endMethodName);
            endTriggeredToday = true;
        }
    }

    private void CallMethodOnTarget(string methodName)
    {
        if (string.IsNullOrEmpty(methodName)) return;

        GameObject target = null;
        if (!string.IsNullOrEmpty(targetObjectID))
        {
            target = SceneRegistry.Instance.Get(targetObjectID);
            if (target == null)
            {
                Debug.LogWarning($"[TimeEventSO] No scene object found with ID: {targetObjectID}");
                return;
            }
        }

        if (target != null)
        {
            foreach (var script in target.GetComponents<MonoBehaviour>())
            {
                var method = script.GetType().GetMethod(
                    methodName,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic
                );

                if (method != null)
                {
                    try
                    {
                        method.Invoke(script, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[TimeEventSO] Failed to invoke method '{methodName}' on '{script.name}': {ex}");
                    }
                }
            }
        }
    }
}
