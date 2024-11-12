namespace Console

module App =
    open Game

    [<EntryPoint>]
    let main args =
        let tryParse (input: string) =
            match System.Int32.TryParse input with
            | true, v -> Some v
            | false, _ -> None

        let width, height, seed =
            args |> Seq.tryItem 0 |> Option.defaultValue "25" |> int,
            args |> Seq.tryItem 1 |> Option.defaultValue "25" |> int,
            args |> Seq.tryItem 2 |> Option.bind tryParse

        let coordSystem =
            args
            |> Seq.exists (fun arg -> arg = "--hex")
            |> function
                | true -> CoordSystem.Hex
                | false -> CoordSystem.Plane

        let renderer: Renderer =
            args
            |> Seq.exists (fun arg -> arg = "--console")
            |> function
                | true -> ConsoleRenderer(width, height)
                | false -> AnsiRenderer(width, height)

        Game.Run(renderer, true, seed, coordSystem)

        0
