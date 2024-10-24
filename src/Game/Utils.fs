namespace Game

open FsRandom

type CoinFlip(seed: uint option) =
    let generator = Statistics.uniformDiscrete (0, 1)

    let mutable state =
        seed
        |> Option.map (fun s -> createState xorshift (s, 0u, 521288629u, 88675123u))
        |> Option.defaultWith Utility.createRandomState

    member _.flip() =
        let _, nextState = Random.next generator state
        state <- nextState

        Random.get generator nextState |> ((*) 1) |> System.Convert.ToBoolean
