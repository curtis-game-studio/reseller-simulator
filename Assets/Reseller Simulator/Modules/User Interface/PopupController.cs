using System;
using System.Collections.Generic;
using UnityEngine;

public class PopupController : MonoBehaviour
{
    public GameObject notificationPrefab;
    public Transform notificationParent;

    // You could extend to accept button labels too, but here's a simple default
    public void ShowNotification(
    string title, 
    string message, 
    Action optionOneAction = null, 
    Action optionTwoAction = null, 
    string optionOneLabel = "OK", 
    string optionTwoLabel = "Cancel")
{
        GameObject popup = Instantiate(notificationPrefab, notificationParent);
        Notification notification = popup.GetComponent<Notification>();

        var buttons = new List<(string label, Action callback)>();

        if (optionOneAction != null)
            buttons.Add((optionOneLabel, optionOneAction));
        if (optionTwoAction != null)
            buttons.Add((optionTwoLabel, optionTwoAction));

        notification.Setup(title, message, buttons);
    }
}
