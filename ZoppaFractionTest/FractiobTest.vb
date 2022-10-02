Imports System
Imports Xunit
Imports ZoppaFraction

Public Class FractiobTest

    <Fact>
    Sub ErrorTest()
        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = FractionOld.MaxValue + 10
            End Sub
        )

        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = FractionOld.MaxValue * 10
            End Sub
        )

        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = FractionOld.MinValue - 10
            End Sub
        )

        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = FractionOld.MinValue / 0.1
            End Sub
        )
    End Sub

    '<Fact>
    'Sub AddTest()
    '    Dim a1 = CFra(100) + 0.5
    '    Assert.Equal(CDbl(a1), 100.5)

    '    Dim b1 = 1 / CFra(3)
    '    Assert.Equal(CDbl(b1 + b1 + b1), 1.0)

    '    Dim a2 = CFra(123.4).Add(FractionOld.Create(2, 3))
    '    Assert.Equal(CDbl(a2), 124.06666666666666)
    'End Sub

    '<Fact>
    'Sub SubtractionTest()
    '    Dim a1 = CFra(100) - 0.5
    '    Assert.Equal(CDbl(a1), 99.5)

    '    Dim b1 = 1 / CFra(3)
    '    Assert.Equal(CDbl(2 - b1 - b1 - b1), 1.0)

    '    Dim a2 = CFra(15).Subtraction(FractionOld.Create(1, 2))
    '    Assert.Equal(CDbl(a2), 14.5)
    'End Sub

    '<Fact>
    'Sub MultiplicationTest()
    '    Dim a1 = CFra(100) * 2
    '    Assert.Equal(CDbl(a1), 200)

    '    Dim b1 = 1 / CFra(3)
    '    Assert.Equal(CDbl(b1 * 3), 1.0)

    '    Dim a2 = (-CFra(15)).Multiplication(FractionOld.Create(-1, 2))
    '    Assert.Equal(CDbl(a2), 7.5)
    'End Sub

    '<Fact>
    'Sub DivisionTest()
    '    Dim a1 = CFra(100) / 2
    '    Assert.Equal(CDbl(a1), 50)

    '    Dim a2 = (-CFra(6)).Division(FractionOld.Create(-3))
    '    Assert.Equal(CDbl(a2), 2)
    'End Sub

    <Fact>
    Sub EqualsTest()
        Dim a1_1 = FractionOld.Create(2, 6)
        Dim a1_2 = FractionOld.Create(6, 18)
        Assert.Equal(a1_1, a1_2)

        Dim a1_3 = FractionOld.Create(2, 7)
        Assert.NotEqual(a1_1, a1_3)
    End Sub

    <Fact>
    Sub CompareTest()
        Dim a1_1 = FractionOld.Create(2, 6)
        Dim a1_2 = FractionOld.Create(6, 18)
        Assert.Equal(a1_1.CompareTo(a1_2), 0)

        Dim a1_3 = FractionOld.Create(1, 2)
        Assert.Equal(a1_1.CompareTo(a1_3), -1)

        Dim a1_4 = FractionOld.Create(1, 4)
        Assert.Equal(a1_1.CompareTo(a1_4), 1)
    End Sub

    <Fact>
    Sub ParseTest()
        Dim a1 = FractionOld.Parse("0.33")
        Assert.Equal(a1.Numerator, 33)
        Assert.Equal(a1.Denominator, 100)

        Assert.Throws(Of FormatException)(
            Sub()
                Dim a2 = FractionOld.Parse("###")
            End Sub
        )

        Dim a3 As FractionOld
        If FractionOld.TryParse("0.5", a3) Then
            Assert.Equal(CDbl(a3), 0.5)
        Else
            Assert.True(False)
        End If
    End Sub

End Class



