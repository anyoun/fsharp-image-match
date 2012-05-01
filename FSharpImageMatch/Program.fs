// Learn more about F# at http://fsharp.net

namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open ImageLib

module Program =
    let alpha = byte (0.3 * 255.0)
    let rand = Random()

    let nextColor () = 
        //Color.FromArgb(alpha, rand.Next(255), rand.Next(255), rand.Next(255))
        //FastColor(alpha, 255uy, 255uy, 255uy)
        FastColor(byte(rand.Next(64,192)), 255uy, 255uy, 255uy)
    let createRandomRectangle maxWidth maxHeight =
        let l = rand.Next maxWidth
        let r = rand.Next maxWidth
        let t = rand.Next maxHeight
        let b = rand.Next maxHeight
        let l,r = if l<r then (l,r) else (r,l)
        let t,b = if t<b then (t,b) else (b,t)
        ColoredRectangle(l, t, r-l, b-t, nextColor())
    let addRandomRectangle (image : CandidateImage) maxWidth maxHeight = 
        let rect = createRandomRectangle maxWidth maxHeight
        let arr = Array.create (image.Rectangles.Length+1) rect
        image.Rectangles.CopyTo(arr, 0)
        CandidateImage(arr)

    type IPermutationStrategy = interface
        abstract name : unit -> string
        abstract next : int -> CandidateImage -> (CandidateImage * bool)
        end
    
    type NullPermuter = 
        interface IPermutationStrategy with
            member this.name () = "Null Permuter"
            member this.next permCount bestCandidate = (bestCandidate, false)
    
    type MoveEdgePermuter(maxWidth : int, maxHeight: int) = 
        interface IPermutationStrategy with
            member this.name () = "Move Edge"
            member this.next permCount bestCandidate =
                if permCount % 1000 = 0 then
                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
                else
                    let index = rand.Next( bestCandidate.Rectangles.Length - 1 )
                    let oldRect = bestCandidate.Rectangles.[index]
                    let newRect = match rand.Next(4) with
                                  | 0 -> new ColoredRectangle(rand.Next maxWidth, oldRect.Y, oldRect.Width, oldRect.Height, oldRect.Color)
                                  | 1 -> new ColoredRectangle(oldRect.X, rand.Next maxHeight, oldRect.Width, oldRect.Height, oldRect.Color)
                                  | 2 -> new ColoredRectangle(oldRect.X, oldRect.Y, rand.Next(1,maxWidth-oldRect.Width), oldRect.Height, oldRect.Color)
                                  | 3 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, rand.Next(1,maxHeight-oldRect.Height), oldRect.Color)
                                  //| 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, nextColor())
                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
                    l.[index] <- newRect
                    (new CandidateImage(l), false)
    
    type ReplaceSquarePermuter(maxWidth : int, maxHeight: int) = 
        let mutable annealCount = 2.0
        interface IPermutationStrategy with
            member this.name () = "Replace Square"
            member this.next permCount bestCandidate =
                if float(permCount) > (annealCount*1000.0) then
                    annealCount <- float(Math.Pow(annealCount,1.5))
                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
                else
                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
                    let index = rand.Next( bestCandidate.Rectangles.Length - 1 )
                    l.[index] <- createRandomRectangle maxWidth maxHeight
                    (new CandidateImage(l), false)
    
    let permutationCount = ref 0

    let original = new Bitmap (Bitmap.FromFile("c:\Users\willt\My Dropbox\Genetic\monalisa_small_grey.png"))
    let maxWidth = original.Width
    let maxHeight = original.Height

    let glWindow = new WindowHost(original, permutationCount)
    let origData = original.LockBits()

    let getCandidatePixel (image:CandidateImage) x y =
        let mutable c = FastColor.Black
        for i = 0 to image.Rectangles.Length-1 do
            let h = image.Rectangles.[i]
            if x >= h.X && y >= h.Y && x < h.X+h.Width && y < h.Y+h.Height then
                //c <- c.Blend(h.Color)
                c <- h.Color.Blend(c)
        c
    let calculateFitness (original:DisposableBitmapData) (candidate:CandidateImage) =
        let mutable error = 0.0f
        for y = 0 to original.Height do
            for x = 0 to original.Width do
                let origPixel = original.GetPixel(x,y)
                let candPixel = getCandidatePixel candidate x y
                error <- error + float32 ((origPixel.R-candPixel.R) * (origPixel.R-candPixel.R))
                error <- error + float32 ((origPixel.G-candPixel.G) * (origPixel.G-candPixel.G))
                error <- error + float32 ((origPixel.B-candPixel.B) * (origPixel.B-candPixel.B))
        1.0f-error

    let permuter = ReplaceSquarePermuter(maxWidth, maxHeight) :> IPermutationStrategy

    let saveCandidateFile (candidate:CandidateImage) =
        try
            System.IO.File.WriteAllLines( "save.txt", 
                [ for r in candidate.Rectangles -> sprintf "%ix%i %ix%i %s" r.X r.Y r.Width r.Height (r.Color.ToString()) ] )
        with
        | _ -> () //Don't care about errors

    let bestCandidate = ref(addRandomRectangle (CandidateImage()) maxWidth maxHeight)
    bestCandidate.Value.Fitness <- calculateFitness origData !bestCandidate

    let iteration () =
        let setBestCandidate c =
            bestCandidate.Value <- c
            glWindow.UpdatePreview bestCandidate.Value
            saveCandidateFile !bestCandidate
        let cand, force = permuter.next !permutationCount !bestCandidate 
        Interlocked.Increment permutationCount |> ignore
        cand.Fitness <- calculateFitness origData cand
        if force then
            setBestCandidate cand
        else if cand.Fitness > (!bestCandidate).Fitness then
            setBestCandidate cand

    let benchmark () =
        let benchmarkCount = 1000
        let add c = addRandomRectangle c maxWidth maxHeight
        let testImage = CandidateImage() |> add |> add |> add |> add
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
    else if false then
        let cand = CandidateImage [| ColoredRectangle(2, 2, maxWidth-4, maxHeight-4, FastColor(255uy, 255uy, 0uy, 255uy))  |] 
        glWindow.UpdatePreview cand
        glWindow.Run(10.0, 10.0)
    else
        let threadProc () =  while true do iteration()
        for i = 0 to 3 do
            let t = new Thread(threadProc)
            t.Priority <- ThreadPriority.Lowest
            t.Start()

        glWindow.Run(10.0, 10.0)
        glWindow.Dispose()