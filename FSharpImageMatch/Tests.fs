namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open System.Text
open System.Text.RegularExpressions
open ImageLib
open System.Diagnostics
open Microsoft.VisualStudio.TestTools.UnitTesting

[<TestClass>]
type public PerformanceTests () =
    let candidateString = "2481128
52x12 10x66 A:64 R:255 G:255 B:255
149x0 43x142 A:70 R:255 G:255 B:255
0x0 66x55 A:88 R:255 G:255 B:255
85x119 34x42 A:96 R:255 G:255 B:255
136x24 56x54 A:64 R:255 G:255 B:255
120x0 29x36 A:64 R:255 G:255 B:255
5x25 47x68 A:64 R:255 G:255 B:255
166x124 3x8 A:103 R:255 G:255 B:255
64x143 14x11 A:88 R:255 G:255 B:255
98x112 23x26 A:65 R:255 G:255 B:255
66x145 22x12 A:74 R:255 G:255 B:255
151x30 33x26 A:65 R:255 G:255 B:255
73x66 39x28 A:71 R:255 G:255 B:255
163x131 18x3 A:64 R:255 G:255 B:255
44x51 16x18 A:75 R:255 G:255 B:255
14x108 18x81 A:64 R:255 G:255 B:255
40x223 39x13 A:64 R:255 G:255 B:255
51x227 51x37 A:64 R:255 G:255 B:255
70x235 29x10 A:64 R:255 G:255 B:255
65x0 127x24 A:120 R:255 G:255 B:255
38x51 21x20 A:65 R:255 G:255 B:255
72x123 63x33 A:64 R:255 G:255 B:255
132x37 17x10 A:111 R:255 G:255 B:255
0x37 5x40 A:64 R:255 G:255 B:255
134x24 58x38 A:72 R:255 G:255 B:255
167x59 22x14 A:74 R:255 G:255 B:255
167x26 10x22 A:66 R:255 G:255 B:255
32x134 12x23 A:64 R:255 G:255 B:255
109x108 7x11 A:64 R:255 G:255 B:255
159x45 33x36 A:64 R:255 G:255 B:255
79x36 24x11 A:79 R:255 G:255 B:255
182x168 0x58 A:68 R:255 G:255 B:255
32x95 29x26 A:64 R:255 G:255 B:255
95x34 3x53 A:65 R:255 G:255 B:255
141x7 0x132 A:129 R:255 G:255 B:255
44x140 74x0 A:190 R:255 G:255 B:255
16x43 48x18 A:64 R:255 G:255 B:255
166x48 18x44 A:65 R:255 G:255 B:255
76x92 25x7 A:69 R:255 G:255 B:255
87x47 27x32 A:83 R:255 G:255 B:255
54x224 14x4 A:75 R:255 G:255 B:255
80x35 7x36 A:76 R:255 G:255 B:255
169x113 23x47 A:64 R:255 G:255 B:255
74x40 34x19 A:84 R:255 G:255 B:255
133x44 54x8 A:80 R:255 G:255 B:255
88x16 25x5 A:66 R:255 G:255 B:255
70x24 9x6 A:107 R:255 G:255 B:255
124x10 68x34 A:64 R:255 G:255 B:255
69x49 3x30 A:65 R:255 G:255 B:255
113x16 21x14 A:76 R:255 G:255 B:255
88x81 18x81 A:64 R:255 G:255 B:255
60x56 0x71 A:79 R:255 G:255 B:255
27x17 25x8 A:64 R:255 G:255 B:255
58x25 10x23 A:99 R:255 G:255 B:255
93x69 14x15 A:90 R:255 G:255 B:255
90x13 30x3 A:64 R:255 G:255 B:255
134x141 17x7 A:100 R:255 G:255 B:255
99x241 42x8 A:64 R:255 G:255 B:255
70x15 16x80 A:64 R:255 G:255 B:255
81x95 11x8 A:114 R:255 G:255 B:255
143x169 9x19 A:64 R:255 G:255 B:255
144x63 5x53 A:64 R:255 G:255 B:255
76x127 12x18 A:127 R:255 G:255 B:255
86x29 9x31 A:69 R:255 G:255 B:255
66x32 6x6 A:86 R:255 G:255 B:255
7x13 142x0 A:122 R:255 G:255 B:255
1x248 0x3 A:123 R:255 G:255 B:255
72x267 0x11 A:129 R:255 G:255 B:255
68x23 2x18 A:132 R:255 G:255 B:255
73x37 27x16 A:66 R:255 G:255 B:255
72x70 15x12 A:114 R:255 G:255 B:255
65x181 31x0 A:176 R:255 G:255 B:255
73x136 41x24 A:79 R:255 G:255 B:255
128x29 8x11 A:110 R:255 G:255 B:255
169x50 16x19 A:73 R:255 G:255 B:255
7x178 7x11 A:64 R:255 G:255 B:255
72x42 18x21 A:82 R:255 G:255 B:255
67x135 44x34 A:64 R:255 G:255 B:255
"

    [<TestMethod>]
    member this.benchmark () =
        let state = new GameState(Image.FromFile("monalisa_small_grey.png") :?> Bitmap)

        let saveCount,savedRects = Persistence.readCandidateString candidateString
        let testImage = CandidateImage(savedRects)

        let benchmarkCount = 100
        //let add c = PermuteHelper.addRandomRectangle c state.Width state.Height
        //let testImage = CandidateImage() |> add |> add |> add |> add
        
        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            Fitness.calculateFitnessNoPainting state.BitmapData testImage |> ignore
        printfn "%f calculateFitness (no painting) per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            Fitness.calculateFitnessPainting state.BitmapData testImage |> ignore
        printfn "%f calculateFitness (painting) per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let permuter = MoveEdgePermuter(state.Width, state.Height) :> IPermutationStrategy
        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            permuter.next saveCount testImage |> ignore
        printfn "%f permutations per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

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

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            for y = 0 to state.Height do
                for x = 0 to state.Width do
                    Fitness.getCandidatePixelNoBlend testImage x y |> ignore
        printfn "%f access candidates (no blend) per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)

        let sw = System.Diagnostics.Stopwatch.StartNew()
        for i = 0 to benchmarkCount do
            for y = 0 to state.Height do
                for x = 0 to state.Width do
                    testImage.GetPixel(x,y) |> ignore
        printfn "%f access candidates (C#) per second" (float benchmarkCount / sw.Elapsed.TotalSeconds)
        
    [<TestMethod>]
    member this.TestFitness () =
        let state = new GameState(Image.FromFile("monalisa_small_grey.png") :?> Bitmap)

        let saveCount,savedRects = Persistence.readCandidateString candidateString
        let testImage = CandidateImage(savedRects)

        let originalFitness = Fitness.calculateFitness state.BitmapData testImage
        let paintingFitness = Fitness.calculateFitnessPainting state.BitmapData testImage

        Assert.AreEqual(originalFitness, paintingFitness)