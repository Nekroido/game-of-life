namespace Console

open Spectre.Console

open Game

type AnsiRenderer(width: int, height: int) =
    let canvas = Canvas(width, height)

    do
        AnsiConsole.Clear()

        for i in 0 .. width - 1 do
            for j in 0 .. height - 1 do
                canvas.SetPixel(i, j, Color.Black) |> ignore

    interface Renderer with
        member _.Width = width
        member _.Height = height

        member _.BeginDraw() =
            System.Console.SetCursorPosition(0, 0)

            true

        member _.EndDraw() = canvas |> AnsiConsole.Write

        member _.DrawCell({ X = x; Y = y }, { IsAlive = isAlive }) =
            canvas.SetPixel(x, y, if isAlive then Color.Green else Color.Grey11) |> ignore

        member _.DrawStatistics
            ({ Alive = alive
               Dead = dead
               Iterations = iteration })
            =
            sprintf
                "[green]Alive: %d[/], [red]Dead: %d[/], [Grey]Iteration: %d[/]"
                alive
                dead
                iteration
            |> AnsiConsole.MarkupLine

type ConsoleRenderer(width: int, height: int) =
    let aliveCell = '●'
    let deadCell = '◌'
    let statisticsOffset = 1

    do System.Console.Clear()

    interface Renderer with
        member _.Width = width
        member _.Height = height

        member _.BeginDraw() =
            System.Console.SetCursorPosition(0, 0)
            true

        member _.EndDraw() = ()

        member _.DrawCell({ X = x; Y = y }, { IsAlive = isAlive }) =
            System.Console.SetCursorPosition(x, y + statisticsOffset)
            printf "%c" (if isAlive then aliveCell else deadCell)

        member _.DrawStatistics
            ({ Alive = alive
               Dead = dead
               Iterations = iteration })
            =
            printfn "Alive: %d, Dead: %d, Iteration: %d" alive dead iteration
