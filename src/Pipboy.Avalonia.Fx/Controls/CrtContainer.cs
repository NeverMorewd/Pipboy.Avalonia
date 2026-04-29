using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Threading;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace PipBoy.Avalonia.Fx.Controls;

public class CrtContainer : OpenGlControlBase
{
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<CrtContainer, object?>(nameof(Content));

    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<Thickness> PaddingProperty =
        AvaloniaProperty.Register<CrtContainer, Thickness>(nameof(Padding));

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
        AvaloniaProperty.Register<CrtContainer, HorizontalAlignment>(nameof(HorizontalContentAlignment), HorizontalAlignment.Stretch);

    public HorizontalAlignment HorizontalContentAlignment
    {
        get => GetValue(HorizontalContentAlignmentProperty);
        set => SetValue(HorizontalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
        AvaloniaProperty.Register<CrtContainer, VerticalAlignment>(nameof(VerticalContentAlignment), VerticalAlignment.Stretch);

    public VerticalAlignment VerticalContentAlignment
    {
        get => GetValue(VerticalContentAlignmentProperty);
        set => SetValue(VerticalContentAlignmentProperty, value);
    }

    public static readonly StyledProperty<float> CurvatureProperty =
        AvaloniaProperty.Register<CrtContainer, float>(nameof(Curvature), 0.2f);

    public float Curvature
    {
        get => GetValue(CurvatureProperty);
        set => SetValue(CurvatureProperty, value);
    }

    public static readonly StyledProperty<float> ScanlinesProperty =
        AvaloniaProperty.Register<CrtContainer, float>(nameof(Scanlines), 0.5f);

    public float Scanlines
    {
        get => GetValue(ScanlinesProperty);
        set => SetValue(ScanlinesProperty, value);
    }

    public static readonly StyledProperty<float> VignetteProperty =
        AvaloniaProperty.Register<CrtContainer, float>(nameof(Vignette), 0.3f);

    public float Vignette
    {
        get => GetValue(VignetteProperty);
        set => SetValue(VignetteProperty, value);
    }

    public static readonly StyledProperty<float> PhosphorGlowProperty =
        AvaloniaProperty.Register<CrtContainer, float>(nameof(PhosphorGlow), 0.1f);

    public float PhosphorGlow
    {
        get => GetValue(PhosphorGlowProperty);
        set => SetValue(PhosphorGlowProperty, value);
    }

    public static readonly StyledProperty<float> FlickerProperty =
        AvaloniaProperty.Register<CrtContainer, float>(nameof(Flicker), 0.05f);

    public float Flicker
    {
        get => GetValue(FlickerProperty);
        set => SetValue(FlickerProperty, value);
    }

    public static readonly StyledProperty<float> GlassReflectProperty =
        AvaloniaProperty.Register<CrtContainer, float>(nameof(GlassReflect), 0.2f);

    public float GlassReflect
    {
        get => GetValue(GlassReflectProperty);
        set => SetValue(GlassReflectProperty, value);
    }

    public static readonly StyledProperty<float[]> TintProperty =
        AvaloniaProperty.Register<CrtContainer, float[]>(nameof(Tint), new[] { 0.5f, 1.0f, 0.5f });

    public float[] Tint
    {
        get => GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    private Point _mousePosition = new Point(0.5, 0.5);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;
        topLevel.PointerMoved += OnGlobalPointerMoved;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;
        topLevel.PointerMoved -= OnGlobalPointerMoved;
    }

    private void OnGlobalPointerMoved(object? sender, PointerEventArgs e)
    {
        var pos = e.GetPosition(this);
        if (Bounds.Width > 0 && Bounds.Height > 0)
        {
            _mousePosition = new Point(
                Math.Clamp(pos.X / Bounds.Width, 0, 1),
                Math.Clamp(pos.Y / Bounds.Height, 0, 1)
            );
        }
    }

    private int _program;
    private int _vbo;
    private int _texture;
    private SKSurface? _surface;
    private SKCanvas? _canvas;
    private DateTime _startTime = DateTime.UtcNow;

    // OpenGL Constants
    private const int GL_VERTEX_SHADER = 0x8B31;
    private const int GL_FRAGMENT_SHADER = 0x8B30;
    private const int GL_ARRAY_BUFFER = 0x8892;
    private const int GL_STATIC_DRAW = 0x88E4;
    private const int GL_FLOAT = 0x1406;
    private const int GL_TEXTURE_2D = 0x0DE1;
    private const int GL_TEXTURE_WRAP_S = 0x2802;
    private const int GL_TEXTURE_WRAP_T = 0x2803;
    private const int GL_TEXTURE_MIN_FILTER = 0x2801;
    private const int GL_TEXTURE_MAG_FILTER = 0x2800;
    private const int GL_CLAMP_TO_EDGE = 0x812F;
    private const int GL_LINEAR = 0x2601;
    private const int GL_RGBA = 0x1908;
    private const int GL_UNSIGNED_BYTE = 0x1401;
    private const int GL_COLOR_BUFFER_BIT = 0x00004000;
    private const int GL_TRIANGLE_STRIP = 0x0005;
    private const int GL_TEXTURE0 = 0x84C0;

    private delegate void glUniform1fDelegate(int location, float v0);
    private delegate void glUniform2fDelegate(int location, float v0, float v1);
    private delegate void glUniform3fDelegate(int location, float v0, float v1, float v2);
    private delegate void glUniform1iDelegate(int location, int v0);

    private glUniform1fDelegate? _glUniform1f;
    private glUniform2fDelegate? _glUniform2f;
    private glUniform3fDelegate? _glUniform3f;
    private glUniform1iDelegate? _glUniform1i;



    private string VertexShaderSource => @"
        attribute vec2 a;
        varying vec2 v;
        void main() {
            vec2 uv = a * 0.5 + 0.5;
            v = vec2(uv.x, 1.0 - uv.y); 
            gl_Position = vec4(a, 0.0, 1.0);
        }";

    // 优化后的 Fragment Shader：增加了边缘平滑过渡逻辑
    private string FragmentShaderSource => @"
        precision highp float;
        varying vec2 v;
        uniform sampler2D tex;
        uniform vec2 res;
        uniform vec2 mouse;
        uniform float curv, scan, vign, glow, flic, refl, time;
        uniform vec3 tint;

        vec2 barrel(vec2 uv, float s) {
            vec2 d = uv - 0.5;
            float r2 = dot(d, d);
            
            // 计算缩放因子以消除黑边
            // 当 s (曲率) 增加时，边缘向内收缩，我们需要通过 scale 将其拉回
            // 我们取四角最远点 (0.5, 0.5) 的畸变程度作为缩放基准
            float max_dist = 0.5;
            float max_r2 = max_dist * max_dist * 2.0; // 四角的 r2 是 0.5
            float scale = 1.0 / (1.0 + s * max_r2);
            
            return 0.5 + d * (1.0 + s * r2) * scale;
        }

        float scanline(float y, float a) {
            float l = sin(y * res.y * 0.5 * 3.14159) * 0.5 + 0.5;
            l = pow(l, 1.5);
            return 1.0 - a * (1.0 - l) * 0.6;
        }

        float vignet(vec2 uv, float s) {
            vec2 d = abs(uv - 0.5) * 2.0;
            float r = sqrt(dot(d, d));
            return 1.0 - s * smoothstep(0.5, 1.0, r);
        }

        float glass(vec2 uv, vec2 m) {
            // 以鼠标位置 m 为中心的反射效果
            // 修正坐标映射：m 是 Avalonia 坐标 (0,0 在左上)，uv 是映射后的坐标。
            // 顶点着色器已经将 v.y 翻转过 (1.0 - uv.y)，所以这里直接使用 m 即可。
            vec2 targetM = vec2(m.x, m.y);
            
            // 加入微小的随时间偏移，模拟呼吸感
            targetM += vec2(sin(time * 0.5) * 0.002, cos(time * 0.5) * 0.002);
            
            // 主高光：模拟玻璃表面的强反射
            vec2 d1 = (uv - targetM) * vec2(1.8, 2.5);
            float r1 = exp(-dot(d1, d1) * 18.0) * 0.7;
            
            // 次级柔和光晕：增加深度感
            vec2 d2 = (uv - targetM) * vec2(0.6, 1.0);
            float r2 = exp(-dot(d2, d2) * 3.5) * 0.2;
            
            // 边缘条状反光：模拟玻璃侧边的折射
            float r3 = exp(-pow(abs(uv.x - targetM.x), 2.0) * 25.0) * exp(-pow(abs(uv.y - 0.5), 2.0) * 0.8) * 0.08;
            
            return clamp(r1 + r2 + r3, 0.0, 1.0);
        }

        void main() {
            vec2 uv = v;
            vec2 d = barrel(uv, curv);
            
            // --- 1. 基础内容处理 ---
            vec2 sampled_d = clamp(d, 0.0, 1.0);
            vec4 col = texture2D(tex, sampled_d);
            
            // 边缘截断处理
            float edgeFactor = smoothstep(0.0, 0.01, d.x) * (1.0 - smoothstep(0.99, 1.0, d.x)) *
                               smoothstep(0.0, 0.01, d.y) * (1.0 - smoothstep(0.99, 1.0, d.y));
            col.rgb *= edgeFactor;

            // --- 2. 应用屏幕特效 ---
            if (glow > 0.001) {
                float sp = glow * 4.0 / res.x;
                col.rgb += (texture2D(tex, clamp(sampled_d + vec2(sp, 0.0), 0.0, 1.0)).rgb + 
                            texture2D(tex, clamp(sampled_d - vec2(sp, 0.0), 0.0, 1.0)).rgb) * glow;
            }

            col.rgb *= tint;
            col.rgb *= scanline(d.y, scan);
            
            // --- 3. 玻璃反射 (光源计算) ---
            float g = glass(uv, mouse) * refl;
            
            // 计算“手电筒”照亮强度
            // g 是反射光强度，我们用它来抵消暗角的影响
            float illumination = g * 1.5; 
            
            // --- 4. 最终合成 ---
            // 计算暗角系数
            float v = vignet(d, vign);
            
            // 关键：将暗角系数与照亮强度结合。即使 v 为 0，只要 illumination 大，内容就能显现。
            float finalVignette = clamp(v + illumination, 0.0, 1.0);
            
            // 应用合并后的可见度到内容
            col.rgb *= finalVignette;
            
            // 应用闪烁和色调
            float fl = 1.0 - flic * 0.05 * sin(time * 43.7) * sin(time * 17.3);
            col.rgb *= fl;
            
            // 叠加反射光斑本身的视觉效果（光晕和高光）
            vec3 reflectionLight = vec3(0.7, 0.85, 1.0) * g * 0.5;
            vec3 reflectionHighlight = vec3(1.0) * pow(g, 2.5) * 0.6;
            
            vec3 finalColor = col.rgb + reflectionLight + reflectionHighlight;
            
            gl_FragColor = vec4(finalColor, 1.0);
        }";

    private unsafe void CompileShader(GlInterface gl, int shader, string source)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(source);
        fixed (byte* ptr = bytes)
        {
            IntPtr p = (IntPtr)ptr;
            int len = bytes.Length;
            gl.ShaderSource(shader, 1, new IntPtr(&p), new IntPtr(&len));
        }
        gl.CompileShader(shader);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        _glUniform1f = Marshal.GetDelegateForFunctionPointer<glUniform1fDelegate>(gl.GetProcAddress("glUniform1f"));
        _glUniform2f = Marshal.GetDelegateForFunctionPointer<glUniform2fDelegate>(gl.GetProcAddress("glUniform2f"));
        _glUniform3f = Marshal.GetDelegateForFunctionPointer<glUniform3fDelegate>(gl.GetProcAddress("glUniform3f"));
        _glUniform1i = Marshal.GetDelegateForFunctionPointer<glUniform1iDelegate>(gl.GetProcAddress("glUniform1i"));

        int vs = gl.CreateShader(GL_VERTEX_SHADER);
        CompileShader(gl, vs, VertexShaderSource);
        int fs = gl.CreateShader(GL_FRAGMENT_SHADER);
        CompileShader(gl, fs, FragmentShaderSource);

        _program = gl.CreateProgram();
        gl.AttachShader(_program, vs);
        gl.AttachShader(_program, fs);
        gl.LinkProgram(_program);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
        float[] vertices = { -1, -1, 1, -1, -1, 1, 1, 1 };
        unsafe { fixed (float* p = vertices) gl.BufferData(GL_ARRAY_BUFFER, (IntPtr)(vertices.Length * 4), (IntPtr)p, GL_STATIC_DRAW); }

        _texture = gl.GenTexture();
        gl.BindTexture(GL_TEXTURE_2D, _texture);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    }

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        UpdateTexture(gl, (int)Bounds.Width, (int)Bounds.Height);
        gl.Viewport(0, 0, (int)Bounds.Width, (int)Bounds.Height);
        gl.ClearColor(0, 0, 0, 1);
        gl.Clear(GL_COLOR_BUFFER_BIT);
        gl.UseProgram(_program);
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);

        unsafe
        {
            var aPosName = System.Text.Encoding.UTF8.GetBytes("a\0");
            fixed (byte* pName = aPosName)
            {
                int aPos = gl.GetAttribLocation(_program, (IntPtr)pName);
                if (aPos != -1)
                {
                    gl.EnableVertexAttribArray(aPos);
                    gl.VertexAttribPointer(aPos, 2, GL_FLOAT, 0, 0, IntPtr.Zero);
                }
            }
        }

        float time = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
        SetUniform(gl, "res", (float)Bounds.Width, (float)Bounds.Height);
        SetUniform(gl, "curv", Curvature);
        SetUniform(gl, "scan", Scanlines);
        SetUniform(gl, "vign", Vignette);
        SetUniform(gl, "glow", PhosphorGlow);
        SetUniform(gl, "flic", Flicker);
        SetUniform(gl, "refl", GlassReflect);
        SetUniform(gl, "mouse", (float)_mousePosition.X, (float)_mousePosition.Y);
        SetUniform(gl, "time", time);
        SetUniform(gl, "tint", Tint[0], Tint[1], Tint[2]);
        gl.ActiveTexture(GL_TEXTURE0);
        gl.BindTexture(GL_TEXTURE_2D, _texture);
        SetUniform(gl, "tex", 0);
        gl.DrawArrays(GL_TRIANGLE_STRIP, 0, 4);

        // 使用更稳健的方式请求重绘
        // 在某些平台上，直接调用 RequestNextFrameRendering 可能会被节流
        // 确保下一帧渲染始终被调度
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Render);
    }

    private void UpdateTexture(GlInterface gl, int w, int h)
    {
        if (_surface == null || _surface.Canvas.DeviceClipBounds.Width != w)
        {
            _surface = SKSurface.Create(new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul));
            _canvas = _surface.Canvas;
        }

        _canvas!.Clear(SKColors.Black);

        if (Content is Control contentControl)
        {
            // 考虑 Padding 计算可用空间
            var padding = Padding;
            var availableWidth = Math.Max(0, w - padding.Left - padding.Right);
            var availableHeight = Math.Max(0, h - padding.Top - padding.Bottom);

            // 测量内容
            contentControl.Measure(new Size(availableWidth, availableHeight));
            var contentSize = contentControl.DesiredSize;

            // 根据对齐方式计算位置
            double x = padding.Left;
            double y = padding.Top;

            if (HorizontalContentAlignment == HorizontalAlignment.Center)
                x += (availableWidth - contentSize.Width) / 2;
            else if (HorizontalContentAlignment == HorizontalAlignment.Right)
                x += availableWidth - contentSize.Width;
            else if (HorizontalContentAlignment == HorizontalAlignment.Stretch)
                contentSize = contentSize.WithWidth(availableWidth);

            if (VerticalContentAlignment == VerticalAlignment.Center)
                y += (availableHeight - contentSize.Height) / 2;
            else if (VerticalContentAlignment == VerticalAlignment.Bottom)
                y += availableHeight - contentSize.Height;
            else if (VerticalContentAlignment == VerticalAlignment.Stretch)
                contentSize = contentSize.WithHeight(availableHeight);

            // 安排内容位置
            contentControl.Arrange(new Rect(x, y, contentSize.Width, contentSize.Height));

            // 渲染内容到位图
            var pixelSize = new PixelSize(w, h);
            var dpi = new Vector(96, 96);
            using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
            bitmap.Render(contentControl);

            // 将位图转换为 SKImage 并绘制到画布
            using var memoryStream = new System.IO.MemoryStream();
            bitmap.Save(memoryStream);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            using var skImage = SkiaSharp.SKImage.FromEncodedData(memoryStream);

            _canvas!.DrawImage(skImage, 0, 0);
        }
        else
        {
            var configWatermarkText = "string.Empty";
            float targetWidth = 300;
            float targetHeight = 300;
            // Fallback or default drawing if no content is set
            _canvas!.Clear(SKColors.Black);
            using SKTypeface typeface = SKTypeface.FromFamilyName("Arial");
            SKFont font = new SKFont(typeface, 64.0f);
            using var textPaint = new SKPaint { Color = SKColors.GreenYellow, IsAntialias = true };
            float textWidth = font.MeasureText(configWatermarkText, textPaint);

            float textTargetWidth = targetWidth / 6f;
            float fontScale = textTargetWidth / textWidth;
            font.Size *= fontScale;
            float rightOffSet = textTargetWidth * 1.1f;
            SKPoint skPoint = new(targetWidth - rightOffSet, targetHeight - font.Size);
            _canvas.DrawText(configWatermarkText, skPoint, font, textPaint);
        }
        gl.BindTexture(GL_TEXTURE_2D, _texture);
        using (var image = _surface.Snapshot())
        {
            var pixmap = image.PeekPixels();
            gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, w, h, 0, GL_RGBA, GL_UNSIGNED_BYTE, pixmap.GetPixels());
        }
    }



    private unsafe void SetUniform(GlInterface gl, string name, params float[] values)
    {
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name + "\0");
        fixed (byte* pName = nameBytes)
        {
            int loc = gl.GetUniformLocation(_program, (IntPtr)pName);
            if (loc == -1) return;
            if (values.Length == 1) _glUniform1f?.Invoke(loc, values[0]);
            else if (values.Length == 2) _glUniform2f?.Invoke(loc, values[0], values[1]);
            else if (values.Length == 3) _glUniform3f?.Invoke(loc, values[0], values[1], values[2]);
        }
    }

    private unsafe void SetUniform(GlInterface gl, string name, int value)
    {
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name + "\0");
        fixed (byte* pName = nameBytes)
        {
            int loc = gl.GetUniformLocation(_program, (IntPtr)pName);
            if (loc != -1) _glUniform1i?.Invoke(loc, value);
        }
    }
}