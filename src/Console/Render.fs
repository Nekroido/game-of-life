namespace Console

open Spectre.Console

type Colors =
    | Black
    | Gray
    | Green

    member this.ToColor =
        match this with
        | Black -> Color.Black
        | Gray -> Color.Grey11
        | Green -> Color.Green

type Render(width: int, height: int, bgColor: Colors) =
    let canvas = Canvas(width, height)
    let mutable initialized = false

    member _.BeginDraw() =
        if not initialized then
            System.Console.Clear()
            initialized <- true

        System.Console.SetCursorPosition(0, 0)

        for i in 0 .. width - 1 do
            for j in 0 .. height - 1 do
                canvas.SetPixel(i, j, bgColor.ToColor) |> ignore

        true
    
    member _.DrawText(text: string, color: Colors) =
        AnsiConsole.MarkupLine(text, color.ToColor) |> ignore

    member _.Draw(x, y, color: Colors) =
        canvas.SetPixel(x, y, color.ToColor) |> ignore

    member _.EndDraw() = AnsiConsole.Write(canvas)
