using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace Pipboy.Avalonia.Fx.Controls;

public class ProCrtControl : TemplatedControl
{
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<ProCrtControl, object?>(nameof(Content));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<float> CurvatureProperty =
        AvaloniaProperty.Register<ProCrtControl, float>(nameof(Curvature), 0.2f);

    public float Curvature
    {
        get => GetValue(CurvatureProperty);
        set => SetValue(CurvatureProperty, value);
    }

    public static readonly StyledProperty<float> ScanlinesProperty =
        AvaloniaProperty.Register<ProCrtControl, float>(nameof(Scanlines), 0.5f);

    public float Scanlines
    {
        get => GetValue(ScanlinesProperty);
        set => SetValue(ScanlinesProperty, value);
    }

    public static readonly StyledProperty<float> VignetteProperty =
        AvaloniaProperty.Register<ProCrtControl, float>(nameof(Vignette), 0.3f);

    public float Vignette
    {
        get => GetValue(VignetteProperty);
        set => SetValue(VignetteProperty, value);
    }

    public static readonly StyledProperty<float> PhosphorGlowProperty =
        AvaloniaProperty.Register<ProCrtControl, float>(nameof(PhosphorGlow), 0.1f);

    public float PhosphorGlow
    {
        get => GetValue(PhosphorGlowProperty);
        set => SetValue(PhosphorGlowProperty, value);
    }

    public static readonly StyledProperty<float> FlickerProperty =
        AvaloniaProperty.Register<ProCrtControl, float>(nameof(Flicker), 0.05f);

    public float Flicker
    {
        get => GetValue(FlickerProperty);
        set => SetValue(FlickerProperty, value);
    }

    public static readonly StyledProperty<float> GlassReflectProperty =
        AvaloniaProperty.Register<ProCrtControl, float>(nameof(GlassReflect), 0.2f);

    public float GlassReflect
    {
        get => GetValue(GlassReflectProperty);
        set => SetValue(GlassReflectProperty, value);
    }

    public static readonly StyledProperty<float[]> TintProperty =
        AvaloniaProperty.Register<ProCrtControl, float[]>(nameof(Tint), [0.5f, 1.0f, 0.5f]);

    public float[] Tint
    {
        get => GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    static ProCrtControl()
    {
        TemplateProperty.OverrideDefaultValue<ProCrtControl>(new FuncControlTemplate<ProCrtControl>((parent, scope) =>
        {
            var grid = new Grid();

            var contentPresenter = new ContentPresenter
            {
                Name = "PART_ContentPresenter",
                [!ContentPresenter.ContentProperty] = parent[!ContentProperty],
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var effectLayer = new CrtGLEffectLayer
            {
                IsHitTestVisible = false,
                [!CrtGLEffectLayer.CurvatureProperty] = parent[!CurvatureProperty],
                [!CrtGLEffectLayer.ScanlinesProperty] = parent[!ScanlinesProperty],
                [!CrtGLEffectLayer.VignetteProperty] = parent[!VignetteProperty],
                [!CrtGLEffectLayer.PhosphorGlowProperty] = parent[!PhosphorGlowProperty],
                [!CrtGLEffectLayer.FlickerProperty] = parent[!FlickerProperty],
                [!CrtGLEffectLayer.GlassReflectProperty] = parent[!GlassReflectProperty],
                [!CrtGLEffectLayer.TintProperty] = parent[!TintProperty],
                SourceElement = contentPresenter
            };

            grid.Children.Add(contentPresenter);
            grid.Children.Add(effectLayer);

            return grid;
        }));
    }
}


