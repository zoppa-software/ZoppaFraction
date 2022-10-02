Option Strict On
Option Explicit On
Imports System.Runtime.CompilerServices

Public Module FractionModule

    <Extension>
    Public Function ChangeFraction(value As Integer) As Fraction
        Return Fraction.Create(value)
    End Function

    <Extension>
    Public Function ChangeFraction(value As Double) As Fraction
        Return Fraction.Create(value)
    End Function

    Public Function CFra(value As Integer) As Fraction
        Return Fraction.Create(value)
    End Function

    Public Function CFra(value As Long) As Fraction
        Return Fraction.Create(value)
    End Function

    Public Function CFra(value As Double) As Fraction
        Return Fraction.Create(value)
    End Function

End Module
