namespace Game

[<AutoOpen>]
module Extensions =
    open System
    type Random with
        member r.NextBoolean() =
            r.Next(0, 2) = 0
