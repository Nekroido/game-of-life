namespace Console

module App =
    open Spectre.Console

    [<EntryPoint>]
    let main args =
        let width, height =
            args |> Seq.tryItem 0 |> Option.defaultValue "25" |> int,
            args |> Seq.tryItem 1 |> Option.defaultValue "25" |> int

        Game.Run(width, height, true)

        0
