﻿[<AutoOpen>]
module TestUtils

open System.Runtime.CompilerServices
open Faqt
open AssertionHelpers
open Xunit


let assertExnMsg (msg: string) (f: unit -> 'a) =
    let ex = Assert.Throws<AssertionFailedException>(f >> ignore)
    Assert.Equal(msg.ReplaceLineEndings("\n").Trim(), ex.Message.ReplaceLineEndings("\n").Trim().Replace("\t", "    "))


[<Extension>]
type Assertions =


    [<Extension>]
    static member TestDerived(t: Testable<'a>, pass) : AndDerived<'a, 'a> =
        use _ = t.Assert()

        if not pass then
            Fail(t, None).Throw("{subject}")

        AndDerived(t, t.Subject)


    [<Extension>]
    static member Test(t: Testable<'a>, pass) : And<'a> =
        use _ = t.Assert()

        if not pass then
            Fail(t, None).Throw("{subject}")

        And(t)


    [<Extension>]
    static member TestSatisfy(t: Testable<'a>, assertion) : And<'a> =
        use x = t.Assert(true)

        try
            assertion t.Subject |> ignore
            And(t)
        with :? AssertionFailedException as ex ->
            Fail(t, None).Throw("{subject}{0}", ex.Message)


    [<Extension>]
    static member TestSatisfyAny(t: Testable<'a>, assertions: seq<'a -> 'ignored>) : And<'a> =
        use _ = t.Assert(true)
        let assertions = assertions |> Seq.toArray

        let exceptions =
            assertions
            |> Array.choose (fun f ->
                try
                    f t.Subject |> ignore
                    None
                with :? AssertionFailedException as ex ->
                    Some ex
            )

        if exceptions.Length = assertions.Length then
            let assertionFailuresString =
                exceptions |> Seq.map (fun ex -> ex.Message) |> String.concat ""

            Fail(t, None).Throw("{subject}{0}", assertionFailuresString)

        And(t)


    [<Extension>]
    static member PassDerived(t: Testable<'a>) : AndDerived<'a, 'a> =
        use _ = t.Assert()
        t.TestDerived(true)


    [<Extension>]
    static member Pass(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()
        t.Test(true)


    [<Extension>]
    static member FailDerived(t: Testable<'a>) : AndDerived<'a, 'a> =
        use _ = t.Assert()
        t.TestDerived(false)


    [<Extension>]
    static member Fail(t: Testable<'a>) : And<'a> =
        use _ = t.Assert()
        t.Test(false)
