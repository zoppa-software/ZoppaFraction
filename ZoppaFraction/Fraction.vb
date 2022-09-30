Option Strict On
Option Explicit On

Public Structure Fraction
    Implements IEquatable(Of Fraction), IComparable(Of Fraction)

    Public Const FEW_DIGITS As Integer = 9

    Private ReadOnly mNumerator As Integer

    Private ReadOnly mDenominator As UInteger

    Public Shared ReadOnly Property Zero As Fraction
        Get
            Return New Fraction(0, 1)
        End Get
    End Property

    Public Shared ReadOnly Property MaxValue As Fraction
        Get
            Return New Fraction(Integer.MaxValue - 1, 1)
        End Get
    End Property

    Public Shared ReadOnly Property MinValue As Fraction
        Get
            Return New Fraction(Integer.MinValue + 1, 1)
        End Get
    End Property

    Private Sub New(num As Integer, den As UInteger)
        Me.mNumerator = num
        Me.mDenominator = den
    End Sub

    Public Shared Function Create(num As Double) As Fraction
        Dim n = Math.Abs(num - Math.Truncate(num))
        Dim c As Long = 1
        For i As Integer = 0 To FEW_DIGITS - 1
            If Math.Abs(n) > Double.Epsilon Then
                n *= 10
                n = n - Math.Truncate(n)
                c = c * 10
            Else
                Exit For
            End If
        Next
        Dim numerator = CLng(num * c)
        If numerator <> 0 Then
            Dim divisor = Euclidean(Math.Abs(numerator), c)
            Return New Fraction(CInt(numerator \ divisor), CUInt(c \ divisor))
        Else
            Throw New InvalidCastException("表現できない実数値です")
        End If
    End Function

    Public Shared Function Create(num As Integer, Optional den As UInteger = 1) As Fraction
        If den <> 0 Then
            If num <> 0 Then
                If den <> 1 Then
                    Dim divisor = Euclidean(Math.Abs(num), den)
                    Return RoundFraction(num, den, divisor)
                Else
                    Return New Fraction(num, 1)
                End If
            Else
                Return Zero
            End If
        Else
            Throw New DivideByZeroException("分母が0です")
        End If
    End Function

    Private Shared Function Euclidean(num As Long, den As Long) As Long
        Dim a As Long, b As Long
        If num < den Then
            a = den
            b = num
        Else
            a = num
            b = den
        End If

        Dim r = a Mod b
        Do While r <> 0
            a = b
            b = r
            r = a Mod b
        Loop
        Return b
    End Function

    Private Shared Function Multiple(m As Long, n As Long) As Long
        Dim ans = Euclidean(m, n)
        If m > n Then
            Return (m \ ans) * n
        Else
            Return (n \ ans) * m
        End If
    End Function

    Private Shared Function RoundFraction(numerator As Long,
                                          denominator As Long,
                                          divisor As Long) As Fraction
        Do While numerator \ divisor > Integer.MaxValue OrElse numerator \ divisor < Integer.MinValue
            numerator >>= 1
            denominator >>= 1
        Loop
        If denominator <> 0 Then
            Return New Fraction(CInt(numerator \ divisor), CUInt(denominator \ divisor))
        Else
            Throw New OverflowException("表現できる数値の範囲を超えました")
        End If
    End Function

    Public Shared Operator +(lf As Fraction, rt As Fraction) As Fraction
        Dim ans = Multiple(lf.mDenominator, rt.mDenominator)
        Dim meVal = lf.mNumerator * (ans \ lf.mDenominator)
        Dim oyrVal = rt.mNumerator * (ans \ rt.mDenominator)
        Return RoundFraction(meVal + oyrVal, ans, 1)
    End Operator

    Public Shared Operator +(lf As Fraction, rt As Integer) As Fraction
        Return lf + Fraction.Create(rt)
    End Operator

    Public Shared Operator +(lf As Fraction, rt As Double) As Fraction
        Return lf + Fraction.Create(rt)
    End Operator

    Public Shared Operator +(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

    Public Shared Operator +(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

    Public Shared Operator -(lf As Fraction, rt As Fraction) As Fraction
        Dim ans = Multiple(lf.mDenominator, rt.mDenominator)
        Dim meVal = lf.mNumerator * (ans \ lf.mDenominator)
        Dim oyrVal = rt.mNumerator * (ans \ rt.mDenominator)
        Return RoundFraction(meVal - oyrVal, ans, 1)
    End Operator

    Public Shared Operator -(self As Fraction) As Fraction
        Return New Fraction(-self.mNumerator, self.mDenominator)
    End Operator

    Public Shared Operator -(lf As Fraction, rt As Integer) As Fraction
        Return lf - Fraction.Create(rt)
    End Operator

    Public Shared Operator -(lf As Fraction, rt As Double) As Fraction
        Return lf - Fraction.Create(rt)
    End Operator

    Public Shared Operator -(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

    Public Shared Operator -(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

    Public Shared Operator *(lf As Fraction, rt As Fraction) As Fraction
        Dim num = CLng(lf.mNumerator) * rt.mNumerator
        Dim den = CLng(lf.mDenominator) * rt.mDenominator
        Dim divisor = Euclidean(Math.Abs(num), den)
        Return RoundFraction(num, den, divisor)
    End Operator

    Public Shared Operator *(lf As Fraction, rt As Integer) As Fraction
        Return lf * Fraction.Create(rt)
    End Operator

    Public Shared Operator *(lf As Fraction, rt As Double) As Fraction
        Return lf * Fraction.Create(rt)
    End Operator

    Public Shared Operator *(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

    Public Shared Operator *(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

    Public Shared Operator /(lf As Fraction, rt As Fraction) As Fraction
        Dim num = CLng(lf.mNumerator) * rt.mDenominator
        Dim den = CLng(rt.mNumerator) * lf.mDenominator
        If rt.mNumerator < 0 Then
            num = -num
            den = -den
        End If
        Dim divisor = Euclidean(Math.Abs(num), den)
        Return RoundFraction(num, den, divisor)
    End Operator

    Public Shared Operator /(lf As Fraction, rt As Integer) As Fraction
        Return lf / Fraction.Create(rt)
    End Operator

    Public Shared Operator /(lf As Fraction, rt As Double) As Fraction
        Return lf / Fraction.Create(rt)
    End Operator

    Public Shared Operator /(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

    Public Shared Operator /(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

    Public Shared Operator =(lf As Fraction, rt As Fraction) As Boolean
        Return lf.Equals(rt)
    End Operator

    Public Shared Operator =(lf As Fraction, rt As Integer) As Boolean
        Return lf.Equals(Fraction.Create(rt))
    End Operator

    Public Shared Operator =(lf As Fraction, rt As Double) As Boolean
        Return lf.Equals(Fraction.Create(rt))
    End Operator

    Public Shared Operator =(lf As Integer, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    Public Shared Operator =(lf As Double, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    Public Shared Operator <>(lf As Fraction, rt As Fraction) As Boolean
        Return Not lf.Equals(rt)
    End Operator

    Public Shared Operator <>(lf As Fraction, rt As Integer) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    Public Shared Operator <>(lf As Fraction, rt As Double) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    Public Shared Operator <>(lf As Integer, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    Public Shared Operator <>(lf As Double, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is Fraction Then
            Return Me.Equals(CType(obj, Fraction))
        Else
            Return False
        End If
    End Function

    Public Overloads Function Equals(other As Fraction) As Boolean Implements IEquatable(Of Fraction).Equals
        Return (Me.mNumerator = other.mNumerator) AndAlso
               (Me.mDenominator = other.mDenominator)
    End Function

    Public Function CompareTo(other As Fraction) As Integer Implements IComparable(Of Fraction).CompareTo
        Dim lf = Me.mNumerator / Me.mDenominator
        Dim rt = other.mNumerator / other.mDenominator
        Return lf.CompareTo(rt)
    End Function

    Public Shared Widening Operator CType(ByVal self As Fraction) As Double
        Return self.mNumerator / self.mDenominator
    End Operator

    Public Shared Widening Operator CType(ByVal self As Fraction) As Integer
        Return CInt(self.mNumerator / self.mDenominator)
    End Operator

    Public Overrides Function ToString() As String
        Dim coef = Math.Pow(10, FEW_DIGITS + 1)
        Dim num = Me.mNumerator / Me.mDenominator
        Dim rest = CLng(Math.Round(num * coef, MidpointRounding.ToEven)) Mod 10
        Return $"{num.ToString("f9")}{If(rest > 0, $" ... {(rest / coef).ToString("f10")}", "")}"
    End Function

End Structure
