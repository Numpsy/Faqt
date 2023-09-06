﻿namespace Faqt.Configuration

open System
open System.Threading


type FaqtConfig = private {
    // TODO
    dummy: unit
} with


    /// Returns the default config. Note that changes to this config is not considered a breaking change. The config is
    /// immutable; all instance methods return a new instance.
    static member Default = { dummy = () }


/// Allows changing the current formatter, either temporarily (for the current thread) or globally.
type Config private () =


    static let mutable globalConfig: FaqtConfig = FaqtConfig.Default


    static let localConfig: AsyncLocal<FaqtConfig> = AsyncLocal()


    static member Current =
        if isNull (box localConfig.Value) then
            globalConfig
        else
            localConfig.Value


    /// Sets the specified config as the default global config.
    static member Set(config) = globalConfig <- config


    /// Sets the specified config as the config for the current thread. When the returned value is disposed, the old
    /// config is restored.
    static member With(config) =
        let oldFormatter = localConfig.Value
        localConfig.Value <- config

        { new IDisposable with
            member _.Dispose() = localConfig.Value <- oldFormatter
        }
