using UnityEngine;

public class eventTest : MonoBehaviour
{
    public ToastNotificationController toast;
    
    public void PostOfficeOpen()
    {
        string timeText = GameManager.Instance.Time.GetFormattedTime();
        UIManager.Instance.toastController.ShowToast($"{timeText} The post office is now OPEN");
    }

    public void PostOfficeClose()
    {
        string timeText = GameManager.Instance.Time.GetFormattedTime();
        UIManager.Instance.toastController.ShowToast($"{timeText} The post office is now CLOSED");
    }
}
