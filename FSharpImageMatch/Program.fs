// Learn more about F# at http://fsharp.net

namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open ImageLib

module Program =
    let permutationsPerNewTriangle = 1000
    let maxNumPermutations = 5000
    let alpha = byte (0.3 * 255.0)
    let rand = Random()
    
    let permutationCount = ref 0

    let original = new Bitmap (Bitmap.FromFile("c:\Users\willt\My Dropbox\Genetic\monalisa_small_grey.png"))
    let maxWidth = original.Width
    let maxHeight = original.Height

    let glWindow = new WindowHost(original, permutationCount)
    let origData = original.LockBits()

    let nextColor () = 
        //Color.FromArgb(alpha, rand.Next(255), rand.Next(255), rand.Next(255))
        FastColor(alpha, 255uy, 255uy, 255uy)

    let addRectangle (image : CandidateImage) = 
        let l = rand.Next maxWidth
        let r = rand.Next maxWidth
        let t = rand.Next maxHeight
        let b = rand.Next maxHeight
        let l,r = (Math.Min(l,r),Math.Max(l,r))
        let t,b = (Math.Min(t,b),Math.Max(t,b))
        let rect = ColoredRectangle(l, r-l, t, b-t, nextColor()) in
        CandidateImage(Array.ofList(rect :: List.ofSeq image.Rectangles) )
    
    let permuteImage (image : CandidateImage) =
        let index = rand.Next( image.Rectangles.Count - 1 )
        let oldRect = image.Rectangles.Item(index)
        let newRect = match rand.Next(5) with
                      | 0 -> new ColoredRectangle(rand.Next maxWidth, oldRect.Y, oldRect.Width, oldRect.Height, oldRect.Color)
                      | 1 -> new ColoredRectangle(oldRect.X, rand.Next maxHeight, oldRect.Width, oldRect.Height, oldRect.Color)
                      | 2 -> new ColoredRectangle(oldRect.X, oldRect.Y, rand.Next(maxWidth-oldRect.Width), oldRect.Height, oldRect.Color)
                      | 3 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, rand.Next(maxHeight-oldRect.Height), oldRect.Color)
                      | 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, nextColor())
        let l = System.Collections.Generic.List<ColoredRectangle>(image.Rectangles)
        l.RemoveAt(index)
        l.Add(newRect)
        new CandidateImage(l)

    let getCandidatePixel (image:CandidateImage) x y =
        let mutable c = FastColor.Black
        for h in image.Rectangles do
            if x >= h.X && y >= h.Y && x < h.X+h.Width && y < h.Y+h.Height then
                c <- c.Blend(h.Color)
        c
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
            if permutationCount.Value % permutationsPerNewTriangle = 0 then
                addRectangle source
            else
                permuteImage source
        candidate.Fitness <- calculateFitness origData candidate
        candidate

    let saveCandidate (candidate:CandidateImage) =
        try
            System.IO.File.WriteAllLines( "save.txt", 
                [ for r in candidate.Rectangles -> sprintf "%ix%i %ix%i" r.X r.Y r.Width r.Height ] )
        with
        | _ -> () //Don't care about errors

    let bestCandidate = ref(addRectangle(CandidateImage()))
    bestCandidate.Value.Fitness <- calculateFitness origData !bestCandidate

    let iteration () =
        if !permutationCount % permutationsPerNewTriangle = 0 then
            //Might accidentally stomp a better candidate here due to race condition
            let cand = addRectangle !bestCandidate
            cand.Fitness <- calculateFitness origData cand
            bestCandidate.Value <- cand
            glWindow.UpdatePreview bestCandidate.Value
        else
            let cand = createCandidate !bestCandidate
            cand.Fitness <- calculateFitness origData cand
            if cand.Fitness > (!bestCandidate).Fitness then
                //Might accidentally stomp a better candidate here due to race condition
                bestCandidate.Value <- cand
                glWindow.UpdatePreview bestCandidate.Value
                saveCandidate !bestCandidate
        permutationCount.Value <- !permutationCount + 1

    let benchmark () =
        let benchmarkCount = 1000
        let testImage = CandidateImage() |> addRectangle |> addRectangle |> addRectangle |> addRectangle
        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            calculateFitness origData testImage |> ignore
        printfn "%f calculateFitness per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            for y = 0 to original.Height do
                for x = 0 to original.Width do
                    origData.GetPixel(x,y) |> ignore
        printfn "%f access bitmaps per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            for y = 0 to original.Height do
                for x = 0 to original.Width do
                    getCandidatePixel testImage x y |> ignore
        printfn "%f access candidates per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)
    
    if false then
        benchmark ()
    else
        Async.Parallel [ for i in 0..4 -> async { 
                                                while true do
                                                    iteration()
                                                } ]
        |> Async.Ignore
        |> Async.Start
        |> ignore

        glWindow.Run(10.0, 10.0)
        glWindow.Dispose()