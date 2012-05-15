namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open ImageLib

module Program =

    let state = new GameState(Image.FromFile("monalisa_small_grey.png") :?> Bitmap)
    let glWindow = new PreviewWindow(state)

    let permuter = MoveEdgePermuter(state.Width, state.Height) :> IPermutationStrategy

    let saveCount,savedRects = Persistence.readCandidateFile ()

    if saveCount > 0 then
        state.PermutationCount.Value <- saveCount
        state.BestCandidate.Value <- CandidateImage(savedRects)
        state.BestCandidate.Value.Fitness <- Fitness.calculateFitness state.BitmapData !state.BestCandidate
    else
        //Starting fresh
        state.BestCandidate.Value <- PermuteHelper.addRandomRectangle (CandidateImage()) state.Width state.Height
        state.BestCandidate.Value.Fitness <- Fitness.calculateFitness state.BitmapData !state.BestCandidate

    let iteration () =
        let sequence = state.SequenceNumber
        let cand, force = permuter.next !state.PermutationCount !state.BestCandidate 
        Interlocked.Increment state.PermutationCount |> ignore
        cand.Fitness <- Fitness.calculateFitness state.BitmapData cand
        state.SomeCandidate.Value <- cand

        if force || (cand.Fitness > (!state.BestCandidate).Fitness && state.SequenceNumber=sequence) then
            state.BestCandidate.Value <- cand
            Interlocked.Increment state.SequenceNumber |> ignore
            Persistence.saveCandidateFile !state.PermutationCount !state.BestCandidate
    
    let threadProc () =  while true do iteration()
    for i = 0 to 3 do
        let t = new Thread(threadProc)
        t.Priority <- ThreadPriority.Lowest
        t.IsBackground <- true
        t.Start()

    glWindow.RunWindow()
    glWindow.Dispose()