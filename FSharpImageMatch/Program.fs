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
        //FastColor(byte(rand.Next(64,192)), 255uy, 255uy, 255uy)
        FastColor(byte(rand.Next(64,192)), 255uy, 255uy, 255uy)
        //FastColor(alpha, 255uy, 255uy, 255uy)

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
        let mutable annealCount = 2.0
        interface IPermutationStrategy with
            member this.name () = "Move Edge"
            member this.next permCount bestCandidate =
                if float(permCount) > (annealCount*1000.0) then
                    annealCount <- float(Math.Pow(annealCount,1.5))
                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
                else
                    let index = rand.Next( bestCandidate.Rectangles.Length )
                    let oldRect = bestCandidate.Rectangles.[index]
                    let newRect = match rand.Next(4) with
                                  | 0 -> new ColoredRectangle(rand.Next (maxWidth-oldRect.Width), oldRect.Y, oldRect.Width, oldRect.Height, oldRect.Color)
                                  | 1 -> new ColoredRectangle(oldRect.X, rand.Next (maxHeight-oldRect.Height), oldRect.Width, oldRect.Height, oldRect.Color)
                                  | 2 -> new ColoredRectangle(oldRect.X, oldRect.Y, rand.Next(1,maxWidth-oldRect.X), oldRect.Height, oldRect.Color)
                                  | 3 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, rand.Next(1,maxHeight-oldRect.Y), oldRect.Color)
                                  //| 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, nextColor())
                                  | _ -> raise(Exception())
                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
                    l.[index] <- newRect
                    (new CandidateImage(l), false)

    type MoveCornerPermuter(maxWidth : int, maxHeight: int) = 
        let mutable annealCount = 2.0
        interface IPermutationStrategy with
            member this.name () = "Move Corner"
            member this.next permCount bestCandidate =
                if float(permCount) > (annealCount*2000.0) then
                    annealCount <- float(Math.Pow(annealCount,1.1))
                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
                else
                    let index = rand.Next( bestCandidate.Rectangles.Length )
                    let oldRect = bestCandidate.Rectangles.[index]
                    let newRect = match rand.Next(5) with
                                  | 0 | 1 -> 
                                    let x = rand.Next(oldRect.Right)
                                    let y = rand.Next(oldRect.Bottom)
                                    new ColoredRectangle(x, y, oldRect.Right-x, oldRect.Bottom-y, oldRect.Color)
                                  | 2 | 3 -> 
                                    let r = rand.Next(oldRect.Left, maxWidth)
                                    let b = rand.Next(oldRect.Top, maxHeight)
                                    new ColoredRectangle(oldRect.X, oldRect.Y, r-oldRect.Left, b-oldRect.Top, oldRect.Color)
                                  | 4 -> new ColoredRectangle(oldRect.X, oldRect.Y, oldRect.Width, oldRect.Height, nextColor())
                                  | _ -> raise(Exception())
                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
                    l.[index] <- newRect
                    (new CandidateImage(l), false)
    
    type ReplaceSquarePermuter(maxWidth : int, maxHeight: int) = 
        let mutable annealCount = 2.0
        interface IPermutationStrategy with
            member this.name () = "Replace Square"
            member this.next permCount bestCandidate =
                if float(permCount) > (annealCount*2000.0) then
                    annealCount <- float(Math.Pow(annealCount,1.1))
                    (addRandomRectangle bestCandidate maxWidth maxHeight, true)
                else
                    let l = bestCandidate.Rectangles.Clone() :?> ColoredRectangle[]
                    let index = rand.Next( bestCandidate.Rectangles.Length - 1 )
                    l.[index] <- createRandomRectangle maxWidth maxHeight
                    (new CandidateImage(l), false)

    let state = new GameState(Image.FromFile("c:\Users\willt\My Dropbox\Genetic\monalisa_small_grey.png") :?> Bitmap)
    let glWindow = new PreviewWindow(state)

    let permuter = MoveCornerPermuter(state.Width, state.Height) :> IPermutationStrategy

    let saveCandidateFile (candidate:CandidateImage) =
        try
            System.IO.File.WriteAllLines( "save.txt", 
                [ for r in candidate.Rectangles -> sprintf "%ix%i %ix%i %s" r.X r.Y r.Width r.Height (r.Color.ToString()) ] )
        with
        | _ -> () //Don't care about errors

    state.BestCandidate.Value <- addRandomRectangle (CandidateImage()) state.Width state.Height
    state.BestCandidate.Value.Fitness <- Fitness.calculateFitness state.BitmapData !state.BestCandidate

    let iteration () =
        let setBestCandidate c =
            state.BestCandidate.Value <- c
            saveCandidateFile !state.BestCandidate
        let cand, force = permuter.next !state.PermutationCount !state.BestCandidate 
        Interlocked.Increment state.PermutationCount |> ignore
        cand.Fitness <- Fitness.calculateFitness state.BitmapData cand
        state.SomeCandidate.Value <- cand
        if force then
            setBestCandidate cand
        else if cand.Fitness > (!state.BestCandidate).Fitness then
            setBestCandidate cand

    let benchmark () =
        let benchmarkCount = 1000
        let add c = addRandomRectangle c state.Width state.Height
        let testImage = CandidateImage() |> add |> add |> add |> add
        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            Fitness.calculateFitness state.BitmapData testImage |> ignore
        printfn "%f calculateFitness per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            for y = 0 to state.Height do
                for x = 0 to state.Width do
                    state.BitmapData.GetPixel(x,y) |> ignore
        printfn "%f access bitmaps per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            for y = 0 to state.Height do
                for x = 0 to state.Width do
                    Fitness.getCandidatePixel testImage x y |> ignore
        printfn "%f access candidates per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)
    
    if false then
        benchmark ()
    else if false then
        let cand = CandidateImage [| ColoredRectangle(2, 2, state.Width-4, state.Height-4, FastColor(255uy, 255uy, 0uy, 255uy))  |] 
        glWindow.Run(10.0, 10.0)
    else
        let threadProc () =  while true do iteration()
        for i = 0 to 3 do
            let t = new Thread(threadProc)
            t.Priority <- ThreadPriority.Lowest
            t.IsBackground <- true
            t.Start()

        glWindow.RunWindow()
        glWindow.Dispose()