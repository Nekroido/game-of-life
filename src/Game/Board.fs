namespace Game

// Components
[<Struct>]
type Board = { Width: int; Height: int }

[<Struct>]
type Statistics = { Alive: int; Dead: int }

// Actions
[<Struct>]
type CreateBoard = { Width: int; Height: int }

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

    let getStatistics (world: Container) = world.GetOrDefault<Statistics>()

// Systems
module BoardSystem =
    open Garnet.Composition

    let createBoard (world: Container) =
        world.On<CreateBoard>
        <| fun ({ Width = w; Height = h }) ->
            world.Set<Board>({ Width = w; Height = h }) |> ignore

            world.Send<InitializeBoard>(InitializeBoard())

    let randomizeBoard (world: Container) =
        world.On<RandomizeBoard>
        <| fun _ ->
            world.Query<Position, Status>()
            |> Seq.iter (fun r ->
                let status = &r.Value2

                status <-
                    { status with
                        IsAlive = System.Random.Shared.NextBoolean() })

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

            for x in 0 .. w - 1 do
                for y in 0 .. h - 1 do
                    world
                        .Create()
                        .With<Position>({ X = x; Y = y })
                        .With<Status>({ IsAlive = false })
                    |> ignore

            world.Send<UpdateStatistics>(UpdateStatistics())

    let updateStatistics (world: Container) =
        world.On<UpdateStatistics>
        <| fun _ ->
            let statistics =
                world.Query<Position, Status>()
                |> Seq.fold
                    (fun acc r ->
                        if r.Value2.IsAlive then
                            { acc with Alive = acc.Alive + 1 }
                        else
                            { acc with Dead = acc.Dead + 1 })
                    { Alive = 0; Dead = 0 }

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
        world.SetFactory<Statistics>(fun _ -> { Alive = 0; Dead = 0 })

        Disposable.Create
            [ world |> createBoard
              world |> updateStatistics
              world |> update
              world |> initializeBoard
              world |> clearBoard
              world |> randomizeBoard ]
