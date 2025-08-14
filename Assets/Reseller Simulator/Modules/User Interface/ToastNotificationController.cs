using System.Collections;
using UnityEngine;
using TMPro;

public class ToastNotificationController : MonoBehaviour
{
    public GameObject toastPrefab;
    public Transform toastParent;
    public float defaultDuration = 3.0f;

    public void ShowToast(string message, float duration = -1f)
    {
        if (duration <= 0) duration = defaultDuration;

        GameObject toastGO = Instantiate(toastPrefab, toastParent);
        TextMeshProUGUI text = toastGO.GetComponentInChildren<TextMeshProUGUI>();
        text.text = message;

        StartCoroutine(AutoClose(toastGO, duration));
    }

    private IEnumerator AutoClose(GameObject toast, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(toast);
    }
}