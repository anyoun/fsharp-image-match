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

type PreviewWindow(state:GameState) =
    inherit GameWindow(800, 600, GraphicsMode(ColorFormat(32), 16, 16))
    let textPrinter = new TextPrinter(TextQuality.Low)
    let font = new Font("Consolas", 12.0f)
    let mutable origImageTexture = 0
    let imageWidth = float32 (state.Width)
    let imageHeight = float32 (state.Height)
  
    let drawImageOutline () =
        GL.Color3(Color.Orange)
        GL.Begin(BeginMode.LineLoop)
        GL.Vertex2(0.0f, 0.0f)
        GL.Vertex2(imageWidth, 0.0f)
        GL.Vertex2(imageWidth, imageHeight)
        GL.Vertex2(0.0f, imageHeight)
        GL.End()

    let drawCandidate (cand:CandidateImage) = 
        GL.Translate(0.0f, imageHeight, 0.0f)
        textPrinter.Print(sprintf "Fitness: %.0f" cand.Fitness, font, Color.White)
        GL.Translate(0.0f, -imageHeight, 0.0f)

        drawImageOutline ()
        for rect in cand.Rectangles do
            GL.Color4(rect.Color.R, rect.Color.G, rect.Color.B, rect.Color.A)
            GL.Begin(BeginMode.Quads)
            GL.Vertex2(rect.X, rect.Y)
            GL.Vertex2(rect.X, rect.Y+rect.Height)
            GL.Vertex2(rect.X+rect.Width, rect.Y+rect.Height)
            GL.Vertex2(rect.X+rect.Width, rect.Y)
            GL.End()

    do
        GL.Enable(EnableCap.Texture2D)
        GL.GenTextures(1, &origImageTexture)
        GL.BindTexture(TextureTarget.Texture2D, origImageTexture)

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, state.Width, state.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, state.BitmapData.Scan0)

        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, int TextureMinFilter.Linear)
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, int TextureMagFilter.Linear)

        GL.Enable(EnableCap.Blend)
        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha)

    override this.OnResize(e) = 
        GL.Viewport(0, 0, this.ClientSize.Width, this.ClientSize.Height)
        let perspective = OpenTK.Matrix4.CreateOrthographicOffCenter( 0.0f, float32 this.ClientSize.Width, 0.0f, float32 this.ClientSize.Height, -10.0f, 10.0f)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadMatrix(ref perspective)

    override this.OnUpdateFrame(e) = 
        base.OnUpdateFrame(e)
        //do something?

    override this.OnRenderFrame(e) =
        base.OnRenderFrame(e)

        GL.Clear(ClearBufferMask.ColorBufferBit)

        let bestCand = !state.BestCandidate
        let someCand = !state.SomeCandidate

        GL.PushMatrix()
        GL.Scale(1.0f, -1.0f, 1.0f)
        GL.Translate(0.0f, float32 -this.ClientSize.Height, 0.0f)
        let ips = float(!state.PermutationCount)/(DateTime.Now-state.StartTime).TotalSeconds
        textPrinter.Print(sprintf "Iteration: %i   at %.0f ips" !state.PermutationCount ips, font, Color.White)

        //Draw some candidate
        GL.Translate(2.0f, 20.0f, 0.0f)
        textPrinter.Print("Last", font, Color.White)
        GL.Translate(0.0f, 20.0f, 0.0f)
        drawCandidate someCand

        //Draw Best candidate
        GL.Translate(imageWidth+2.0f, -20.0f, 0.0f)
        textPrinter.Print("Best", font, Color.White)
        GL.Translate(0.0f, 20.0f, 0.0f)
        drawCandidate bestCand

        //Draw error
        GL.Translate(imageWidth+2.0f, -20.0f, 0.0f)
        textPrinter.Print("Error", font, Color.White)
        GL.Translate(0.0f, 20.0f, 0.0f)
        drawImageOutline ()
        GL.Begin(BeginMode.Points)
        let maxError = float (255*255)
        for y = 0 to int imageHeight do
            for x = 0 to int imageWidth do
                let error = float (Fitness.getPixelError state.BitmapData bestCand x y)
                let color = error/maxError
                GL.Color4( color, color, color, 1.0 )
                GL.Vertex2(x, y)
        GL.End()


        //Draw original image
        GL.Translate(imageWidth+2.0f, -20.0f, 0.0f)
        textPrinter.Print("Original", font, Color.White)
        GL.Translate(0.0f, 20.0f, 0.0f)
        drawImageOutline ()
        GL.Color4( Color.White )
        GL.BindTexture(TextureTarget.Texture2D, origImageTexture)
        
        GL.Begin(BeginMode.Quads)

        GL.TexCoord2(0.0f, 0.0f)
        GL.Vertex2(0.0f, 0.0f)
        GL.TexCoord2(0.0f, 1.0f)
        GL.Vertex2(0.0f, imageHeight)
        GL.TexCoord2(1.0f, 1.0f)
        GL.Vertex2(imageWidth, imageHeight)
        GL.TexCoord2(1.0f, 0.0f)
        GL.Vertex2(imageWidth, 0.0f)

        GL.End()

        GL.PopMatrix()

        this.SwapBuffers()