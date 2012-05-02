namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open ImageLib

module Fitness =
    let getCandidatePixel (image:CandidateImage) x y =
        let mutable c = FastColor.Black
        for i = 0 to image.Rectangles.Length-1 do
            let h = image.Rectangles.[i]
            if x >= h.X && y >= h.Y && x < h.X+h.Width && y < h.Y+h.Height then
                c <- h.Color.Blend(c)
        c

    let getPixelError (original:DisposableBitmapData) (candidate:CandidateImage) x y =
        let origPixel = original.GetPixel(x,y)
        let candPixel = getCandidatePixel candidate x y
        let rDiff = (float32 origPixel.R)-(float32 candPixel.R)
        let gDiff = (float32 origPixel.G)-(float32 candPixel.G)
        let bDiff = (float32 origPixel.B)-(float32 candPixel.B)
        rDiff*rDiff + gDiff*gDiff + bDiff*bDiff

    let calculateFitness (original:DisposableBitmapData) (candidate:CandidateImage) =
        let mutable error = 0.0f
        for y = 0 to original.Height do
            for x = 0 to original.Width do
                let origPixel = original.GetPixel(x,y)
                let candPixel = getCandidatePixel candidate x y
                let rDiff = (float32 origPixel.R)-(float32 candPixel.R)
                let gDiff = (float32 origPixel.G)-(float32 candPixel.G)
                let bDiff = (float32 origPixel.B)-(float32 candPixel.B)
                error <- error + rDiff*rDiff + gDiff*gDiff + bDiff*bDiff
        1.0f-error

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