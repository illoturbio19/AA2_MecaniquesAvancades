using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioReverbFilter))]
public class BackgroundMusicManager : MonoBehaviour
{
    public static BackgroundMusicManager Instance;

    [Header("References")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioLowPassFilter lowPass;
    [SerializeField] private AudioReverbFilter reverb;

    [Header("Base")]
    [SerializeField] private AudioClip backgroundClip;
    [SerializeField] private float baseVolume = 0.7f;
    [SerializeField] private float normalPitch = 1f;

    private readonly Dictionary<ZoneArea2D, ZoneProfileSO> activeZones = new();
    private ZoneProfileSO currentZone;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        if (lowPass == null) lowPass = GetComponent<AudioLowPassFilter>();
        if (reverb == null) reverb = GetComponent<AudioReverbFilter>();

        SetupBaseMusic();
        ApplyCurrentZoneEffect();
    }

    private void SetupBaseMusic()
    {
        if (backgroundClip == null) return;

        musicSource.clip = backgroundClip;
        musicSource.loop = true;          // sí, loop activat
        musicSource.playOnAwake = false;  // el script ja fa Play
        musicSource.volume = baseVolume;
        musicSource.pitch = normalPitch;
        musicSource.spatialBlend = 0f;    // 2D

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void ApplyZone(ZoneProfileSO profile, ZoneArea2D source)
    {
        if (profile == null || source == null) return;

        activeZones[source] = profile;
        ResolveCurrentZone();
        ApplyCurrentZoneEffect();
    }

    public void RemoveZone(ZoneArea2D source)
    {
        if (source == null) return;

        if (activeZones.ContainsKey(source))
            activeZones.Remove(source);

        ResolveCurrentZone();
        ApplyCurrentZoneEffect();
    }

    private void ResolveCurrentZone()
    {
        currentZone = null;
        int bestPriority = int.MinValue;

        foreach (var kvp in activeZones)
        {
            if (kvp.Value == null) continue;

            if (kvp.Value.priority > bestPriority)
            {
                bestPriority = kvp.Value.priority;
                currentZone = kvp.Value;
            }
        }
    }

    private void ApplyCurrentZoneEffect()
    {
        ResetEffects();

        if (currentZone == null)
            return;

        // 1. Bounce
        if (currentZone.bounceMultiplier > 1f)
        {
            musicSource.pitch = 1.10f;

            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Arena;
            return;
        }

        // 2. Directional gravity
        if (Mathf.Abs(currentZone.gravityDirection.x) > 0.5f)
        {
            musicSource.pitch = 0.88f;

            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Cave;

            lowPass.enabled = true;
            lowPass.cutoffFrequency = 5000f;
            return;
        }

        // 3. Low gravity
        if (currentZone.gravityMultiplier < 0.95f)
        {
            musicSource.pitch = 0.80f;

            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Hallway;

            lowPass.enabled = true;
            lowPass.cutoffFrequency = 4200f;
            return;
        }

        // 4. Small
        if (currentZone.scaleMultiplier < 0.95f)
        {
            musicSource.pitch = 1.22f;

            lowPass.enabled = true;
            lowPass.cutoffFrequency = 9500f;
            return;
        }

        // 5. Slide
        if (currentZone.groundDecelerationMultiplier < 0.95f)
        {
            musicSource.pitch = 0.94f;

            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Stoneroom;

            lowPass.enabled = true;
            lowPass.cutoffFrequency = 11000f;
            return;
        }

        // 6. Normal
        musicSource.pitch = normalPitch;
    }

    private void ResetEffects()
    {
        if (musicSource != null)
            musicSource.pitch = normalPitch;

        if (lowPass != null)
        {
            lowPass.enabled = false;
            lowPass.cutoffFrequency = 22000f;
        }

        if (reverb != null)
        {
            reverb.enabled = false;
            reverb.reverbPreset = AudioReverbPreset.Off;
        }
    }
}