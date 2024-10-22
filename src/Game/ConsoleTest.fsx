#r "nuget: Garnet"
#load "Utils.fs"
#load "Cell.fs"
#load "Board.fs"
#load "Game.fs"

open Garnet.Composition
open Game

let world = Container()

world |> GameSystem.register

let w, h = 30, 30

world.Run<CreateBoard>({ Width = w; Height = h })
world.Run<RandomizeBoard>(RandomizeBoard())

let printGrid (world: Container) =
    let statistics = Board.getStatistics world
    printfn "Alive: %d, Dead: %d" statistics.Alive statistics.Dead

    for y in 0 .. h - 1 do
        for x in 0 .. w - 1 do
            let cell =
                world.Query<Position, Status>()
                |> Seq.find (fun r -> r.Value1.X = x && r.Value1.Y = y)

            printf "%c" (if cell.Value2.IsAlive then 'O' else '-')

        printfn ""

System.Console.Clear()

printGrid world

for _ in 0..100 do
    world.Step 1L
    world.Run(Update())

    System.Console.SetCursorPosition(0, 0)
    printGrid world

    System.Threading.Thread.Sleep(50)
