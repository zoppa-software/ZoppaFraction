Imports System.Runtime.InteropServices
Imports Xunit
Imports ZoppaFraction

Public Class VariableIntegerTest

    <Fact>
    Sub AdditionTest()
        ' +, +
        Dim a1 = New VariableInteger(50275) + New VariableInteger(98732)
        Assert.Equal(CLng(a1), 149007)
        Dim a10 = New VariableInteger(98732) + New VariableInteger(50275)
        Assert.Equal(CLng(a10), 149007)
        ' +, -
        Dim a2 = New VariableInteger(51126) + New VariableInteger(-86073)
        Assert.Equal(CLng(a2), -34947)
        Dim a21 = New VariableInteger(86073) + New VariableInteger(-51126)
        Assert.Equal(CLng(a21), 34947)
        ' -, +
        Dim a3 = New VariableInteger(-65952) + New VariableInteger(56528)
        Assert.Equal(CLng(a3), -9424)
        Dim a31 = New VariableInteger(-56528) + New VariableInteger(65952)
        Assert.Equal(CLng(a31), 9424)
        ' -, -
        Dim a4 = New VariableInteger(-24681) + New VariableInteger(-84475)
        Assert.Equal(CLng(a4), -109156)
        Dim a41 = New VariableInteger(-84475) + New VariableInteger(-24681)
        Assert.Equal(CLng(a41), -109156)
    End Sub

    <Fact>
    Sub SubtractionTest()
        ' +, +
        Dim a1 = New VariableInteger(88322) - New VariableInteger(68121)
        Assert.Equal(CLng(a1), 20201)
        Dim a10 = New VariableInteger(68121) - New VariableInteger(88322)
        Assert.Equal(CLng(a10), -20201)
        ' +, -
        Dim a2 = New VariableInteger(18491) - New VariableInteger(-5352)
        Assert.Equal(CLng(a2), 23843)
        Dim a21 = New VariableInteger(5352) - New VariableInteger(-18491)
        Assert.Equal(CLng(a21), 23843)
        ' -, +
        Dim a3 = New VariableInteger(-26712) - New VariableInteger(45505)
        Assert.Equal(CLng(a3), -72217)
        Dim a31 = New VariableInteger(-45505) - New VariableInteger(26712)
        Assert.Equal(CLng(a31), -72217)
        ' -, -
        Dim a4 = New VariableInteger(-95571) - New VariableInteger(-60964)
        Assert.Equal(CLng(a4), -34607)
        Dim a41 = New VariableInteger(-60964) - New VariableInteger(-95571)
        Assert.Equal(CLng(a41), 34607)
    End Sub

    <Fact>
    Sub MultiplicationTest()
        ' +, +
        Dim a1 = New VariableInteger(29086) * New VariableInteger(478)
        Assert.Equal(CLng(a1), 13903108)
        ' +, -
        Dim a2 = New VariableInteger(5461) * New VariableInteger(-23404)
        Assert.Equal(CLng(a2), -127809244)
        ' -, +
        Dim a3 = New VariableInteger(-28419) * New VariableInteger(93504)
        Assert.Equal(CLng(a3), -2657290176)
        ' -, -
        Dim a4 = New VariableInteger(-44638) * New VariableInteger(-26024)
        Assert.Equal(CLng(a4), 1161659312)
    End Sub

    <Fact>
    Sub DivisionTest()
        ' +, +
        Dim a1 = New VariableInteger(52728) / New VariableInteger(13844)
        Assert.Equal(CLng(a1), 3)
        ' +, -
        Dim a2 = New VariableInteger(66678) / New VariableInteger(-3071)
        Assert.Equal(CLng(a2), -21)
        ' -, +
        Dim a3 = New VariableInteger(-58747) / New VariableInteger(8747)
        Assert.Equal(CLng(a3), -6)
        ' -, -
        Dim a4 = New VariableInteger(-52538) / New VariableInteger(-5)
        Assert.Equal(CLng(a4), 10507)
    End Sub

End Class
