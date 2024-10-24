namespace Game

open Garnet.Composition

type Renderer =
    abstract member Width: int
    abstract member Height: int
    abstract member BeginDraw: unit -> bool
    abstract member EndDraw: unit -> unit
    abstract member DrawCell: Position * Status -> unit
    abstract member DrawStatistics: Statistics -> unit

module GameSystem =
    let register (world: Container) =
        Disposable.Create [ world |> CellSystem.register; world |> BoardSystem.register ]

type Game(renderer: Renderer, randomize: bool, seed: int option) =
    let world: Container = Container()
    let cts = new System.Threading.CancellationTokenSource()

    do
        world |> GameSystem.register |> ignore

        world.Run<CreateBoard>(
            { Width = renderer.Width
              Height = renderer.Height
              Seed = seed }
        )

        if randomize then
            world.Run<RandomizeBoard>(RandomizeBoard())

        System.Console.CancelKeyPress.Add(fun _ -> cts.Cancel())

    member _.Run() =

        while not cts.Token.IsCancellationRequested do
            world.Step 1L
            world.Run(Update())

            if renderer.BeginDraw() then
                let statistics = Board.getStatistics world

                renderer.DrawStatistics statistics

                world.Query<Position, Status>()
                |> Seq.iter (fun cell -> renderer.DrawCell(cell.Value1, cell.Value2))

                renderer.EndDraw()

            System.Threading.Thread.Sleep(30)

    interface System.IDisposable with
        member _.Dispose() = world.DestroyAll()

    static member Run(renderer: Renderer, randomize: bool, seed: int option) =
        use game = new Game(renderer, randomize, seed)
        game.Run()
