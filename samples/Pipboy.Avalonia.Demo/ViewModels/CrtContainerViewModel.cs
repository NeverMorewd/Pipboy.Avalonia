using ReactiveUI;
using System.Reactive;

namespace Pipboy.Avalonia.Demo.ViewModels;

public class CrtContainerViewModel : ReactiveObject
{
    private float _curvature = 0.18f;
    private float _scanlines = 0.35f;
    private float _vignette = 0.72f;
    private float _phosphorGlow = 0.25f;
    private float _flicker = 0.45f;
    private float _glassReflect = 0.38f;
    private float[] _tint = new float[] { 0.298f, 1.0f, 0.569f };

    public float Curvature { get => _curvature; set => this.RaiseAndSetIfChanged(ref _curvature, value); }
    public float Scanlines { get => _scanlines; set => this.RaiseAndSetIfChanged(ref _scanlines, value); }
    public float Vignette { get => _vignette; set => this.RaiseAndSetIfChanged(ref _vignette, value); }
    public float PhosphorGlow { get => _phosphorGlow; set => this.RaiseAndSetIfChanged(ref _phosphorGlow, value); }
    public float Flicker { get => _flicker; set => this.RaiseAndSetIfChanged(ref _flicker, value); }
    public float GlassReflect { get => _glassReflect; set => this.RaiseAndSetIfChanged(ref _glassReflect, value); }
    public float[] Tint { get => _tint; set => this.RaiseAndSetIfChanged(ref _tint, value); }

    public ReactiveCommand<string, Unit> ApplyPresetCommand { get; }

    public CrtContainerViewModel()
    {
        ApplyPresetCommand = ReactiveCommand.Create<string>(ApplyPreset);
    }

    private void ApplyPreset(string name)
    {
        switch (name)
        {
            case "pipboy": SetPreset(0.18f, 0.35f, 0.72f, 0.25f, 0.45f, 0.38f, 0.298f, 1f, 0.569f); break;
            case "amber": SetPreset(0.14f, 0.45f, 0.80f, 0.30f, 0.60f, 0.50f, 1f, 0.702f, 0.278f); break;
            case "white": SetPreset(0.10f, 0.20f, 0.55f, 0.15f, 0.20f, 0.30f, 0.91f, 0.96f, 0.91f); break;
            case "arcade": SetPreset(0.32f, 0.55f, 0.90f, 0.40f, 0.70f, 0.60f, 0.298f, 1f, 0.569f); break;
            case "flat": SetPreset(0.00f, 0.30f, 0.50f, 0.15f, 0.10f, 0.20f, 0.298f, 1f, 0.569f); break;
        }
    }

    private void SetPreset(float curv, float scan, float vign, float glow, float flic, float refl, float r, float g, float b)
    {
        Curvature = curv;
        Scanlines = scan;
        Vignette = vign;
        PhosphorGlow = glow;
        Flicker = flic;
        GlassReflect = refl;
        Tint = new[] { r, g, b };
    }
}
