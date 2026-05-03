using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Platform;
using Avalonia.Threading;
using System.Runtime.InteropServices;

namespace Pipboy.Avalonia.Fx.Controls;

internal class CrtGLEffectLayer : OpenGlControlBase
{
    public static readonly StyledProperty<float> CurvatureProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float>("Curvature");
    public static readonly StyledProperty<float> ScanlinesProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float>("Scanlines");
    public static readonly StyledProperty<float> VignetteProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float>("Vignette");
    public static readonly StyledProperty<float> PhosphorGlowProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float>("PhosphorGlow");
    public static readonly StyledProperty<float> FlickerProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float>("Flicker");
    public static readonly StyledProperty<float> GlassReflectProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float>("GlassReflect");
    public static readonly StyledProperty<float[]> TintProperty = AvaloniaProperty.Register<CrtGLEffectLayer, float[]>("Tint");
    
    private int _program;
    private int _vbo;
    private int _texture;
    private RenderTargetBitmap? _renderBitmap;
    private WriteableBitmap? _writeableBitmap;
    private bool _textureAllocated;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private Point _mousePosition = new(0.5, 0.5);

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
    private delegate void glTexSubImage2DDelegate(int target, int level, int xoffset, int yoffset, int width, int height, int format, int type, IntPtr pixels);

    private glUniform1fDelegate? _glUniform1f;
    private glUniform2fDelegate? _glUniform2f;
    private glUniform3fDelegate? _glUniform3f;
    private glUniform1iDelegate? _glUniform1i;
    private glTexSubImage2DDelegate? _glTexSubImage2D;

    public Visual? SourceElement { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null) topLevel.PointerMoved += OnGlobalPointerMoved;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel != null) topLevel.PointerMoved -= OnGlobalPointerMoved;
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

    protected override void OnOpenGlInit(GlInterface gl)
    {
        _glUniform1f = Marshal.GetDelegateForFunctionPointer<glUniform1fDelegate>(gl.GetProcAddress("glUniform1f"));
        _glUniform2f = Marshal.GetDelegateForFunctionPointer<glUniform2fDelegate>(gl.GetProcAddress("glUniform2f"));
        _glUniform3f = Marshal.GetDelegateForFunctionPointer<glUniform3fDelegate>(gl.GetProcAddress("glUniform3f"));
        _glUniform1i = Marshal.GetDelegateForFunctionPointer<glUniform1iDelegate>(gl.GetProcAddress("glUniform1i"));
        _glTexSubImage2D = Marshal.GetDelegateForFunctionPointer<glTexSubImage2DDelegate>(gl.GetProcAddress("glTexSubImage2D"));

        int vs = gl.CreateShader(GL_VERTEX_SHADER);
        CompileShader(gl, vs, @"
            attribute vec2 a;
            varying vec2 v;
            void main() {
                vec2 uv = a * 0.5 + 0.5;
                v = vec2(uv.x, 1.0 - uv.y); 
                gl_Position = vec4(a, 0.0, 1.0);
            }");

        int fs = gl.CreateShader(GL_FRAGMENT_SHADER);
        CompileShader(gl, fs, @"
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
                float max_dist = 0.5;
                float max_r2 = max_dist * max_dist * 2.0;
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
                vec2 targetM = vec2(m.x, m.y);
                targetM += vec2(sin(time * 0.5) * 0.002, cos(time * 0.5) * 0.002);
                vec2 d1 = (uv - targetM) * vec2(1.8, 2.5);
                float r1 = exp(-dot(d1, d1) * 18.0) * 0.7;
                vec2 d2 = (uv - targetM) * vec2(0.6, 1.0);
                float r2 = exp(-dot(d2, d2) * 3.5) * 0.2;
                float r3 = exp(-pow(abs(uv.x - targetM.x), 2.0) * 25.0) * exp(-pow(abs(uv.y - 0.5), 2.0) * 0.8) * 0.08;
                return clamp(r1 + r2 + r3, 0.0, 1.0);
            }

            void main() {
                vec2 uv = v;
                vec2 d = barrel(uv, curv);
                vec2 sampled_d = clamp(d, 0.0, 1.0);
                vec4 col = texture2D(tex, sampled_d);
                
                float edgeFactor = smoothstep(0.0, 0.01, d.x) * (1.0 - smoothstep(0.99, 1.0, d.x)) *
                                   smoothstep(0.0, 0.01, d.y) * (1.0 - smoothstep(0.99, 1.0, d.y));
                col.rgb *= edgeFactor;

                if (glow > 0.001) {
                    float sp = glow * 4.0 / res.x;
                    col.rgb += (texture2D(tex, clamp(sampled_d + vec2(sp, 0.0), 0.0, 1.0)).rgb + 
                                texture2D(tex, clamp(sampled_d - vec2(sp, 0.0), 0.0, 1.0)).rgb) * glow;
                }

                col.rgb *= tint;
                col.rgb *= scanline(d.y, scan);
                
                float g = glass(uv, mouse) * refl;            
                float illumination = g * 1.5;           
                float v_val = vignet(d, vign);        
                float finalVignette = clamp(v_val + illumination, 0.0, 1.0);
                col.rgb *= finalVignette;
                
                float fl = 1.0 - flic * 0.05 * sin(time * 43.7) * sin(time * 17.3);
                col.rgb *= fl;
                
                vec3 reflectionLight = vec3(0.7, 0.85, 1.0) * g * 0.5;
                vec3 reflectionHighlight = vec3(1.0) * pow(g, 2.5) * 0.6;
                vec3 finalColor = col.rgb + reflectionLight + reflectionHighlight;
                
                gl_FragColor = vec4(finalColor, 1.0);
            }");

        _program = gl.CreateProgram();
        gl.AttachShader(_program, vs);
        gl.AttachShader(_program, fs);
        gl.LinkProgram(_program);

        _vbo = gl.GenBuffer();
        gl.BindBuffer(GL_ARRAY_BUFFER, _vbo);
        float[] vertices = { -1, -1, 1, -1, -1, 1, 1, 1 };
        unsafe { fixed (float* p = vertices) gl.BufferData(GL_ARRAY_BUFFER, vertices.Length * 4, (IntPtr)p, GL_STATIC_DRAW); }

        _texture = gl.GenTexture();
        gl.BindTexture(GL_TEXTURE_2D, _texture);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        gl.TexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    }

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

    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        int w = (int)Bounds.Width;
        int h = (int)Bounds.Height;
        if (w <= 0 || h <= 0) return;

        UpdateTexture(gl, w, h);

        gl.Viewport(0, 0, w, h);
        gl.ClearColor(0, 0, 0, 0);
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
        SetUniform(gl, "res", (float)w, (float)h);
        SetUniform(gl, "curv", GetValue(CurvatureProperty));
        SetUniform(gl, "scan", GetValue(ScanlinesProperty));
        SetUniform(gl, "vign", GetValue(VignetteProperty));
        SetUniform(gl, "glow", GetValue(PhosphorGlowProperty));
        SetUniform(gl, "flic", GetValue(FlickerProperty));
        SetUniform(gl, "refl", GetValue(GlassReflectProperty));
        SetUniform(gl, "mouse", (float)_mousePosition.X, (float)_mousePosition.Y);
        SetUniform(gl, "time", time);
        var tint = GetValue(TintProperty);
        SetUniform(gl, "tint", tint[0], tint[1], tint[2]);

        gl.ActiveTexture(GL_TEXTURE0);
        gl.BindTexture(GL_TEXTURE_2D, _texture);
        SetUniform(gl, "tex", 0);

        gl.DrawArrays(GL_TRIANGLE_STRIP, 0, 4);
        Dispatcher.UIThread.Post(RequestNextFrameRendering, DispatcherPriority.Render);
    }

    private void UpdateTexture(GlInterface gl, int w, int h)
    {
        if (_renderBitmap == null || _renderBitmap.PixelSize.Width != w || _renderBitmap.PixelSize.Height != h)
        {
            _renderBitmap?.Dispose();
            _writeableBitmap?.Dispose();
            _renderBitmap = new RenderTargetBitmap(new PixelSize(w, h), new Vector(96, 96));
            _writeableBitmap = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            _textureAllocated = false;
        }

        if (SourceElement != null)
        {
            _renderBitmap.Render(SourceElement);
            using var fb = _writeableBitmap!.Lock();
            _renderBitmap.CopyPixels(new PixelRect(0, 0, w, h), fb.Address, fb.RowBytes * h, fb.RowBytes);
            gl.BindTexture(GL_TEXTURE_2D, _texture);
            if (!_textureAllocated)
            {
                gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, w, h, 0, GL_RGBA, GL_UNSIGNED_BYTE, fb.Address);
                _textureAllocated = true;
            }
            else
            {
                _glTexSubImage2D?.Invoke(GL_TEXTURE_2D, 0, 0, 0, w, h, GL_RGBA, GL_UNSIGNED_BYTE, fb.Address);
            }
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

    protected override void OnOpenGlDeinit(GlInterface gl)
    {
        gl.DeleteProgram(_program);
        gl.DeleteBuffer(_vbo);
        gl.DeleteTexture(_texture);
        _renderBitmap?.Dispose();
        _renderBitmap = null;
        _writeableBitmap?.Dispose();
        _writeableBitmap = null;
    }
}
