Imports System
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary
Imports Xunit
Imports ZoppaFraction

Public Class FractionTest

    <Fact>
    Sub ConvertTest()
        Dim a1 = CFra(Decimal.Parse("0.1"))
        Assert.Equal(a1.ToString(), "0.1")

        Dim a2 = CFra(Decimal.Parse("-0.1"))
        Assert.Equal(a2.ToString(), "-0.1")

        Dim a3 = CFra(Decimal.Parse("123456789.123456789"))
        Assert.Equal(a3.ToString(), "123456789.123456789")

        Assert.Equal(CDec(a3), Decimal.Parse("123456789.123456789"))

        Dim a4 As Fraction = Nothing
        Assert.True(Fraction.TryParse("-12.345", a4))
        Assert.Equal(a4.ToString(), "-12.345")

        Assert.Throws(Of FormatException)(
            Sub()
                Fraction.Parse("-1.123.5")
            End Sub
        )
    End Sub

    <Fact>
    Sub NumberTest()
        Dim a1 = Fraction.Create(1, 10)
        Dim a2 = a1 + a1 + a1 + a1 + a1
        Assert.Equal(CDbl(a2), 0.5)

        Dim ans = CFra(1.0) / 3.0 * 3.0
        Assert.Equal(ans, 1)

        ' ïÇìÆè¨êîì_Ç≈ÇÕà»â∫ÇÃåãâ ÇÕ 1Ç…Ç»ÇËÇ‹ÇπÇÒ
        Dim a = 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1

        ' Ç±ÇøÇÁÇÕ 1Ç…Ç»ÇËÇ‹Ç∑
        Dim n = Fraction.Parse("0.1")
        Assert.Equal(CDbl(n + n + n + n + n + n + n + n + n + n), 1)


        'Dim a1 = CFra(5) + CFra(-8)
        'Assert.Equal(CDbl(a1), -3)

        'Dim a2 = CFra(5) + CFra(-4)
        'Assert.Equal(CDbl(a2), 1)

        'Dim a3 = CFra(-1) - CFra(-7)
        'Assert.Equal(CDbl(a3), 6)

        'Dim spe1 = (CFra(Long.MaxValue) + CFra(Long.MaxValue)) / 2
        'Assert.Equal(CDbl(spe1), 9.2233720368547758E+18)
        'Stop
    End Sub

    <Fact>
    Sub ToStringTest()
        Dim a1 = Fraction.Create(19999, 10000)
        Assert.Equal(a1.ToString(2), "2")

        Dim a2 = Fraction.Create(-5, 3)
        Assert.Equal(a2.ToString(), "-1.6666666666666666666666667")
    End Sub

    <Fact>
    Sub SerializationTest()
        Dim v = Fraction.Create(1.2345)

        Using mem As New MemoryStream()
            Dim formatter As New BinaryFormatter()
            formatter.Serialize(mem, v)

            mem.Position = 0
            Dim ans = CType(formatter.Deserialize(mem), Fraction)

            Assert.Equal(ans, v)
        End Using
    End Sub

End Class



