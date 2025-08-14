using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Ambience")]
public class AmbienceSO : ScriptableObject
{
    public string ambienceName;
    public AudioClip[] layers; // Each clip is a separate layer (rain, birds, wind, etc.)
    [Range(0f, 1f)] public float volume = 1f;
}