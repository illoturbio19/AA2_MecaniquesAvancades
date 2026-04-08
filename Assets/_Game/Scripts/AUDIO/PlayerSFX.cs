using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioHighPassFilter))]
[RequireComponent(typeof(AudioReverbFilter))]
[RequireComponent(typeof(AudioDistortionFilter))]
public class PlayerSFX : MonoBehaviour, IZoneAffectable
{
    [Header("Clips")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip throwClip;
    [SerializeField] private AudioClip deathClip;

    [Header("Base Volumes")]
    [SerializeField] private float jumpVolume = 0.9f;
    [SerializeField] private float throwVolume = 0.9f;
    [SerializeField] private float deathVolume = 1f;

    [Header("Random Pitch")]
    [SerializeField] private float randomPitchMin = 0.98f;
    [SerializeField] private float randomPitchMax = 1.02f;

    [Header("Zone Pitch Multipliers")]
    [SerializeField] private float normalPitchMultiplier = 1.00f;
    [SerializeField] private float bouncePitchMultiplier = 1.14f;
    [SerializeField] private float lowGravityPitchMultiplier = 0.82f;
    [SerializeField] private float directionalPitchMultiplier = 0.90f;
    [SerializeField] private float smallPitchMultiplier = 1.28f;
    [SerializeField] private float slidePitchMultiplier = 0.95f;

    private AudioSource audioSource;
    private AudioLowPassFilter lowPass;
    private AudioHighPassFilter highPass;
    private AudioReverbFilter reverb;
    private AudioDistortionFilter distortion;

    private readonly Dictionary<ZoneArea2D, ZoneProfileSO> activeZones = new();
    private ZoneProfileSO currentZone;

    private float currentPitchMultiplier = 1f;

    private enum SFXZoneType
    {
        Normal,
        Bounce,
        LowGravity,
        DirectionalGravity,
        Small,
        Slide
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lowPass = GetComponent<AudioLowPassFilter>();
        highPass = GetComponent<AudioHighPassFilter>();
        reverb = GetComponent<AudioReverbFilter>();
        distortion = GetComponent<AudioDistortionFilter>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        ResetEffects();
    }

    public void PlayJump()
    {
        PlayClip(jumpClip, jumpVolume);
    }

    public void PlayThrow()
    {
        PlayClip(throwClip, throwVolume);
    }

    public void PlayDeath()
    {
        PlayClip(deathClip, deathVolume);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || audioSource == null) return;

        float randomPitch = Random.Range(randomPitchMin, randomPitchMax);
        audioSource.pitch = randomPitch * currentPitchMultiplier;
        audioSource.PlayOneShot(clip, volume);
        audioSource.pitch = 1f;
    }

    public void ApplyZone(ZoneProfileSO profile, ZoneArea2D source)
    {
        if (profile == null || source == null) return;
        if (!profile.affectsPlayer) return;

        activeZones[source] = profile;
        ResolveCurrentZone();
        ApplyCurrentZoneEffects();
    }

    public void RemoveZone(ZoneArea2D source)
    {
        if (source == null) return;

        if (activeZones.ContainsKey(source))
            activeZones.Remove(source);

        ResolveCurrentZone();
        ApplyCurrentZoneEffects();
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

    private void ApplyCurrentZoneEffects()
    {
        ResetEffects();

        SFXZoneType zoneType = DetectZoneType(currentZone);

        switch (zoneType)
        {
            case SFXZoneType.Normal:
                ApplyNormalEffects();
                break;
            case SFXZoneType.Bounce:
                ApplyBounceEffects();
                break;
            case SFXZoneType.LowGravity:
                ApplyLowGravityEffects();
                break;
            case SFXZoneType.DirectionalGravity:
                ApplyDirectionalEffects();
                break;
            case SFXZoneType.Small:
                ApplySmallEffects();
                break;
            case SFXZoneType.Slide:
                ApplySlideEffects();
                break;
        }
    }

    private SFXZoneType DetectZoneType(ZoneProfileSO zone)
    {
        if (zone == null)
            return SFXZoneType.Normal;

        if (zone.bounceMultiplier > 1f)
            return SFXZoneType.Bounce;

        if (Mathf.Abs(zone.gravityDirection.x) > 0.5f)
            return SFXZoneType.DirectionalGravity;

        if (zone.gravityMultiplier < 0.95f)
            return SFXZoneType.LowGravity;

        if (zone.scaleMultiplier < 0.95f)
            return SFXZoneType.Small;

        if (zone.groundDecelerationMultiplier < 0.95f)
            return SFXZoneType.Slide;

        return SFXZoneType.Normal;
    }

    private void ResetEffects()
    {
        currentPitchMultiplier = 1f;

        if (lowPass != null)
        {
            lowPass.enabled = false;
            lowPass.cutoffFrequency = 22000f;
            lowPass.lowpassResonanceQ = 1f;
        }

        if (highPass != null)
        {
            highPass.enabled = false;
            highPass.cutoffFrequency = 10f;
            highPass.highpassResonanceQ = 1f;
        }

        if (reverb != null)
        {
            reverb.enabled = false;
            reverb.reverbPreset = AudioReverbPreset.Off;
        }

        if (distortion != null)
        {
            distortion.enabled = false;
            distortion.distortionLevel = 0f;
        }
    }

    private void ApplyNormalEffects()
    {
        currentPitchMultiplier = normalPitchMultiplier;
    }

    private void ApplyBounceEffects()
    {
        currentPitchMultiplier = bouncePitchMultiplier;

        if (highPass != null)
        {
            highPass.enabled = true;
            highPass.cutoffFrequency = 900f;
            highPass.highpassResonanceQ = 1.2f;
        }

        if (reverb != null)
        {
            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Arena;
        }

        if (distortion != null)
        {
            distortion.enabled = true;
            distortion.distortionLevel = 0.12f;
        }
    }

    private void ApplyLowGravityEffects()
    {
        currentPitchMultiplier = lowGravityPitchMultiplier;

        if (lowPass != null)
        {
            lowPass.enabled = true;
            lowPass.cutoffFrequency = 4200f;
            lowPass.lowpassResonanceQ = 1.1f;
        }

        if (reverb != null)
        {
            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Hallway;
        }
    }

    private void ApplyDirectionalEffects()
    {
        currentPitchMultiplier = directionalPitchMultiplier;

        if (highPass != null)
        {
            highPass.enabled = true;
            highPass.cutoffFrequency = 600f;
            highPass.highpassResonanceQ = 1.4f;
        }

        if (reverb != null)
        {
            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Cave;
        }

        if (distortion != null)
        {
            distortion.enabled = true;
            distortion.distortionLevel = 0.22f;
        }
    }

    private void ApplySmallEffects()
    {
        currentPitchMultiplier = smallPitchMultiplier;

        if (highPass != null)
        {
            highPass.enabled = true;
            highPass.cutoffFrequency = 1400f;
            highPass.highpassResonanceQ = 1.3f;
        }

        if (reverb != null)
        {
            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Room;
        }
    }

    private void ApplySlideEffects()
    {
        currentPitchMultiplier = slidePitchMultiplier;

        if (lowPass != null)
        {
            lowPass.enabled = true;
            lowPass.cutoffFrequency = 9000f;
            lowPass.lowpassResonanceQ = 1f;
        }

        if (reverb != null)
        {
            reverb.enabled = true;
            reverb.reverbPreset = AudioReverbPreset.Stoneroom;
        }

        if (distortion != null)
        {
            distortion.enabled = true;
            distortion.distortionLevel = 0.05f;
        }
    }
}