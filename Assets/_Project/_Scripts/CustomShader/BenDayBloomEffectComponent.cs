using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Ben Day Bloom", typeof(UniversalRenderPipeline))]
public class BenDayBloomEffectComponent : VolumeComponent, IPostProcessComponent
{
    // Bloom Settings
    [Header("Bloom Settings")]
    [Tooltip("Filters out pixels below this brightness level.")]
    public FloatParameter threshold = new FloatParameter(0.9f, true);

    [Tooltip("Strength of the bloom effect.")]
    public FloatParameter intensity = new FloatParameter(1f, true);

    [Tooltip("Controls how much the bloom spreads.")]
    public ClampedFloatParameter scatter = new ClampedFloatParameter(0.7f, 0f, 1f, true);

    [Tooltip("Clamps bright pixels to prevent excessive glow.")]
    public IntParameter clamp = new IntParameter(65472, true);

    [Tooltip("Number of blur iterations (higher = smoother but slower).")]
    public ClampedIntParameter maxIterations = new ClampedIntParameter(6, 1, 10, true);

    [Tooltip("Tint color for the bloom effect.")]
    public NoInterpColorParameter tint = new NoInterpColorParameter(Color.white);

    // Ben-Day Dots Settings
    [Header("Ben-Day Dots")]
    [Tooltip("Density of the dot pattern.")]
    public IntParameter dotsDensity = new IntParameter(10, true);

    [Tooltip("Cutoff threshold for dot visibility.")]
    public ClampedFloatParameter dotsCutoff = new ClampedFloatParameter(0.4f, 0f, 1f, true);

    // Required for IPostProcessComponent
    public bool IsActive() => intensity.value > 0f && maxIterations.value > 0;
    public bool IsTileCompatible() => false; // Disable tile-based rendering for this effect
}