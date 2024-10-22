namespace Console

module App =
    open Spectre.Console

    [<EntryPoint>]
    let main args =
        let width, height =
            args |> Seq.tryItem 0 |> Option.defaultValue "50" |> int,
            args |> Seq.tryItem 1 |> Option.defaultValue "50" |> int

        let game = Game.Run(width, height, true)

        0
