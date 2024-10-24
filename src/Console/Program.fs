namespace Console

module App =
    open Game

    [<EntryPoint>]
    let main args =
        let width, height =
            args |> Seq.tryItem 0 |> Option.defaultValue "25" |> int,
            args |> Seq.tryItem 1 |> Option.defaultValue "25" |> int

        let renderer = AnsiRenderer(width, height)
        //let renderer = ConsoleRenderer(width, height)

        Game.Run(renderer, true, None)

        0
