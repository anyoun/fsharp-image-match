namespace FSharpImageMatch

open System
open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open System.Windows.Forms
open System.Drawing
open ImageLib

type WindowHost() =
    inherit GameWindow(800, 600, GraphicsMode(ColorFormat(32), 16, 16))
    let mutable currentCandidate = new CandidateImage()
    override this.OnResize(e) = 
        //GL.Viewport(0, 0, this.Width, this.Height)
        //let aspect = single this.Width / single this.Height in
        //let perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1.0f, 64.0f)
        //GL.MatrixMode(MatrixMode.Projection)
        //GL.LoadMatrix(ref perspective)
        
        //GL.Viewport(0, 0, this.Width, this.Height)
        //GL.MatrixMode(MatrixMode.Projection)
        //GL.LoadIdentity()
        //GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0)

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

        //GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)
        //let lookat = Matrix4.LookAt(0.0f, 5.0f, 5.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f)
        //GL.MatrixMode(MatrixMode.Modelview)

        GL.Clear(ClearBufferMask.ColorBufferBit)

//        GL.Begin(BeginMode.Triangles)
//
//        GL.Color3(Color.MidnightBlue)
//        GL.Vertex2(100.0f, 100.0f)
//        GL.Color3(Color.SpringGreen)
//        GL.Vertex2(200.0f, 100.0f)
//        GL.Color3(Color.Ivory)
//        GL.Vertex2(100.0f, 200.0f)
//
//        GL.End()

        for rect in currentCandidate.Rectangles do
            GL.Color3(rect.Color)
            GL.Begin(BeginMode.Quads)
            GL.Vertex2(rect.X, rect.Y)
            GL.Vertex2(rect.X, rect.Y+rect.Height)
            GL.Vertex2(rect.X+rect.Width, rect.Y+rect.Height)
            GL.Vertex2(rect.X+rect.Width, rect.Y)
            GL.End()

        this.SwapBuffers()