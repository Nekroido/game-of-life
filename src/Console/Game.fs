namespace Console

open System
open System.Threading
open Garnet.Composition

open Game

type Game(width: int, height: int, randomize: bool) =
    let world: Container = Container()
    let renderer = Render(width, height, Colors.Black)

    let cts = new CancellationTokenSource()

    do
        world |> GameSystem.register |> ignore

        world.Run<CreateBoard>({ Width = width; Height = height })

        if randomize then
            world.Run<RandomizeBoard>(RandomizeBoard())

        Console.CancelKeyPress.Add(fun _ -> cts.Cancel())

    member _.Run() =

        while not cts.Token.IsCancellationRequested do
            world.Step 1L
            world.Run(Update())

            if renderer.BeginDraw() then
                let statistics = Board.getStatistics world

                renderer.DrawText(
                    sprintf "Alive: %d, Dead: %d" statistics.Alive statistics.Dead,
                    Colors.Gray
                )

                for y in 0 .. height - 1 do
                    for x in 0 .. width - 1 do
                        let cell =
                            world.Query<Position, Status>()
                            |> Seq.find (fun r -> r.Value1.X = x && r.Value1.Y = y)

                        renderer.Draw(
                            x,
                            y,
                            cell.Value2.IsAlive
                            |> function
                                | true -> Colors.Green
                                | false -> Colors.Gray
                        )

                renderer.EndDraw()

            Thread.Sleep(30)

    interface IDisposable with
        member _.Dispose() = world.DestroyAll()

    static member Run(width: int, height: int, randomize: bool) =
        use game = new Game(width, height, randomize)

        game.Run()
