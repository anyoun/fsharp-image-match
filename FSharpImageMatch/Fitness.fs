namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open ImageLib

module Fitness =
    let getCandidatePixel (image:CandidateImage) x y =
        let mutable c = FastColor.Black
        for h in image.Rectangles do
            if x >= h.X && y >= h.Y && x < h.X+h.Width && y < h.Y+h.Height then
                c <- h.Color.Blend(c)
        c
    let getCandidatePixelNoBlend (image:CandidateImage) x y =
        let mutable c = FastColor.Black
        for h in image.Rectangles do
            if x >= h.X && y >= h.Y && x < h.X+h.Width && y < h.Y+h.Height then
                c <- h.Color
        c

    let getPixelError (original:DisposableBitmapData) (candidate:CandidateImage) x y =
        let origPixel = original.GetPixel(x,y)
        let candPixel = getCandidatePixel candidate x y
        let rDiff = (float32 origPixel.R)-(float32 candPixel.R)
        let gDiff = (float32 origPixel.G)-(float32 candPixel.G)
        let bDiff = (float32 origPixel.B)-(float32 candPixel.B)
        rDiff*rDiff + gDiff*gDiff + bDiff*bDiff

    let calculateFitnessNoPainting (original:DisposableBitmapData) (candidate:CandidateImage) =
        let mutable error = 0.0f
        for y = 0 to original.Height-1 do
            for x = 0 to original.Width-1 do
                let origPixel = original.GetPixel(x,y)
                let candPixel = getCandidatePixel candidate x y
                let rDiff = (float32 origPixel.R)-(float32 candPixel.R)
                let gDiff = (float32 origPixel.G)-(float32 candPixel.G)
                let bDiff = (float32 origPixel.B)-(float32 candPixel.B)
                error <- error + rDiff*rDiff + gDiff*gDiff + bDiff*bDiff
        1.0f-error

    let drawCandidate (image:CandidateImage) width height =
        let bmp = MemoryBitmap.Create(width, height)
        for i = 0 to image.Rectangles.Length-1 do
            let h = image.Rectangles.[i]
            for y = h.Y to h.Y+h.Height-1 do
                for x = h.X to h.X+h.Width-1 do
                    let oldColor = bmp.GetPixel(x,y)
                    bmp.SetPixel(x, y, h.Color.Blend(oldColor))
        bmp

    let calculateFitnessPainting (original:DisposableBitmapData) (candidate:CandidateImage) =
        use bmp = drawCandidate candidate original.Width original.Height
        let mutable error = 0.0f
        for y = 0 to original.Height-1 do
            for x = 0 to original.Width-1 do
                let origPixel = original.GetPixel(x,y)
                let candPixel = bmp.GetPixel(x,y)
                let rDiff = (float32 origPixel.R)-(float32 candPixel.R)
                let gDiff = (float32 origPixel.G)-(float32 candPixel.G)
                let bDiff = (float32 origPixel.B)-(float32 candPixel.B)
                error <- error + rDiff*rDiff + gDiff*gDiff + bDiff*bDiff
        1.0f-error

    let calculateFitness = calculateFitnessPainting

type GameState(orignalBitmap : Bitmap) =
    let permutationCount = ref 0
    let bestCandidate = ref (new CandidateImage())
    let someCandidate = ref (new CandidateImage())
    let origBitmapData = orignalBitmap.LockBits()
    let startTime = DateTime.Now
    
    member this.StartTime = startTime
    member this.Width with get () = orignalBitmap.Width
    member this.Height with get () = orignalBitmap.Height
    member this.BitmapData with get () = origBitmapData
    member this.PermutationCount with get () = permutationCount
    member this.BestCandidate with get () = bestCandidate
    member this.SomeCandidate with get () = someCandidate