namespace Game

// Components
[<Struct>]
type Board =
    { Width: int
      Height: int
      Seed: CoinFlip }

[<Struct>]
type Statistics =
    { Alive: int
      Dead: int
      Iterations: int }

// Actions
[<Struct>]
type CreateBoard =
    { Width: int
      Height: int
      Seed: int option }

[<Struct>]
type Update = struct end

[<Struct>]
type UpdateBoard = struct end

[<Struct>]
type RandomizeBoard = struct end

[<Struct>]
type ClearBoard = struct end

[<Struct>]
type InitializeBoard = struct end

[<Struct>]
type UpdateStatistics = struct end

// Events
[<Struct>]
type BoardCleared = struct end

[<Struct>]
type BoardUpdated = { Statistics: Statistics }

module Board =
    open Garnet.Composition

    let getBoard (world: Container) = world.GetOrDefault<Board>()

    let getStatistics (world: Container) = world.GetOrDefault<Statistics>()

// Systems
module BoardSystem =
    open Garnet.Composition

    let createBoard (world: Container) =
        world.On<CreateBoard>
        <| fun ({ Width = w; Height = h; Seed = seed }) ->
            world.Set<Board>(
                { Width = w
                  Height = h
                  Seed = CoinFlip(seed |> Option.map uint) }
            )
            |> ignore

            world.Send<InitializeBoard>(InitializeBoard())

    let randomizeBoard (world: Container) =
        world.On<RandomizeBoard>
        <| fun _ ->
            let board = world.GetOrDefault<Board>()

            world.Query<Position, Status>()
            |> Seq.iter (fun r ->
                let status = &r.Value2

                status <-
                    { status with
                        IsAlive = board.Seed.flip () })

            world.Send<UpdateStatistics>(UpdateStatistics())

    let clearBoard (world: Container) =
        world.On<ClearBoard>
        <| fun _ ->
            world.Query<Eid, Position, Status>()
            |> Seq.iter (fun r -> world.Destroy(r.Value1))

            world.Send<UpdateStatistics>(UpdateStatistics())
            world.Publish<BoardCleared>(BoardCleared())

    let initializeBoard (world: Container) =
        world.On<InitializeBoard>
        <| fun _ ->
            let board = world.GetOrDefault<Board>()
            let w, h = board.Width, board.Height

            Array2D.init w h (fun _ _ -> world.Create().With<Status>({ IsAlive = false }))
            |> Array2D.iteri (fun x y e -> e.Add<Position>({ X = x; Y = y }))
            |> ignore

            world.Send<UpdateStatistics>(UpdateStatistics())

    let updateStatistics (world: Container) =
        world.On<UpdateStatistics>
        <| fun _ ->
            let statistics =
                world
                |> Board.getStatistics
                |> fun statistics ->
                    world.Query<Position, Status>()
                    |> Seq.fold
                        (fun acc r ->
                            if r.Value2.IsAlive then
                                { acc with Alive = acc.Alive + 1 }
                            else
                                { acc with Dead = acc.Dead + 1 })
                        { Alive = 0
                          Dead = 0
                          Iterations = statistics.Iterations + 1 }

            world.Set<Statistics>(statistics)
            world.Publish<BoardUpdated>({ Statistics = statistics })

    let update (world: Container) =
        world.On<Update>
        <| fun _ ->
            world.Start
            <| seq {
                yield world.Wait<UpdateCells> <| UpdateCells()
                yield world.Wait<UpdateStatistics> <| UpdateStatistics()
                yield Wait.All
            }

    let register (world: Container) =
        world.SetFactory<Statistics>(fun _ -> { Alive = 0; Dead = 0; Iterations = 0 })

        Disposable.Create
            [ world |> createBoard
              world |> updateStatistics
              world |> update
              world |> initializeBoard
              world |> clearBoard
              world |> randomizeBoard ]
