﻿module Tests

open Faqt
open Configuration
open Xunit


[<Fact>]
let ``Can set, override and restore custom global formatter`` () =
    Config.Set(FaqtConfig.Default.Format(fun _ -> "GLOBAL FORMATTER"))

    fun () ->
        let x = 1
        x.Should().Be(2)
    |> assertExnMsg
        """
GLOBAL FORMATTER
"""

    do
        (use _ = Config.With(FaqtConfig.Default.Format(fun _ -> "OVERRIDDEN FORMATTER"))

         fun () ->
             let x = 1
             x.Should().Be(2)
         |> assertExnMsg
             """
        OVERRIDDEN FORMATTER
    """)

    fun () ->
        let x = 1
        x.Should().Be(2)
    |> assertExnMsg
        """
GLOBAL FORMATTER
"""
