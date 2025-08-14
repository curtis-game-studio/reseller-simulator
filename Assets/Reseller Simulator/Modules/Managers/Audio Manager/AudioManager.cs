using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Database")]
    public List<SoundSO> sounds;
    public List<AmbienceSO> ambiences;

    private Dictionary<string, SoundSO> soundDict;
    private Dictionary<string, AmbienceSO> ambienceDict;

    private List<AudioSource> activeLoopingSources = new List<AudioSource>();
    private Dictionary<string, AudioSource> activeAmbienceLayers = new Dictionary<string, AudioSource>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        soundDict = new Dictionary<string, SoundSO>();
        ambienceDict = new Dictionary<string, AmbienceSO>();

        foreach (var s in sounds) soundDict[s.soundName] = s;
        foreach (var a in ambiences) ambienceDict[a.ambienceName] = a;
    }

    // ---------------- ONE-SHOTS ----------------
    public void PlayOneShot(string soundName, Vector3? position = null)
    {
        if (!soundDict.TryGetValue(soundName, out var sound)) return;
        float finalPitch = sound.useRandomPitch ? Random.Range(sound.pitch - 0.05f, sound.pitch + 0.05f) : sound.pitch;


        AudioSource source = CreateAudioSource(position, sound.spatial);
        source.volume = sound.volume;
        if (sound.useRandomPitch) source.pitch = finalPitch;
        source.PlayOneShot(sound.GetRandomClip());
        Destroy(source.gameObject, sound.GetRandomClip().length + 0.1f);
    }

    // ---------------- LOOPING ----------------
    public AudioSource PlayLooping(string soundName, Vector3? position = null)
    {
        if (!soundDict.TryGetValue(soundName, out var sound)) return null;

        AudioSource source = CreateAudioSource(position, sound.spatial);
        source.clip = sound.GetRandomClip();
        source.volume = sound.volume;
        source.pitch = sound.pitch;
        source.loop = true;
        source.Play();

        activeLoopingSources.Add(source);
        return source;
    }

    public void StopLooping(AudioSource source)
    {
        if (source != null)
        {
            activeLoopingSources.Remove(source);
            Destroy(source.gameObject);
        }
    }

    // ---------------- AMBIENCE ----------------
    public void PlayAmbience(string ambienceName)
    {
        if (!ambienceDict.TryGetValue(ambienceName, out var ambience)) return;

        foreach (var clip in ambience.layers)
        {
            if (clip == null) continue;
            string layerKey = ambienceName + "_" + clip.name;

            if (activeAmbienceLayers.ContainsKey(layerKey)) continue;

            AudioSource source = CreateAudioSource(null, false);
            source.clip = clip;
            source.volume = ambience.volume;
            source.loop = true;
            source.Play();

            activeAmbienceLayers[layerKey] = source;
        }
    }

    public void StopAmbience(string ambienceName)
    {
        foreach (var key in new List<string>(activeAmbienceLayers.Keys))
        {
            if (key.StartsWith(ambienceName + "_"))
            {
                Destroy(activeAmbienceLayers[key].gameObject);
                activeAmbienceLayers.Remove(key);
            }
        }
    }

    // ---------------- HELPERS ----------------
    private AudioSource CreateAudioSource(Vector3? position, bool spatial)
    {
        GameObject go = new GameObject("Audio_" + Random.Range(0, 1000));
        if (position.HasValue) go.transform.position = position.Value;
        go.transform.parent = transform;

        AudioSource source = go.AddComponent<AudioSource>();
        source.spatialBlend = spatial ? 1f : 0f; // 1 = 3D, 0 = 2D
        return source;
    }
}
