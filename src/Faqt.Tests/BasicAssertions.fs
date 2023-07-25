﻿module BasicAssertions

open System
open System.Collections.Generic
open System.IO
open Faqt
open Xunit


module Be =


    [<Fact>]
    let ``Passes for equal integers and can be chained with And`` () =
        (1).Should().Be(1).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes for equal custom type and can be chained with And`` () =
        let x = {| A = 1; B = "foo" |}

        x
            .Should()
            .Be({| A = 1; B = "foo" |})
            .Id<And<{| A: int; B: string |}>>()
            .And.Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Fails with expected message for unequal integers`` () =
        fun () ->
            let x = 1
            x.Should().Be(2)
        |> assertExnMsg
            """
x
    should be
2
    but was
1
"""


    [<Fact>]
    let ``Fails with expected message for unequal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().Be({| A = 2; B = "bar" |})
        |> assertExnMsg
            """
x
    should be
{ A = 2
  B = "bar" }
    but was
{ A = 1
  B = "foo" }
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().Be(2, "some reason")
        |> assertExnMsg
            """
x
    should be
2
    because some reason, but was
1
"""


module ``Be with custom comparer`` =


    [<Fact>]
    let ``Passes if isEqual returns true and can be chained with AndDerived with expected value`` () =
        let isEqual _ _ = true

        (1)
            .Should()
            .Be("asd", isEqual)
            .Id<AndDerived<int, string>>()
            .WhoseValue.Should()
            .Be("asd")


    [<Fact>]
    let ``Fails with expected message if isEqual returns false`` () =
        let isEqual _ _ = false

        fun () ->
            let x = 1
            x.Should().Be(2, isEqual)
        |> assertExnMsg
            """
x
    should be
2
    but the specified equality comparer returned false when comparing it to
1
"""


    [<Fact>]
    let ``Fails with expected message if isEqual returns false even if values are equal using (=)`` () =
        let isEqual _ _ = false

        fun () ->
            let x = 1
            x.Should().Be(1, isEqual)
        |> assertExnMsg
            """
x
    should be
1
    but the specified equality comparer returned false when comparing it to
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let isEqual _ _ = false

        fun () ->
            let x = 1
            x.Should().Be(1, isEqual, "some reason")
        |> assertExnMsg
            """
x
    should be
1
    because some reason, but the specified equality comparer returned false when comparing it to
1
"""


module NotBe =


    [<Fact>]
    let ``Passes for unequal integers and can be chained with And`` () =
        (1).Should().NotBe(2).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Passes for unequal custom type and can be chained with And`` () =
        let x = {| A = 1; B = "foo" |}

        x
            .Should()
            .NotBe({| A = 2; B = "bar" |})
            .Id<And<{| A: int; B: string |}>>()
            .And.Be({| A = 1; B = "foo" |})


    [<Fact>]
    let ``Fails with expected message for equal integers`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x)
        |> assertExnMsg
            """
x
    should not be
1
    but the values were equal.
"""


    [<Fact>]
    let ``Fails with expected message for equal custom type`` () =
        fun () ->
            let x = {| A = 1; B = "foo" |}
            x.Should().NotBe(x)
        |> assertExnMsg
            """
x
    should not be
{ A = 1
  B = "foo" }
    but the values were equal.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = 1
            x.Should().NotBe(x, "some reason")
        |> assertExnMsg
            """
x
    should not be
1
    because some reason, but the values were equal.
"""


module ``NotBe with custom comparer`` =


    [<Fact>]
    let ``Passes if isEqual returns false and can be chained with And`` () =
        let isEqual _ _ = false
        (1).Should().NotBe("asd", isEqual).Id<And<int>>().And.Be(1)


    [<Fact>]
    let ``Fails with expected message if isEqual returns true, even if the values are not equal using (=)`` () =
        let isEqual _ _ = true

        fun () ->
            let x = 1
            x.Should().NotBe(2, isEqual)
        |> assertExnMsg
            """
x
    should not be
2
    but the specified equality comparer returned true when comparing it to
1
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let isEqual _ _ = true

        fun () ->
            let x = 1
            x.Should().NotBe(1, isEqual, "some reason")
        |> assertExnMsg
            """
x
    should not be
1
    because some reason, but the specified equality comparer returned true when comparing it to
1
"""


module BeSameAs =


    [<Fact>]
    let ``Passes for reference equal values and can be chained with And`` () =
        let x = "asd"
        let y = x
        x.Should().BeSameAs(y).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Throws ArgumentNullException for null argument`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "a".Should().BeSameAs(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message for null`` () =
        let x = null
        let y = "asd"

        fun () -> x.Should().BeSameAs(y)
        |> assertExnMsg
            $"""
x
    should be reference equal to
%i{LanguagePrimitives.PhysicalHash y} System.String
"asd"
    but was
null
"""


    [<Fact>]
    let ``Fails with expected message for non-reference-equal values of generic type even if they are equal`` () =
        let x = Map.empty.Add("a", 1)
        let y = Map.empty.Add("a", 1)
        Assert.True((x = y)) // Sanity check

        fun () -> x.Should().BeSameAs(y)
        |> assertExnMsg
            $"""
x
    should be reference equal to
%i{LanguagePrimitives.PhysicalHash y} Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
map [("a", 1)]
    but was
%i{LanguagePrimitives.PhysicalHash x} Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
map [("a", 1)]
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let x = "a"
        let y = "b"

        fun () -> x.Should().BeSameAs(y, "some reason")
        |> assertExnMsg
            $"""
x
    should be reference equal to
%i{LanguagePrimitives.PhysicalHash y} System.String
"b"
    because some reason, but was
%i{LanguagePrimitives.PhysicalHash x} System.String
"a"
"""


module NotBeSameAs =


    [<Fact>]
    let ``Passes for non-reference equal values and can be chained with And`` () =
        "asd".Should().NotBeSameAs("foo").Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes for null`` () =
        null.Should().NotBeSameAs("foo").Id<And<string>>()


    [<Fact>]
    let ``Throws ArgumentNullException for null argument`` () =
        Assert.Throws<ArgumentNullException>(fun () -> "a".Should().NotBeSameAs(null) |> ignore)


    [<Fact>]
    let ``Fails with expected message for reference-equal values of generic type`` () =
        let x = Map.empty.Add("a", 1)
        let y = x

        fun () -> x.Should().NotBeSameAs(y)
        |> assertExnMsg
            $"""
x
    should not be reference equal to
%i{LanguagePrimitives.PhysicalHash y} Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
map [("a", 1)]
    but was the same reference.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        let x = "a"
        let y = x

        fun () -> x.Should().NotBeSameAs(y, "some reason")
        |> assertExnMsg
            $"""
x
    should not be reference equal to
%i{LanguagePrimitives.PhysicalHash y} System.String
"a"
    because some reason, but was the same reference.
"""


module BeNull =


    [<Fact>]
    let ``Passes for null and can be chained with And`` () =
        (null: string).Should().BeNull().Id<And<string>>().And.BeNull()


    [<Fact>]
    let ``Fails with expected message if not null`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull()
        |> assertExnMsg
            """
x
    should be null, but was
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeNull("some reason")
        |> assertExnMsg
            """
x
    should be null because some reason, but was
"asd"
"""


module NotBeNull =


    [<Fact>]
    let ``Passes for non-null and can be chained with And`` () =
        "asd".Should().NotBeNull().Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().NotBeNull()
        |> assertExnMsg
            """
x
    should not be null, but was null.
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let (x: string) = null
            x.Should().NotBeNull("some reason")
        |> assertExnMsg
            """
x
    should not be null because some reason, but was null.
"""


module ``BeOfType non-generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with And`` () =
        "asd".Should().BeOfType(typeof<string>).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with And`` () =
        (box "asd").Should().BeOfType(typeof<string>).Id<And<obj>>().And.Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeOfType(typeof<string>)
        |> assertExnMsg
            """
x
    should be of type
System.String
    but was
null
"""


    [<Fact>]
    let ``Fails with expected message for different types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType(typeof<int>)
        |> assertExnMsg
            """
x
    should be of type
System.Int32
    but was
System.String
    with data
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if non-generic interface, even if type implements it`` () =
        fun () ->
            let x = new MemoryStream()
            x :> IDisposable |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<IDisposable>)
        |> assertExnMsg
            """
x
    should be of type
System.IDisposable
    but was
System.IO.MemoryStream
    with data
System.IO.MemoryStream
"""


    [<Fact>]
    let ``Fails with expected message if generic interface, even if type implements it`` () =
        fun () ->
            let x: Map<string, int> = Map.empty
            x :> IDictionary<string, int> |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<IDictionary<string, int>>)
        |> assertExnMsg
            """
x
    should be of type
System.Collections.Generic.IDictionary<System.String, System.Int32>
    but was
Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
    with data
map []
"""


    [<Fact>]
    let ``Fails with expected message if sub-type`` () =
        fun () ->
            let x = new MemoryStream()
            x :> Stream |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType(typeof<Stream>)
        |> assertExnMsg
            """
x
    should be of type
System.IO.Stream
    but was
System.IO.MemoryStream
    with data
System.IO.MemoryStream
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType(typeof<int>, "some reason")
        |> assertExnMsg
            """
x
    should be of type
System.Int32
    because some reason, but was
System.String
    with data
"asd"
"""


module ``BeOfType generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with AndDerived with cast value`` () =
        "asd"
            .Should()
            .BeOfType<string>()
            .Id<AndDerived<string, string>>()
            .WhoseValue.Should()
            .Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with AndDerived with cast value`` () =
        (box "asd")
            .Should()
            .BeOfType<string>()
            .Id<AndDerived<obj, string>>()
            .WhoseValue.Should()
            .Be("asd")


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeOfType<string>()
        |> assertExnMsg
            """
x
    should be of type
System.String
    but was
null
"""


    [<Fact>]
    let ``Fails with expected message for different types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType<int>()
        |> assertExnMsg
            """
x
    should be of type
System.Int32
    but was
System.String
    with data
"asd"
"""


    [<Fact>]
    let ``Fails with expected message if non-generic interface, even if type implements it`` () =
        fun () ->
            let x = new MemoryStream()
            x :> IDisposable |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<IDisposable>()
        |> assertExnMsg
            """
x
    should be of type
System.IDisposable
    but was
System.IO.MemoryStream
    with data
System.IO.MemoryStream
"""


    [<Fact>]
    let ``Fails with expected message if generic interface, even if type implements it`` () =
        fun () ->
            let x: Map<string, int> = Map.empty
            x :> IDictionary<string, int> |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<IDictionary<string, int>>()
        |> assertExnMsg
            """
x
    should be of type
System.Collections.Generic.IDictionary<System.String, System.Int32>
    but was
Microsoft.FSharp.Collections.FSharpMap<System.String, System.Int32>
    with data
map []
"""


    [<Fact>]
    let ``Fails with expected message if sub-type`` () =
        fun () ->
            let x = new MemoryStream()
            x :> Stream |> ignore // Sanity check to avoid false negatives
            x.Should().BeOfType<Stream>()
        |> assertExnMsg
            """
x
    should be of type
System.IO.Stream
    but was
System.IO.MemoryStream
    with data
System.IO.MemoryStream
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeOfType<int>("some reason")
        |> assertExnMsg
            """
x
    should be of type
System.Int32
    because some reason, but was
System.String
    with data
"asd"
"""


module ``BeAssignableTo non-generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with And`` () =
        "asd".Should().BeAssignableTo(typeof<string>).Id<And<string>>().And.Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with And`` () =
        (box "asd").Should().BeAssignableTo(typeof<string>).Id<And<obj>>().And.Be("asd")


    [<Fact>]
    let ``Passes for instance of type that implements specified interface`` () =
        let x = new MemoryStream()
        x.Should().BeAssignableTo(typeof<IDisposable>)


    [<Fact>]
    let ``Passes for boxed instance of type that implements specified interface`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo(typeof<IDisposable>)


    [<Fact>]
    let ``Passes for instance of subtype of specified type`` () =
        let x = new MemoryStream()
        x.Should().BeAssignableTo(typeof<Stream>)


    [<Fact>]
    let ``Passes for boxed instance of subtype of specified type`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo(typeof<Stream>)


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeAssignableTo(typeof<string>)
        |> assertExnMsg
            """
x
    should be assignable to
System.String
    but was
null
"""


    [<Fact>]
    let ``Fails with expected message for incompatible types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo(typeof<int>)
        |> assertExnMsg
            """
x
    should be assignable to
System.Int32
    but was
System.String
    with data
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo(typeof<int>, "some reason")
        |> assertExnMsg
            """
x
    should be assignable to
System.Int32
    because some reason, but was
System.String
    with data
"asd"
"""


module ``BeAssignableTo generic`` =


    [<Fact>]
    let ``Passes for instance of specified type and can be chained with AndDerived with cast value`` () =
        "asd"
            .Should()
            .BeAssignableTo<string>()
            .Id<AndDerived<string, string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes for boxed instance of specified type and can be chained with AndDerived with cast value`` () =
        (box "asd")
            .Should()
            .BeAssignableTo<string>()
            .Id<AndDerived<obj, string>>()
            .And.Be("asd")


    [<Fact>]
    let ``Passes for instance of type that implements specified interface`` () =
        let x = new MemoryStream()
        x.Should().BeAssignableTo<IDisposable>()


    [<Fact>]
    let ``Passes for boxed instance of type that implements specified interface`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo<IDisposable>()


    [<Fact>]
    let ``Passes for instance of subtype of specified type`` () =
        let x = new MemoryStream()
        x.Should().BeAssignableTo<Stream>()


    [<Fact>]
    let ``Passes for boxed instance of subtype of specified type`` () =
        let x = new MemoryStream()
        (box x).Should().BeAssignableTo<Stream>()


    [<Fact>]
    let ``Fails with expected message if null`` () =
        fun () ->
            let (x: string) = null
            x.Should().BeAssignableTo<string>()
        |> assertExnMsg
            """
x
    should be assignable to
System.String
    but was
null
"""


    [<Fact>]
    let ``Fails with expected message for incompatible types`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo<int>()
        |> assertExnMsg
            """
x
    should be assignable to
System.Int32
    but was
System.String
    with data
"asd"
"""


    [<Fact>]
    let ``Fails with expected message with because`` () =
        fun () ->
            let x = "asd"
            x.Should().BeAssignableTo<int>("some reason")
        |> assertExnMsg
            """
x
    should be assignable to
System.Int32
    because some reason, but was
System.String
    with data
"asd"
"""
