﻿// Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.FSharp.Editor

open System
open System.Composition
open System.Collections.Immutable
open System.Threading
open System.Threading.Tasks

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Diagnostics
open Microsoft.CodeAnalysis.Host.Mef
open Microsoft.CodeAnalysis.Text
open Microsoft.CodeAnalysis.SolutionCrawler

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

open Microsoft.VisualStudio.FSharp.LanguageService

[<DiagnosticAnalyzer(FSharpCommonConstants.FSharpLanguageName)>]
type internal FSharpDocumentDiagnosticAnalyzer() =
    inherit DocumentDiagnosticAnalyzer()
    
    override this.SupportedDiagnostics with get() = ImmutableArray<DiagnosticDescriptor>.Empty

    override this.AnalyzeSyntaxAsync(document: Document, cancellationToken: CancellationToken): Task<ImmutableArray<Diagnostic>> =
        let computation = async {
            let! sourceText = document.GetTextAsync(cancellationToken) |> Async.AwaitTask
            let options = CommonRoslynHelpers.GetFSharpProjectOptionsForRoslynProject(document.Project)
            let! parseResults = FSharpChecker.Instance.ParseFileInProject(document.Name, sourceText.ToString(), options)
            
            parseResults.Errors |> Seq.iter(fun (error) -> printf "%A" error) |> ignore

            return ImmutableArray<Diagnostic>.Empty
        }

        Async.StartAsTask(computation, TaskCreationOptions.None, cancellationToken)

    override this.AnalyzeSemanticsAsync(_: Document, _: CancellationToken): Task<ImmutableArray<Diagnostic>> =
        Task.FromResult(ImmutableArray<Diagnostic>.Empty)
