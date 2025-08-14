using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sound")]
public class SoundSO : ScriptableObject
{
    public string soundName;
    public AudioClip[] clips; 
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;
    public bool spatial = false; 
    public bool useRandomPitch = false;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
