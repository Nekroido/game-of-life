#r "nuget: FsRandom"
#r "nuget: Garnet"
#load "Utils.fs"
#load "Cell.fs"
#load "Board.fs"
#load "Game.fs"

// 0 0 0 0 0
//  0 0 0 0 

open Garnet.Composition
open Game

let world = Container()

world |> GameSystem.register

let w, h = 50, 50
let seed = 41582342

world.Run<CreateBoard>(
    { Width = w
      Height = h
      CoordSystem = CoordSystem.Hex
      Seed = None }
)

world.Run<RandomizeBoard>(RandomizeBoard())

let makeLWSS (world: Container) =
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 10; Y = 1 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 10; Y = 1 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 10; Y = 2 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 10; Y = 3 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 11; Y = 0 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 11; Y = 3 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 12; Y = 3 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 13; Y = 3 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 14; Y = 0 } })
    world.Send<ReviveCell>({ Position = Position.PlaneCoords { X = 14; Y = 2 } })

//world |> makeLWSS

let printGrid (world: Container) =
    let statisticsOffset = 1
    let statistics = Board.getStatistics world

    printfn
        "Alive: %d, Dead: %d, Iteration: %d"
        statistics.Alive
        statistics.Dead
        statistics.Iterations

    world.Query<Position, Status>()
    |> Seq.iter (fun cell ->
        match cell.Value1 with
        | PlaneCoords plane ->
            System.Console.SetCursorPosition(plane.X, plane.Y + statisticsOffset)
            printf "%c" (if cell.Value2.IsAlive then 'O' else '-')
        | HexCoords { Q = q; R = r } ->
            let rowOffset = r % 2 <> 0 |> function true -> 1 | false -> 0
            let x = q * 2
            let y = r
            System.Console.SetCursorPosition(x + rowOffset, y + statisticsOffset)
            printf "%c" (if cell.Value2.IsAlive then '●' else '◌')
        | _ -> ())

    printfn ""

System.Console.Clear()

printGrid world

for _ in 0..100 do
    world.Step 1L
    world.Run(Update())

    System.Console.SetCursorPosition(0, 0)
    printGrid world

    System.Threading.Thread.Sleep(50)
