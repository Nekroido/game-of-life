namespace Game

open Garnet.Composition

module GameSystem =
    let register (world: Container) =
        Disposable.Create [ world |> CellSystem.register; world |> BoardSystem.register ]
