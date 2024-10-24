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
let seed = 123

world.Run<CreateBoard>(
    { Width = w
      Height = h
      Seed = seed |> System.Random |> Some }
)
//world.Run<RandomizeBoard>(RandomizeBoard())

let makeLWSS (world: Container) =
    world.Send<ReviveCell>({ Position = { X = 10; Y = 1 } })
    world.Send<ReviveCell>({ Position = { X = 10; Y = 1 } })
    world.Send<ReviveCell>({ Position = { X = 10; Y = 2 } })
    world.Send<ReviveCell>({ Position = { X = 10; Y = 3 } })
    world.Send<ReviveCell>({ Position = { X = 11; Y = 0 } })
    world.Send<ReviveCell>({ Position = { X = 11; Y = 3 } })
    world.Send<ReviveCell>({ Position = { X = 12; Y = 3 } })
    world.Send<ReviveCell>({ Position = { X = 13; Y = 3 } })
    world.Send<ReviveCell>({ Position = { X = 14; Y = 0 } })
    world.Send<ReviveCell>({ Position = { X = 14; Y = 2 } })

world |> makeLWSS

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
