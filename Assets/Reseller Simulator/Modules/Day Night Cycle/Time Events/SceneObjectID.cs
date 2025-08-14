using UnityEngine;

public class SceneObjectID : MonoBehaviour
{
    public string objectID; // Example: "PostOfficeDoor" or "TimeAnnouncerGO"

    void Start()
    {
        if (!string.IsNullOrEmpty(objectID))
        {
            SceneRegistry.Instance.Register(objectID, gameObject);
        }
    }
}
