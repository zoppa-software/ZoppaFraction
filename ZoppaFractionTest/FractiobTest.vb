Imports System
Imports Xunit
Imports ZoppaFraction

Public Class FractiobTest

    <Fact>
    Sub ErrorTest()
        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = Fraction.MaxValue + 10
            End Sub
        )

        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = Fraction.MaxValue * 10
            End Sub
        )

        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = Fraction.MinValue - 10
            End Sub
        )

        Assert.Throws(Of OverflowException)(
            Sub()
                Dim ans = Fraction.MinValue / 0.1
            End Sub
        )
    End Sub

    <Fact>
    Sub AddTest()
        Dim a1 = CFra(100) + 0.5
        Assert.Equal(CDbl(a1), 100.5)

        Dim b1 = 1 / CFra(3)
        Assert.Equal(CDbl(b1 + b1 + b1), 1.0)
    End Sub

End Class



