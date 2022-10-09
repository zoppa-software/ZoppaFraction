Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Runtime.Serialization.Formatters.Binary
Imports Xunit
Imports ZoppaFraction

Public Class VarIntegerTest

    <Fact>
    Sub AdditionTest()
        ' +, +
        Dim a1 = New VarInteger(50275) + New VarInteger(98732)
        Assert.Equal(CLng(a1), 149007)
        Dim a10 = New VarInteger(98732) + New VarInteger(50275)
        Assert.Equal(CLng(a10), 149007)
        ' +, -
        Dim a2 = New VarInteger(51126) + New VarInteger(-86073)
        Assert.Equal(CLng(a2), -34947)
        Dim a21 = New VarInteger(86073) + New VarInteger(-51126)
        Assert.Equal(CLng(a21), 34947)
        ' -, +
        Dim a3 = New VarInteger(-65952) + New VarInteger(56528)
        Assert.Equal(CLng(a3), -9424)
        Dim a31 = New VarInteger(-56528) + New VarInteger(65952)
        Assert.Equal(CLng(a31), 9424)
        ' -, -
        Dim a4 = New VarInteger(-24681) + New VarInteger(-84475)
        Assert.Equal(CLng(a4), -109156)
        Dim a41 = New VarInteger(-84475) + New VarInteger(-24681)
        Assert.Equal(CLng(a41), -109156)
    End Sub

    <Fact>
    Sub SubtractionTest()
        ' +, +
        Dim a1 = New VarInteger(88322) - New VarInteger(68121)
        Assert.Equal(CLng(a1), 20201)
        Dim a10 = New VarInteger(68121) - New VarInteger(88322)
        Assert.Equal(CLng(a10), -20201)
        ' +, -
        Dim a2 = New VarInteger(18491) - New VarInteger(-5352)
        Assert.Equal(CLng(a2), 23843)
        Dim a21 = New VarInteger(5352) - New VarInteger(-18491)
        Assert.Equal(CLng(a21), 23843)
        ' -, +
        Dim a3 = New VarInteger(-26712) - New VarInteger(45505)
        Assert.Equal(CLng(a3), -72217)
        Dim a31 = New VarInteger(-45505) - New VarInteger(26712)
        Assert.Equal(CLng(a31), -72217)
        ' -, -
        Dim a4 = New VarInteger(-95571) - New VarInteger(-60964)
        Assert.Equal(CLng(a4), -34607)
        Dim a41 = New VarInteger(-60964) - New VarInteger(-95571)
        Assert.Equal(CLng(a41), 34607)
    End Sub

    <Fact>
    Sub MultiplicationTest()
        ' +, +
        Dim a1 = New VarInteger(29086) * New VarInteger(478)
        Assert.Equal(CLng(a1), 13903108)
        ' +, -
        Dim a2 = New VarInteger(5461) * New VarInteger(-23404)
        Assert.Equal(CLng(a2), -127809244)
        ' -, +
        Dim a3 = New VarInteger(-28419) * New VarInteger(93504)
        Assert.Equal(CLng(a3), -2657290176)
        ' -, -
        Dim a4 = New VarInteger(-44638) * New VarInteger(-26024)
        Assert.Equal(CLng(a4), 1161659312)
    End Sub

    <Fact>
    Sub DivisionTest()
        ' +, +
        Dim a1 = New VarInteger(52728) / New VarInteger(13844)
        Assert.Equal(CLng(a1), 3)
        ' +, -
        Dim a2 = New VarInteger(66678) / New VarInteger(-3071)
        Assert.Equal(CLng(a2), -21)
        ' -, +
        Dim a3 = New VarInteger(-58747) / New VarInteger(8747)
        Assert.Equal(CLng(a3), -6)
        ' -, -
        Dim a4 = New VarInteger(-52538) / New VarInteger(-5)
        Assert.Equal(CLng(a4), 10507)
    End Sub

    <Fact>
    Sub CompareTest()
        Assert.True(CVarInt(3) > CVarInt(2))
        Assert.False(CVarInt(2) > CVarInt(2))
        Assert.False(CVarInt(1) > CVarInt(2))
        Assert.True(CVarInt(1) > CVarInt(-1))
        Assert.False(CVarInt(-1) > CVarInt(1))
        Assert.True(CVarInt(-2) > CVarInt(-3))

        Assert.True(CVarInt(91718) < CVarInt(292379))
        Assert.False(CVarInt(91718) < CVarInt(77235))
        Assert.False(CVarInt(77235) < CVarInt(77235))
        Assert.False(CVarInt(91718) < CVarInt(-45689))
        Assert.True(CVarInt(-91718) < CVarInt(45689))
        Assert.True(CVarInt(-56728) < CVarInt(-45689))

        Assert.True(CVarInt(-68801) >= CVarInt(-68801))
        Assert.True(CVarInt(-56728) <= CVarInt(-56728))
    End Sub

    <Fact>
    Sub SerializationTest()
        Dim v As New VarInteger(30822)

        Using mem As New MemoryStream()
            Dim formatter As New BinaryFormatter()
            formatter.Serialize(mem, v)

            mem.Position = 0
            Dim ans = CType(formatter.Deserialize(mem), VarInteger)

            Assert.Equal(ans, v)
        End Using
    End Sub

    <Fact>
    Sub ShiftTest()
        For i As Integer = 30 To 1 Step -1
            Dim ival = 1 << i
            Dim vval = CVarInt(1).LeftShift(i)
            Assert.Equal(CInt(vval), ival)
        Next
    End Sub

    <Fact>
    Sub CastTest()
        Dim v1 = Integer.MinValue.ChangeVarInteger()
        Assert.Equal(CInt(v1), Integer.MinValue)

        Dim v2 = Integer.MaxValue.ChangeVarInteger()
        Assert.Equal(CInt(v2), Integer.MaxValue)

        Dim v3 = Long.MinValue.ChangeVarInteger()
        Assert.Equal(CLng(v3), Long.MinValue)

        Dim v4 = Long.MaxValue.ChangeVarInteger()
        Assert.Equal(CLng(v4), Long.MaxValue)
    End Sub

End Class
