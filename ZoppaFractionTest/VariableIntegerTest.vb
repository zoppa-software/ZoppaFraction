Imports System.Runtime.InteropServices
Imports Xunit
Imports ZoppaFraction

Public Class VariableIntegerTest

    '<Fact>
    'Sub SubtractionTest()
    '    Dim a1 = New VariableInteger(-100) - New VariableInteger(-110)

    '    Dim a2 = New VariableInteger(-100) - New VariableInteger(-10)

    '    Dim a3 = New VariableInteger(100) - New VariableInteger(110)

    '    Dim a4 = New VariableInteger(100) - New VariableInteger(10)
    '    Stop
    'End Sub

    <Fact>
    Sub AnswerTest()
        Dim e1 = New VariableInteger(100)
        Dim e2 = New VariableInteger(110)
        Dim e3 = e1 - e2

        Dim a = New VariableInteger(4565555)
        Dim s = a.ToString()
        Dim a2 = a.DivisionAndRemainder(New VariableInteger(10))
        Dim sz = Marshal.SizeOf(New Fraction())

        Dim b = New VariableInteger(293291)
        Dim b2 = b.DivisionAndRemainder(New VariableInteger(14918))

        Dim i1 = 600 / (-7)
        Dim i2 = 600 Mod (-7)

        Dim c = New VariableInteger(600)
        Dim c2 = c.DivisionAndRemainder(New VariableInteger(-7))

        Dim d = New VariableInteger(123)
        Dim d2 = d.Multiplication(New VariableInteger(5))
        Stop
    End Sub

    <Fact>
    Sub OriginTest()
        'Dim a = Fraction.Create(999.123)
        'Stop

        Dim n1 = Fraction.Create(1, 3)
        Dim n2 = n1 / Fraction.Create(3)

        Dim n3 = n1 + n1 + n1

        Dim n4 = n1 - n1
        Stop
    End Sub

End Class
