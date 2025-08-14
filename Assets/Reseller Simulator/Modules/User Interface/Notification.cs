using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

public class Notification : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Transform buttonsContainer;
    public GameObject buttonPrefab;

    [Header("Layout References")]
    public HorizontalLayoutGroup buttonsLayoutGroup;

    private List<Button> activeButtons = new List<Button>();
    private Action onCloseCallback;

    public void Setup(string title, string message, List<(string label, Action callback)> buttons, Action onClose = null, float autoCloseDelay = 0f)
    {
        titleText.text = title;
        messageText.text = message;

        onCloseCallback = onClose;

        // Clear previous buttons
        foreach (var btn in activeButtons)
            Destroy(btn.gameObject);
        activeButtons.Clear();

        // Adjust layout based on button count
        if (buttons.Count == 1)
        {
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.spacing = 0;
        }
        else if (buttons.Count == 2)
        {
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.spacing = 30;  // Or whatever spacing feels right
        }
        else
        {
            // For 3+ buttons, use default alignment and spacing
            buttonsLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            buttonsLayoutGroup.spacing = 10;
        }

        // Create buttons dynamically
        foreach (var (label, callback) in buttons)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonsContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = label;

            btn.onClick.AddListener(() =>
            {
                callback?.Invoke();
                Close();
            });

            activeButtons.Add(btn);
        }

        // Optional: handle auto-close if you want here
    }

    public void Close()
    {
        onCloseCallback?.Invoke();
        Destroy(gameObject);
    }
}
