namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open System.Text
open System.Text.RegularExpressions
open ImageLib

module PermuteHelper =
    let alpha = byte (0.3 * 255.0)
    let rand = Random()

    let nextColor () = 
        //FastColor(byte(rand.Next(64,192)), 255uy, 255uy, 255uy)
        FastColor(byte(rand.Next(64,192)), byte(rand.Next(0,255)), byte(rand.Next(0,255)), byte(rand.Next(0,255)))

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
    
//    type NullPermuter = 
//        interface IPermutationStrategy with
//            member this.name () = "Null Permuter"
//            member this.next permCount bestCandidate = (bestCandidate, false)
    
type MoveEdgePermuter(maxWidth : int, maxHeight: int) = 
    interface IPermutationStrategy with
        member this.name () = "Move Edge"
        member this.next permCount bestCandidate =
            if permCount / 2000 > bestCandidate.Rectangles.Length then
                (PermuteHelper.addRandomRectangle bestCandidate maxWidth maxHeight, true)
            else
                let index = PermuteHelper.rand.Next( bestCandidate.Rectangles.Length )
                let oldRect = bestCandidate.Rectangles.[index]
                let newRect = match PermuteHelper.rand.Next(5) with
                                | 0 -> new ColoredRectangle(PermuteHelper.rand.Next (maxWidth-oldRect.Width), oldRect.Y, oldRect.Width, oldRect.Height, oldRect.Color)
                                | 1 -> new ColoredRectangle(oldRect.X, PermuteHelper.rand.Next (maxHeight-oldRect.Height), oldRect.Width, oldRect.Height, oldRect.Color)
                                | 2 -> new ColoredRectangle(oldRect.X, oldRect.Y, PermuteHelper.rand.Next(1,maxWidth-oldRect.X), oldRect.Height, oldRect.Color)
                                | 3 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, PermuteHelper.rand.Next(1,maxHeight-oldRect.Y), oldRect.Color)
                                | 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, PermuteHelper.nextColor())
                                | _ -> raise(Exception())
                let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
                l.[index] <- newRect
                (new CandidateImage(l), false)

//    type MoveCornerPermuter(maxWidth : int, maxHeight: int) = 
//        let mutable annealCount = 2.0
//        interface IPermutationStrategy with
//            member this.name () = "Move Corner"
//            member this.next permCount bestCandidate =
//                if float(permCount) > (annealCount*2000.0) then
//                    annealCount <- float(Math.Pow(annealCount,1.1))
//                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
//                else
//                    let index = rand.Next( bestCandidate.Rectangles.Length )
//                    let oldRect = bestCandidate.Rectangles.[index]
//                    let newRect = match rand.Next(5) with
//                                  | 0 | 1 -> 
//                                    let x = rand.Next(oldRect.Right)
//                                    let y = rand.Next(oldRect.Bottom)
//                                    new ColoredRectangle(x, y, oldRect.Right-x, oldRect.Bottom-y, oldRect.Color)
//                                  | 2 | 3 -> 
//                                    let r = rand.Next(oldRect.Left, maxWidth)
//                                    let b = rand.Next(oldRect.Top, maxHeight)
//                                    new ColoredRectangle(oldRect.X, oldRect.Y, r-oldRect.Left, b-oldRect.Top, oldRect.Color)
//                                  | 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, nextColor())
//                                  | _ -> raise(Exception())
//                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
//                    l.[index] <- newRect
//                    (new CandidateImage(l), false)
//    
//    type ReplaceSquarePermuter(maxWidth : int, maxHeight: int) = 
//        let mutable annealCount = 2.0
//        interface IPermutationStrategy with
//            member this.name () = "Replace Square"
//            member this.next permCount bestCandidate =
//                if float(permCount) > (annealCount*2000.0) then
//                    annealCount <- float(Math.Pow(annealCount,1.1))
//                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
//                else
//                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
//                    let index = rand.Next( bestCandidate.Rectangles.Length - 1 )
//                    l.[index] <- createRandomRectangle maxWidth maxHeight
//                    (new CandidateImage(l), false)
