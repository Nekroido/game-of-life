namespace Game

open Garnet.Composition

[<Struct>]
type PlaneCoords = { X: int; Y: int }

[<Struct>]
type HexCoords = { Q: int; R: int }

// Components
[<Struct>]
type Position =
    | PlaneCoords of plane: PlaneCoords
    | HexCoords of hex: HexCoords

[<Struct>]
type Status = { IsAlive: bool }

// Actions
[<Struct>]
type ReviveCell = { Position: Position }

[<Struct>]
type KillCell = { Position: Position }

// Events
[<Struct>]
type CellRevived = { Position: Position }

[<Struct>]
type CellKilled = { Position: Position }

[<Struct>]
type UpdateCells = struct end

// Helpers
module Cell =
    let planeDirections =
        [| (-1, -1); (0, -1); (1, -1); (-1, 0); (1, 0); (-1, 1); (0, 1); (1, 1) |]

    let hexDirections = [| (-1, 0); (1, 0); (-1, 1); (0, 1); (1, -1); (0, -1) |]

    let getNeighbors (position: Position) (world: Container) =
        let neighbors =
            world.Query<Position, Status>()
            |> Seq.filter (fun r ->
                match position, r.Value1 with
                | PlaneCoords p1, PlaneCoords p2 ->
                    planeDirections
                    |> Array.exists (fun (dx, dy) ->
                        p2.X = p1.X + dx && p2.Y = p1.Y + dy)
                | HexCoords h1, HexCoords h2 ->
                    hexDirections
                    |> Array.exists (fun (dq, dr) ->
                        h2.Q = h1.Q + dq && h2.R = h1.R + dr)
                | _, _ -> false)

        neighbors

    let getAliveNeighbors (position: Position) (world: Container) =
        world |> getNeighbors position |> Seq.filter (fun r -> r.Value2.IsAlive)

// Systems
module CellSystem =
    let killCell (world: Container) =
        world.On<KillCell>
        <| fun e ->
            let cell =
                world.Query<Eid, Position, Status>()
                |> Seq.find (fun r -> r.Value2 = e.Position)

            let status = &cell.Value3
            status <- { status with IsAlive = false }

            world.Publish<CellKilled>({ Position = e.Position })

    let reviveCell (world: Container) =
        world.On<ReviveCell>
        <| fun e ->
            let cell =
                world.Query<Eid, Position, Status>()
                |> Seq.find (fun r -> r.Value2 = e.Position)

            let status = &cell.Value3
            status <- { status with IsAlive = true }

            world.Publish<CellRevived>({ Position = e.Position })

    let updateCells (world: Container) =
        world.On<UpdateCells>
        <| fun _ ->
            let updatedCells =
                world.Query<Eid, Position, Status>()
                |> Seq.map (fun r ->
                    let position = r.Value2
                    let status = &r.Value3

                    let livingNeighbors =
                        world |> Cell.getAliveNeighbors position |> Seq.length

                    let newStatus =
                        status.IsAlive
                        |> function
                            | true -> livingNeighbors = 2 || livingNeighbors = 3
                            | false -> livingNeighbors = 3

                    (position, { status with IsAlive = not newStatus }))
                |> Map.ofSeq

            world.Start
            <| seq {
                for r in updatedCells do
                    if r.Value.IsAlive then
                        yield world.Wait<KillCell> <| { Position = r.Key }
                    else
                        yield world.Wait<ReviveCell> <| { Position = r.Key }

                yield Wait.All
            }

    let register (world: Container) =
        Disposable.Create [ killCell world; reviveCell world; updateCells world ]
