namespace FSharpImageMatch

open System
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Compatibility
open System.Windows.Forms
open System.Drawing
open System.Drawing.Imaging
open ImageLib

type WindowHost(originalImage:Bitmap, permutationCount:ref<int>) =
    inherit GameWindow(800, 600, GraphicsMode(ColorFormat(32), 16, 16))
    let mutable currentCandidate = new CandidateImage()
    let textPrinter = new TextPrinter(TextQuality.Low)
    let font = new Font("Consolas", 12.0f)
    let mutable origImageTexture = 0
    let imageWidth = float32 originalImage.Width
    let imageHeight = float32 originalImage.Height
    do
        GL.Enable(EnableCap.Texture2D)
        GL.GenTextures(1, &origImageTexture)
        GL.BindTexture(TextureTarget.Texture2D, origImageTexture    )

        let bitmapData = originalImage.LockBits(new Rectangle(0,0,originalImage.Width, originalImage.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0)
        originalImage.UnlockBits(bitmapData)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Linear)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMagFilter.Linear)

        GL.Enable(EnableCap.Blend)
        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)

    override this.OnResize(e) = 
        GL.Viewport(0, 0, this.Width, this.Height)
        let perspective = OpenTK.Matrix4.CreateOrthographicOffCenter( 0.0f, float32 this.Width, 0.0f, float32 this.Height, -10.0f, 10.0f)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadMatrix(ref perspective)

    override this.OnUpdateFrame(e) = 
        base.OnUpdateFrame(e)
        //do something?

    member this.UpdatePreview (candidate:CandidateImage) =
        currentCandidate <- candidate

    override this.OnRenderFrame(e) =
        base.OnRenderFrame(e)

        GL.Clear(ClearBufferMask.ColorBufferBit)

        GL.PushMatrix()
        GL.Scale(1.0f, -1.0f, 1.0f)
        GL.Translate(0.0f, float32 -this.Height, 0.0f)
        textPrinter.Print(sprintf "Iteration %i Fitness: %f" permutationCount.Value currentCandidate.Fitness, font, Color.White)
        GL.PopMatrix()

        GL.PushMatrix()
        GL.Translate(0.0f, imageHeight, 0.0f)
        GL.Scale(1.0f, -1.0f, 1.0f)

        for rect in currentCandidate.Rectangles do
            GL.Color4(rect.Color.A, rect.Color.R, rect.Color.G, rect.Color.B)
            GL.Begin(BeginMode.Quads)
            GL.Vertex2(rect.X, rect.Y)
            GL.Vertex2(rect.X, rect.Y+rect.Height)
            GL.Vertex2(rect.X+rect.Width, rect.Y+rect.Height)
            GL.Vertex2(rect.X+rect.Width, rect.Y)
            GL.End()

        GL.PopMatrix()

        GL.Color3(Color.White)
        GL.BindTexture(TextureTarget.Texture2D, origImageTexture)
        
        GL.Begin(BeginMode.Quads)

        GL.TexCoord2(0.0f, 1.0f)
        GL.Vertex2(imageWidth, 0.0f)
        GL.TexCoord2(1.0f, 1.0f)
        GL.Vertex2(imageWidth+imageWidth, 0.0f)
        GL.TexCoord2(1.0f, 0.0f)
        GL.Vertex2(imageWidth+imageWidth, imageHeight)
        GL.TexCoord2(0.0f, 0.0f)
        GL.Vertex2(imageWidth, imageHeight)

        GL.End();

        this.SwapBuffers()