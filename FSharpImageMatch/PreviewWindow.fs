namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open ImageLib


type PreviewWindow() as this =
    inherit Form()

    let imagePreview = new PictureBox()
    let progressLabel = new Label()

    do
        this.Width <- 640
        this.Height <- 480

        imagePreview.Dock <- DockStyle.Fill
        imagePreview.SizeMode <- PictureBoxSizeMode.Zoom
        this.Controls.Add imagePreview
        
        progressLabel.Dock <- DockStyle.Top
        this.Controls.Add progressLabel

    member this.UpdatePreview(bitmap, fitness, interations) =
        if this.InvokeRequired then
            ignore(this.Invoke(Action( fun () -> this.UpdatePreview(bitmap, fitness, interations) )))
        else
            Console.WriteLine("Updating preview...")
            imagePreview.Image <- bitmap
            progressLabel.Text <- sprintf "Found image with fitness %f after iteration %i" fitness interations