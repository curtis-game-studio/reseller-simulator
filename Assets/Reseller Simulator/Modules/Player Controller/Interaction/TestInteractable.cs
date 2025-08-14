using UnityEngine;

public class TestInteractable : Interactable
{
    public override void OnFocus()
    {
        //print("LOOKING AT: " + gameObject.name);
    }

    public override void OnInteract()
    {
        print("We are interacting!");
        if (GameManager.Instance.Time.currentTime >= 19)
        {
            print("The time is greater than the threshold");
            GameManager.Instance.dayTransitionController.HandleDayEnd();
        }
    }

    public override void OnLoseFocus()
    {
        //print("STOPPED LOOKING AT: " + gameObject.name);
    }
}