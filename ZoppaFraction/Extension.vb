Option Strict On
Option Explicit On

Imports System.Runtime.CompilerServices

''' <summary>拡張機能。</summary>
Public Module Extension

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











    ''' <summary>可変長整数に変換します。</summary>
    ''' <param name="value">変換する値。</param>
    ''' <returns>可変長整数。</returns>
    <Extension>
    Public Function ChangeVarInteger(value As Integer) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数に変換します。</summary>
    ''' <param name="value">変換する値。</param>
    ''' <returns>可変長整数。</returns>
    <Extension>
    Public Function ChangeVarInteger(value As Long) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数に変換します。</summary>
    ''' <param name="value">変換する値。</param>
    ''' <returns>可変長整数。</returns>
    <Extension>
    Public Function ChangeVarInteger(value As ULong) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数に変換します。</summary>
    ''' <param name="value">変換する値。</param>
    ''' <returns>可変長整数。</returns>
    Public Function CVarInt(value As Integer) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数に変換します。</summary>
    ''' <param name="value">変換する値。</param>
    ''' <returns>可変長整数。</returns>
    Public Function CVarInt(value As Long) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数に変換します。</summary>
    ''' <param name="value">変換する値。</param>
    ''' <returns>可変長整数。</returns>
    Public Function CVarInt(value As ULong) As VarInteger
        Return New VarInteger(value)
    End Function

End Module
