﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.CancellationTokenParametersMustComeLastAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;
using VerifyVB = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.CancellationTokenParametersMustComeLastAnalyzer,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class CancellationTokenParametersMustComeLast
    {
        [Fact]
        public async Task NoDiagnosticInEmptyFile()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task DiagnosticForMethod()
        {
            var source = @"
using System.Threading;
class T
{
    void M(CancellationToken t, int i)
    {
    }
}";
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("T.M(System.Threading.CancellationToken, int)");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task DiagnosticWhenFirstAndLastByOtherInBetween()
        {
            var source = @"
using System.Threading;
class T
{
    void M(CancellationToken t1, int i, CancellationToken t2)
    {
    }
}";
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("T.M(System.Threading.CancellationToken, int, System.Threading.CancellationToken)");
            await VerifyCS.VerifyAnalyzerAsync(source, expected);
        }

        [Fact]
        public async Task NoDiagnosticWhenLastParam()
        {
            var test = @"
using System.Threading;
class T
{
    void M(int i, CancellationToken t)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NoDiagnosticWhenOnlyParam()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NoDiagnosticWhenParamsComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, params object[] args)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NoDiagnosticWhenOutComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, out int i)
    {
        i = 2;
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NoDiagnosticWhenRefComesAfter()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, ref int x, ref int y)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NoDiagnosticWhenOptionalParameterComesAfterNonOptionalCancellationToken()
        {
            var test = @"
using System.Threading;
class T
{
    void M(CancellationToken t, int x = 0)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task NoDiagnosticOnOverride()
        {
            var test = @"
using System.Threading;
class B
{
    protected virtual void M(CancellationToken t, int i) { }
}

class T : B
{
    protected override void M(CancellationToken t, int i) { }
}";

            // One diagnostic for the virtual, but none for the override.
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 28).WithArguments("B.M(System.Threading.CancellationToken, int)");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task NoDiagnosticOnImplicitInterfaceImplementation()
        {
            var test = @"
using System.Threading;
interface I
{
    void M(CancellationToken t, int i);
}

class T : I
{
    public void M(CancellationToken t, int i) { }
}";

            // One diagnostic for the interface, but none for the implementation.
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("I.M(System.Threading.CancellationToken, int)");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact]
        public async Task NoDiagnosticOnExplicitInterfaceImplementation()
        {
            var test = @"
using System.Threading;
interface I
{
    void M(CancellationToken t, int i);
}

class T : I
{
    void I.M(CancellationToken t, int i) { }
}";

            // One diagnostic for the interface, but none for the implementation.
            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 10).WithArguments("I.M(System.Threading.CancellationToken, int)");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact, WorkItem(1491, "https://github.com/dotnet/roslyn-analyzers/issues/1491")]
        public async Task NoDiagnosticOnCancellationTokenExtensionMethod()
        {
            var test = @"
using System.Threading;
static class C1
{
    public static void M1(this CancellationToken p1, object p2)
    {
    }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact, WorkItem(1816, "https://github.com/dotnet/roslyn-analyzers/issues/1816")]
        public async Task NoDiagnosticWhenMultipleAtEndOfParameterList()
        {
            var test = @"
using System.Threading;
static class C1
{
    public static void M1(object p1, CancellationToken token1, CancellationToken token2) { }
    public static void M2(object p1, CancellationToken token1, CancellationToken token2, CancellationToken token3) { }
    public static void M3(CancellationToken token1, CancellationToken token2, CancellationToken token3) { }
    public static void M4(CancellationToken token1, CancellationToken token2 = default(CancellationToken)) { }
    public static void M5(CancellationToken token1 = default(CancellationToken), CancellationToken token2 = default(CancellationToken)) { }
}";
            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task DiagnosticOnExtensionMethodWhenCancellationTokenIsNotFirstParameter()
        {
            var test = @"
using System.Threading;
static class C1
{
    public static void M1(this object p1, CancellationToken p2, object p3)
    {
    }
}";

            var expected = new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(5, 24).WithArguments("C1.M1(object, System.Threading.CancellationToken, object)");
            VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        [Fact, WorkItem(2281, "https://github.com/dotnet/roslyn-analyzers/issues/2281")]
        public async Task CA1068_DoNotReportOnIProgressLastAndCancellationTokenBeforeLast()
        {
            VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Threading;
using System.Threading.Tasks;

public class C
{
    public Task SomeAsync(object o, CancellationToken cancellationToken, IProgress<int> progress)
    {
        throw new NotImplementedException();
    }
}");

            VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class C
    Public Function SomeAsync(ByVal o As Object, ByVal cancellationToken As CancellationToken, ByVal progress As IProgress(Of Integer)) As Task
        Throw New NotImplementedException()
    End Function
End Class");
        }

        [Fact, WorkItem(2281, "https://github.com/dotnet/roslyn-analyzers/issues/2281")]
        public async Task CA1068_ReportOnIProgressLastAndCancellationTokenNotBeforeLast()
        {
            VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Threading;
using System.Threading.Tasks;

public class C
{
    public Task SomeAsync(CancellationToken cancellationToken, object o, IProgress<int> progress)
    {
        throw new NotImplementedException();
    }
}",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(8, 17)
                .WithArguments("C.SomeAsync(System.Threading.CancellationToken, object, System.IProgress<int>)"));

            VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class C
    Public Function SomeAsync(ByVal cancellationToken As CancellationToken, ByVal o As Object, ByVal progress As IProgress(Of Integer)) As Task
        Throw New NotImplementedException()
    End Function
End Class",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(7, 21)
                .WithArguments("Public Function SomeAsync(cancellationToken As System.Threading.CancellationToken, o As Object, progress As System.IProgress(Of Integer)) As System.Threading.Tasks.Task"));
        }

        [Fact, WorkItem(2281, "https://github.com/dotnet/roslyn-analyzers/issues/2281")]
        public async Task CA1068_OnlyExcludeOneIProgressAtTheEnd()
        {
            VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Threading;
using System.Threading.Tasks;

public class C
{
    public Task SomeAsync(CancellationToken cancellationToken, IProgress<int> progress1, IProgress<int> progress2)
    {
        throw new NotImplementedException();
    }
}",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(8, 17)
                .WithArguments("C.SomeAsync(System.Threading.CancellationToken, System.IProgress<int>, System.IProgress<int>)"));

            await VerifyVB.VerifyAnalyzerAsync(@"
Imports System
Imports System.Threading
Imports System.Threading.Tasks

Public Class C
    Public Function SomeAsync(ByVal cancellationToken As CancellationToken, ByVal progress1 As IProgress(Of Integer), ByVal progress2 As IProgress(Of Integer)) As Task
        Throw New NotImplementedException()
    End Function
End Class",
            new DiagnosticResult(CancellationTokenParametersMustComeLastAnalyzer.Rule).WithLocation(7, 21)
                .WithArguments("Public Function SomeAsync(cancellationToken As System.Threading.CancellationToken, progress1 As System.IProgress(Of Integer), progress2 As System.IProgress(Of Integer)) As System.Threading.Tasks.Task"));
        }
    }
}
