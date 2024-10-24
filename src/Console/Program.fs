namespace Console

module App =
    open Game

    [<EntryPoint>]
    let main args =
        let width, height =
            args |> Seq.tryItem 0 |> Option.defaultValue "25" |> int,
            args |> Seq.tryItem 1 |> Option.defaultValue "25" |> int

        let renderer: Renderer =
            args |> Seq.exists (fun arg -> arg = "--console")
            |> function
            | true -> ConsoleRenderer(width, height)
            | false -> AnsiRenderer(width, height)

        Game.Run(renderer, true, None)

        0
