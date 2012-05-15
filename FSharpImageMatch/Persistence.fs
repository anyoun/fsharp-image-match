namespace FSharpImageMatch
open System
open System.Windows.Forms
open System.Drawing
open System.Threading
open System.Text
open System.Text.RegularExpressions
open ImageLib

module Persistence =
    let saveCandidateFile (count:int) (candidate:CandidateImage) =
        let lines = [ for r in candidate.Rectangles -> sprintf "%ix%i %ix%i %s" r.X r.Y r.Width r.Height (r.Color.ToString()) ]
        let lines = (string count) :: lines
        try
            System.IO.File.WriteAllLines( "save.txt", lines )                
        with
        | _ -> () //Don't care about errors

    let readCandidateLines lines =
        let parseLine s = 
            let pattern = @"(\d+)x(\d+) (\d+)x(\d+) A:(\d+) R:(\d+) G:(\d+) B:(\d+)"
            let m = Regex.Match(s, pattern)
            if not m.Success then raise(Exception("Can't parse save file."))
            let captures = m.Groups
            let color = new FastColor(byte captures.[5].Value, byte captures.[6].Value, byte captures.[7].Value, byte captures.[8].Value)
            new ColoredRectangle(int captures.[1].Value, int captures.[2].Value, int captures.[3].Value, int captures.[4].Value, color)
        let firstLine = Seq.head lines
        let perutationCount = int firstLine
        ( perutationCount, Array.ofSeq (Seq.map parseLine (Seq.skip 1 lines)) )

    let readCandidateFile () =
        try
            readCandidateLines (System.IO.File.ReadAllLines("save.txt"))
        with
        | _ -> (0, Array.empty)
    let readCandidateString (s:string) = readCandidateLines (s.Split( [|'\r'; '\n'|], StringSplitOptions.RemoveEmptyEntries))