// Learn more about F# at http://fsharp.net

namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open ImageLib

module Program =
    let permutationsPerNewTriangle = 5000 in
    let maxNumPermutations = 5000 in
    let alpha = byte (0.3 * 255.0) in
    let rand = Random() in
    
    //let mutable permutationCount = 0 in
    //let mutable bestCandidate = CandidateImage() in

    let original = Bitmap (Bitmap.FromFile("c:\Users\willt\My Dropbox\Genetic\monalisa_small_grey.png"))
    let maxWidth = original.Width
    let maxHeight = original.Height
    let origData = original.LockBits()

    let fitnessComparer = ImageFitnessComparer()
    let calcFitness (orig, candidate) = fitnessComparer.CalculateFitness(orig, candidate)
    let renderer = new ImageRenderer()
    renderer.Width <- original.Width
    renderer.Height <- original.Height

    let addRectangle (image : CandidateImage) = 
        let r = ColoredRectangle(rand.Next maxWidth, rand.Next maxHeight, 100, 100, Color.White) in
        CandidateImage(Array.ofList(r :: List.ofSeq image.Rectangles) )
    
    let permuteImage (image : CandidateImage) =
        let index = rand.Next( image.Rectangles.Count - 1 )
        let oldRect = image.Rectangles.Item(index)
        let newRect = match rand.Next(4) with
                      | 0 -> new ColoredRectangle(rand.Next maxWidth, oldRect.Y, oldRect.Width, oldRect.Height, oldRect.Color)
                      | 1 -> new ColoredRectangle(oldRect.X, rand.Next maxHeight, oldRect.Width, oldRect.Height, oldRect.Color)
                      | 2 -> new ColoredRectangle(oldRect.X, oldRect.Y, rand.Next(maxWidth-oldRect.Width), oldRect.Height, oldRect.Color)
                      | 3 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, rand.Next(maxHeight-oldRect.Height), oldRect.Color)
                      | 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, Color.FromArgb(rand.Next()))
        let l = System.Collections.Generic.List<ColoredRectangle>(image.Rectangles)
        l.RemoveAt(index)
        l.Add(newRect)
        new CandidateImage(l)

    let blendPixelOver (a:Color) (b:Color) =
        Color.FromArgb( int (a.A + b.A*(1uy-a.A)), int (a.A*a.R + b.A*b.R*(1uy-a.A)), int (a.A*a.G + b.A*b.G*(1uy-a.A)), int (a.A*a.B + b.A*b.B*(1uy-a.A)))
    let getCandidatePixel (image:CandidateImage) x y =
        let rec pixelColor (rects:ColoredRectangle list) x y = 
            match rects with
            | h::tail -> 
                if x >= h.X && y >= h.Y && x < h.X+h.Width && y < h.Y+h.Height then
                    blendPixelOver h.Color (pixelColor tail x y)
                else
                    pixelColor tail x y
            | [] -> Color.Black
        pixelColor (List.ofSeq image.Rectangles) x y
    let calculateFitness (original:DisposableBitmapData) (candidate:CandidateImage) =
        let mutable error = 0.0f
        for y = 0 to original.Height do
            for x = 0 to original.Width do
                let origPixel = original.GetPixel(x,y)
                let candPixel = getCandidatePixel candidate x y
                error <- error + float32 (origPixel.R-candPixel.R) * float32 (origPixel.R-candPixel.R)
                error <- error + float32 (origPixel.G-candPixel.G) * float32 (origPixel.G-candPixel.G)
                error <- error + float32 (origPixel.B-candPixel.B) * float32 (origPixel.B-candPixel.B)
        1.0f-error
            

    let createCandidate (source) =
        let candidate = 
            if rand.Next(permutationsPerNewTriangle) = 0 then
                addRectangle source
            else
                permuteImage source
        //renderer.Render candidate 
        //candidate.Fitness <- calcFitness(origData, candidate.RenderedData)
        candidate.Fitness <- calculateFitness origData candidate
        candidate

//    let nextFitness () = 
//        if rand.Next(permutationsPerNewTriangle) = 0 then
//            bestCandidate <- addRectangle bestCandidate
//        else
//            let cand = permuteImage bestCandidate
//            renderer.Render(cand)
//            cand.Fitness <- calcFitness(original, cand.Rendered)
//            if cand.Fitness > bestCandidate.Fitness then
//                bestCandidate <- cand
//                printfn "Found better candidate with fitness %f after iteration %i" bestCandidate.Fitness permutationCount

    //bestCandidate <- addRectangle bestCandidate
    //renderer.Render(bestCandidate)
    //bestCandidate.Fitness <- calcFitness(original, bestCandidate.Rendered)

    //let window = new PreviewWindow()
    let glWindow = new WindowHost()

    let bestCandidate = ref(addRectangle(CandidateImage()))
    //renderer.Render(!bestCandidate)
    //(!bestCandidate).Fitness <- calcFitness(origData, (!bestCandidate).RenderedData)
    (!bestCandidate).Fitness <- calculateFitness origData !bestCandidate

    let iteration () =
        let cand = createCandidate(lock bestCandidate (fun () -> !bestCandidate ))
        let swapped = ref false
        let action = fun () ->
            if cand.Fitness > (!bestCandidate).Fitness then
                bestCandidate.Value <- cand
                swapped.Value <- true
        lock bestCandidate action
        if swapped.Value then
            //window.UpdatePreview(bestCandidate.Value, 1.0, 1)
            glWindow.UpdatePreview bestCandidate.Value
            ()
    
    Async.Parallel [ for i in 0..maxNumPermutations -> async { iteration() } ]
    |> Async.Ignore
    |> Async.Start
    |> ignore
    //bestCandidate.Rendered.Save("best.bmp", Imaging.ImageFormat.Png)

    //Application.Run(window)
    glWindow.Run(30.0, 30.0)
    glWindow.Dispose()