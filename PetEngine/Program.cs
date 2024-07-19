using System.Diagnostics;
using System.Drawing;

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;

using StbImageSharp;
using Silk.NET.GLFW;


public class PetEng: VertexOperations
{
    private static uint _vao; //Vertex Array Object
    private static uint _vbo; //Vertex Buffer Object
    private static uint _ebo; //Element Buffer Object
    private static IWindow _window;
    private static GL _gl;
    private static uint _program;
    private static uint _texture;
    
    private static float[] vertices = 
    {
        //     aPosition       |   aTexCoords
             0.5f,  0.5f, 0.0f,  1.0f, 1.0f,
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f, 0.0f,  0.0f, 1.0f
    };

    public static void Main()
    {
        WindowOptions options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "My first Silk.NET application!"
        };
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.Run();
    }
    //Rebind array buffer when vertex changed
    private static unsafe void updateVertexBuffer(float[] vertex)
    {
        //Render new vertices position
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        
        fixed (float* buf = vertex)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertex.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        ///////////////////////////////
    }

    public static void KeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        Console.WriteLine(key);
        switch (key)
        {
            case Key.Escape:
                _window.Close();
                break;
            // Render Modes
            case Key.F:
                _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Fill);
                Console.WriteLine("FillMode");
                break;
            case Key.N:
                _gl.PolygonMode(GLEnum.FrontAndBack, GLEnum.Line);
                Console.WriteLine("LineMode");
                break;
            ///////////////
            // Translating vertices
            case Key.Up:
                PetEng.vertices = Move3(PetEng.vertices, 0f, 0.1f, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            case Key.Down:
                PetEng.vertices = Move3(PetEng.vertices, (float)Math.PI, 0.1f, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            case Key.Left:
                PetEng.vertices = Move3(PetEng.vertices, 1.5f *(float)Math.PI, 0.1f, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            case Key.Right:
                PetEng.vertices = Move3(PetEng.vertices, 0.5f * (float)Math.PI, 0.1f, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            /////////////////
            // Rotating vertices
            // Around object center to left
            case Key.Q:
                PetEng.vertices = Rotate3(PetEng.vertices, -0.25f * (float)Math.PI, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            // Around object center to right
            case Key.E:
                PetEng.vertices = Rotate3(PetEng.vertices, 0.25f * (float)Math.PI, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            // Around global center to right
            case Key.R:
                PetEng.vertices = Rotate3(PetEng.vertices, 0.25f * (float)Math.PI, 5, "Z", "global");
                updateVertexBuffer(PetEng.vertices);
                break;
            case Key.S:
                PetEng.vertices = Scale3(PetEng.vertices, 0.9f, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            case Key.L:
                PetEng.vertices = Scale3(PetEng.vertices, 1.1f, 5);
                updateVertexBuffer(PetEng.vertices);
                break;
            default:
                break;    
        }
    }
    public static unsafe void OnLoad()
    {
        Console.WriteLine("Loaded");

        _gl = _window.CreateOpenGL();
        _gl.ClearColor(Color.CornflowerBlue);

        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);

        float[] vertices = 
        {
        //     aPosition       |   aTexCoords
             0.5f,  0.5f, 0.0f,  1.0f, 1.0f,
             0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 
            -0.5f, -0.5f, 0.0f,  0.0f, 0.0f,
            -0.5f,  0.5f, 0.0f,  0.0f, 1.0f
        };
        
        uint[] indices =
        {
            0u, 1u, 3u,
            1u, 2u, 3u
        };

        _vbo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

        fixed (float* buf = vertices)
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint) (vertices.Length * sizeof(float)), buf, BufferUsageARB.StaticDraw);

        _ebo = _gl.GenBuffer();
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);

        fixed (uint* buf = indices)
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint) (indices.Length * sizeof(uint)), buf, BufferUsageARB.StaticDraw);
        
        _texture = _gl.GenTexture();
        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);

        ImageResult image = ImageResult.FromMemory(File.ReadAllBytes("silk.png"), ColorComponents.RedGreenBlueAlpha);

        fixed (byte* ptr = image.Data)
            _gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint) image.Width, 
                (uint) image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);

        //to do if values in fragment shader lesser then 0 or greater than 1 at x
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)TextureWrapMode.Repeat);
        //to do if values in fragment shader lesser then 0 or greater than 1 at y
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)TextureWrapMode.Repeat);
        //to do if texture area is lesser than image
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        //to do if texture area is greater than image
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)TextureMagFilter.Nearest);

        _gl.Enable(EnableCap.Blend);
        _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        // Mipmap generation and usage
        //_gl.GenerateMipmap(TextureTarget.Texture2D);
        //
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        _gl.TexParameterI(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)TextureMagFilter.Nearest);

        const string vertexCode = @"
        #version 330 core
        
        layout (location = 0) in vec3 aPosition; // Vertices Coords

        layout (location = 1) in vec2 aTextureCoord; // Texture Coords

        out vec2 frag_texCoords;
        
        void main()
        {
            gl_Position = vec4(aPosition, 1.0);

            frag_texCoords = aTextureCoord;
        }";

        uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
        _gl.ShaderSource(vertexShader, vertexCode);

        _gl.CompileShader(vertexShader);
        _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vStatus);
        
        if (vStatus != ((int) (GLEnum.True)))
        {
            throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
        }

        const string fragmentCode = @"
        #version 330 core

        in vec2 frag_texCoords;

        out vec4 out_color;

        uniform sampler2D uTexture; 

        void main()
        {
            //out_color = vec4(frag_texCoords.x, frag_texCoords.y, 0.0, 1.0);
            out_color = texture(uTexture, frag_texCoords);
        }";

        uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
        _gl.ShaderSource(fragmentShader, fragmentCode);

        _gl.CompileShader(fragmentShader);

        _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fStatus);
        if (fStatus != (int) GLEnum.True)
            throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));


        _program = _gl.CreateProgram();

        _gl.AttachShader(_program, vertexShader);
        _gl.AttachShader(_program, fragmentShader);
        
        _gl.LinkProgram(_program);

        _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));
        
        _gl.DetachShader(_program, vertexShader);
        _gl.DetachShader(_program, fragmentShader);
        _gl.DeleteShader(vertexShader);
        _gl.DeleteShader(fragmentShader);

        const uint positionLoc =  0;
        _gl.EnableVertexAttribArray(positionLoc);
        _gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*) 0);
        
        const uint texCoordLoc = 1;
        _gl.EnableVertexAttribArray(texCoordLoc);
        _gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, true, 5 * sizeof(float), (void*) (3 * sizeof(float)));

        int location = _gl.GetUniformLocation(_program, "uTexture");
        _gl.Uniform1(location, 0);

        _gl.BindVertexArray(0);
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        IInputContext input = _window.CreateInput();

        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += KeyDown;
        }
    }

    public static void OnUpdate(double deltaTime)
    {
        //Console.WriteLine("Updated");
        _gl.Viewport(0, 0, (uint)_window.GetFullSize().X, (uint)_window.GetFullSize().Y);
    }

    public static unsafe void OnRender(double deltaTime)
    {
        //Console.WriteLine("Rendered");
        _gl.Clear(ClearBufferMask.ColorBufferBit);
        
        _gl.BindVertexArray(_vao);
        _gl.UseProgram(_program);

        _gl.ActiveTexture(TextureUnit.Texture0);
        _gl.BindTexture(TextureTarget.Texture2D, _texture);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*) 0);
    }
}
