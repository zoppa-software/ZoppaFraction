Option Strict On
Option Explicit On

Imports System.Diagnostics.CodeAnalysis
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Security.Cryptography
Imports System.Text

''' <summary>分数。</summary>
<Serializable()>
Public NotInheritable Class Fraction
    Implements IEquatable(Of Fraction), IComparable, IComparable(Of Fraction), ISerializable

    ''' <summary>小数点以下の有効桁数。</summary>
    Public Const DECIMAL_PLACE As Integer = 25

    ' 分子
    Private ReadOnly mNumerator As VarInteger

    ' 分母
    Private ReadOnly mDenominator As VarInteger

    ''' <summary>最小値を取得します（遅延）</summary>
    ''' <returns>最小値。</returns>
    Private Shared ReadOnly Property MinValueInstance() As New Lazy(Of Fraction)(
        Function() New Fraction(Long.MinValue + 1, 1)
    )

    ''' <summary>最小値を取得します。</summary>
    ''' <returns>最小値。</returns>
    Public Shared ReadOnly Property MinValue() As Fraction
        Get
            Return MinValueInstance.Value
        End Get
    End Property

    ''' <summary>最大値を取得します（遅延）</summary>
    ''' <returns>最大値。</returns>
    Private Shared ReadOnly Property MaxValueInstance() As New Lazy(Of Fraction)(
        Function() New Fraction(Long.MaxValue, 1)
    )

    ''' <summary>最大値を取得します。</summary>
    ''' <returns>最大値。</returns>
    Public Shared ReadOnly Property MaxValue() As Fraction
        Get
            Return MaxValueInstance.Value
        End Get
    End Property

    ''' <summary>分子を取得します。</summary>
    ''' <returns>分子。</returns>
    Public ReadOnly Property Numerator As VarInteger
        Get
            Return Me.mNumerator
        End Get
    End Property

    ''' <summary>分母を取得します。</summary>
    ''' <returns>分母値。</returns>
    Public ReadOnly Property Denominator As VarInteger
        Get
            Return Me.mDenominator
        End Get
    End Property

    ''' <summary>0値を取得します。</summary>
    ''' <returns>0値。</returns>
    Public Shared ReadOnly Property Zero As Fraction
        Get
            Return New Fraction(VarInteger.Zero, New VarInteger(1))
        End Get
    End Property

    Public Sub New()
        Me.mNumerator = VarInteger.Zero
        Me.mDenominator = New VarInteger(1)
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    Private Sub New(num As Long, den As ULong)
        Me.mNumerator = New VarInteger(num)
        Me.mDenominator = New VarInteger(den)
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    Private Sub New(num As VarInteger, den As VarInteger)
        Me.mNumerator = num
        Me.mDenominator = den
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="info">インフォメーション。</param>
    ''' <param name="context">コンテキスト。</param>
    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        Dim sign = info.GetBoolean("Sign")
        Me.mNumerator = New VarInteger(sign, CType(info.GetValue("Numerator", GetType(Byte())), Byte()))
        Me.mDenominator = New VarInteger(True, CType(info.GetValue("Denominator", GetType(Byte())), Byte()))
    End Sub

    ''' <summary>シリアライズオブジェクトを取得する。</summary>
    ''' <param name="info">インフォメーション。</param>
    ''' <param name="context">コンテキスト。</param>
    Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
        info.AddValue("Sign", Me.mNumerator.IsPlusSign)
        info.AddValue("Numerator", Me.mNumerator.Raw)
        info.AddValue("Denominator", Me.mDenominator.Raw)
    End Sub

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">小数値。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Create(num As Double) As Fraction
        Dim tmp = BitConverter.ToInt64(BitConverter.GetBytes(num), 0)

        ' ビット表現を取得
        Dim flag = (tmp And &H8000000000000000) = 0
        Dim nexp = (tmp >> 52) And &H7FF
        Dim nnum = tmp And &HFFFFFFFFFFFFF Or &H10000000000000

        ' 分子、分母に変換
        Dim numerator = New VarInteger(nnum)
        Dim denominator = New VarInteger(CLng(1 / Math.Pow(2, nexp - 1023 - 52)))
        Dim divisor = Euclidean(numerator, denominator)
        Return New Fraction(If(flag, numerator, -numerator) / divisor, denominator / divisor)
    End Function

    Public Shared Function Create(num As Decimal) As Fraction
        Dim bit = Decimal.GetBits(num)

        ' ビット表現を取得
        Dim info = bit(3) >> 16
        Dim sign = info >> 15
        Dim exponent = info And &HFF

        ' 分子を取得
        Dim bit2 = BitConverter.ToUInt32(BitConverter.GetBytes(bit(2)), 0)
        Dim bit1 = BitConverter.ToUInt32(BitConverter.GetBytes(bit(1)), 0)
        Dim bit0 = BitConverter.ToUInt32(BitConverter.GetBytes(bit(0)), 0)
        Dim numerator = VarInteger.Create(bit2).LeftShift(64) + VarInteger.Create(bit1).LeftShift(32) + VarInteger.Create(bit0)

        ' 分母を取得
        Dim denominator = New VarInteger(1)
        Dim dec As New VarInteger(10)
        For i As Integer = 0 To exponent - 1
            denominator = denominator.Multiplication(dec)
        Next

        Dim divisor = Euclidean(numerator, denominator)
        Return New Fraction(If(sign = 0, numerator, -numerator) / divisor, denominator / divisor)
    End Function

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Create(num As Integer, Optional den As UInteger = 1) As Fraction
        If den <> 0 Then
            If num <> 0 Then
                If den <> 1 Then
                    Dim numerator = New VarInteger(num)
                    Dim denominator = New VarInteger(den)
                    Dim divisor = Euclidean(numerator.Abs(), denominator)
                    Return New Fraction(numerator / divisor, denominator / divisor)
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

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Create(num As Long, Optional den As ULong = 1) As Fraction
        If den <> 0 Then
            If num <> 0 Then
                If den <> 1 Then
                    Dim numerator = New VarInteger(num)
                    Dim denominator = New VarInteger(den)
                    Dim divisor = Euclidean(numerator.Abs(), denominator)
                    Return New Fraction(numerator / divisor, denominator / divisor)
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

    ''' <summary>最大公約数を取得します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>最大公約数。</returns>
    Private Shared Function Euclidean(lf As VarInteger,
                                      rt As VarInteger) As VarInteger
        Dim a As VarInteger, b As VarInteger
        If lf.CompareTo(rt) < 0 Then
            a = rt
            b = lf
        Else
            a = lf
            b = rt
        End If

        Dim r = a Mod b
        Do While Not r.IsZero
            a = b
            b = r
            r = a Mod b
        Loop
        Return b
    End Function

    ''' <summary>最小公倍数を取得します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>最小公倍数。</returns>
    Private Shared Function Multiple(lf As VarInteger,
                                     rt As VarInteger) As VarInteger
        Dim ans = Euclidean(lf, rt)
        If lf.CompareTo(rt) > 0 Then
            Return (lf / ans) * rt
        Else
            Return (rt / ans) * lf
        End If
    End Function

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列。</returns>
    Public Overrides Function ToString() As String
        Return Me.ToString(DECIMAL_PLACE)
    End Function

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列。</returns>
    Public Overloads Function ToString(number As Integer) As String
        Dim divans = Me.mNumerator.DivisionAndRemainder(Me.mDenominator)
        Dim hiAns = divans.Quotient.Abs()
        Dim lowAns = VarInteger.Zero

        If Not divans.Remainder.IsZero Then
            Dim num = divans.Remainder.Abs()
            Dim maxNum = New VarInteger(1)
            Dim dec = New VarInteger(10)
            For i As Integer = 0 To number - 1
                num = num.Multiplication(dec)
                maxNum = maxNum.Multiplication(dec)
                lowAns = lowAns.Multiplication(dec)

                Dim figre = num.DivisionAndRemainder(Me.mDenominator)
                lowAns = lowAns.Addition(figre.Quotient)
                If figre.Remainder.IsZero Then
                    Exit For
                Else
                    num = figre.Remainder
                    If i = number - 1 Then
                        num = num.Multiplication(dec)
                        figre = num.DivisionAndRemainder(Me.mDenominator)
                        If figre.Quotient.CompareTo(VarInteger.Create(5)) >= 0 Then
                            lowAns = lowAns.Addition(VarInteger.Create(1))

                            If lowAns.CompareTo(maxNum) >= 0 Then
                                hiAns = hiAns.Addition(VarInteger.Create(1))
                                lowAns = VarInteger.Zero
                            End If
                        End If
                    End If
                End If
            Next
        End If

        Dim res As New StringBuilder()
        If divans.Remainder.IsMinusSign Then
            res.Append("-")
        End If
        res.Append(hiAns.ToString())
        If Not lowAns.IsZero Then
            res.Append(".")
            res.Append(lowAns.ToString())
        End If
        Return res.ToString()
    End Function

#Region "加算"

    ''' <summary>数値を加算します。</summary>
    ''' <param name="other">加算する値。</param>
    ''' <returns>加算結果。</returns>
    Public Function Addition(other As Fraction) As Fraction
        Dim ans = Multiple(Me.mDenominator, other.mDenominator)
        Dim value = Me.mNumerator * (ans / Me.mDenominator) +
                    other.mNumerator * (ans / other.mDenominator)
        Dim divisor = Euclidean(value.Abs(), ans)
        Return New Fraction(value / divisor, ans / divisor)
    End Function

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Fraction, rt As Fraction) As Fraction
        Return lf.Addition(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Fraction, rt As Integer) As Fraction
        Return lf + Fraction.Create(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Fraction, rt As Long) As Fraction
        Return lf + Fraction.Create(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Fraction, rt As Double) As Fraction
        Return lf + Fraction.Create(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Fraction, rt As Decimal) As Fraction
        Return lf + Fraction.Create(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Long, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Decimal, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

#End Region

#Region "引算、符号反転"

    ''' <summary>符号を反転します。</summary>
    ''' <param name="self">反転する数値。</param>
    ''' <returns>反転結果。</returns>
    Public Shared Operator -(self As Fraction) As Fraction
        Return New Fraction(-self.mNumerator, self.mDenominator)
    End Operator

    ''' <summary>数値を引算します。</summary>
    ''' <param name="other">引算する値。</param>
    ''' <returns>引算結果。</returns>
    Public Function Subtraction(other As Fraction) As Fraction
        Dim ans = Multiple(Me.mDenominator, other.mDenominator)
        Dim value = Me.mNumerator * (ans / Me.mDenominator) -
                    other.mNumerator * (ans / other.mDenominator)
        Dim divisor = Euclidean(value.Abs(), ans)
        Return New Fraction(value / divisor, ans / divisor)
    End Function

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Fraction, rt As Fraction) As Fraction
        Return lf.Subtraction(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Fraction, rt As Integer) As Fraction
        Return lf - Fraction.Create(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Fraction, rt As Long) As Fraction
        Return lf - Fraction.Create(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Fraction, rt As Double) As Fraction
        Return lf - Fraction.Create(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Fraction, rt As Decimal) As Fraction
        Return lf - Fraction.Create(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Long, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Decimal, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

#End Region

#Region "乗算"

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="other">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Function Multiplication(other As Fraction) As Fraction
        Dim num = Me.mNumerator * other.mNumerator
        Dim den = Me.mDenominator * other.mDenominator
        Dim divisor = Euclidean(num.Abs(), den)
        Return New Fraction(num / divisor, den / divisor)
    End Function

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Fraction, rt As Fraction) As Fraction
        Return lf.Multiplication(rt)
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Fraction, rt As Integer) As Fraction
        Return lf * Fraction.Create(rt)
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Fraction, rt As Long) As Fraction
        Return lf * Fraction.Create(rt)
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Fraction, rt As Double) As Fraction
        Return lf * Fraction.Create(rt)
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Fraction, rt As Decimal) As Fraction
        Return lf * Fraction.Create(rt)
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Long, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Decimal, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

#End Region

#Region "除算"

    ''' <summary>数値を除算します。</summary>
    ''' <param name="other">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function Division(other As Fraction) As Fraction
        Dim num = Me.mNumerator * other.mDenominator
        Dim den = other.mNumerator * Me.mDenominator
        If other.mNumerator.IsMinusSign Then
            num = -num
            den = -den
        End If
        Dim divisor = Euclidean(num.Abs(), den)
        Return New Fraction(num / divisor, den / divisor)
    End Function

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Fraction, rt As Fraction) As Fraction
        Return lf.Division(rt)
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Fraction, rt As Integer) As Fraction
        Return lf / Fraction.Create(rt)
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Fraction, rt As Long) As Fraction
        Return lf / Fraction.Create(rt)
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Fraction, rt As Double) As Fraction
        Return lf / Fraction.Create(rt)
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Fraction, rt As Decimal) As Fraction
        Return lf / Fraction.Create(rt)
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Integer, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Long, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Decimal, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

#End Region

#Region "比較"

    ''' <summary>等しければ真を返します。</summary>
    ''' <param name="obj">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is Fraction Then
            Return Me.Equals(CType(obj, Fraction))
        Else
            Return False
        End If
    End Function

    ''' <summary>等しければ真を返します。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Overloads Function Equals(other As Fraction) As Boolean Implements IEquatable(Of Fraction).Equals
        Return (Me.mNumerator = other.mNumerator) AndAlso
               (Me.mDenominator = other.mDenominator)
    End Function

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Fraction, rt As Fraction) As Boolean
        Return lf.Equals(rt)
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Fraction, rt As Integer) As Boolean
        Return lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Fraction, rt As Long) As Boolean
        Return lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Fraction, rt As Double) As Boolean
        Return lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Fraction, rt As Decimal) As Boolean
        Return lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Integer, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Long, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Double, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Decimal, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Fraction, rt As Fraction) As Boolean
        Return Not lf.Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Fraction, rt As Integer) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Fraction, rt As Long) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Fraction, rt As Double) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Fraction, rt As Decimal) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Integer, rt As Fraction) As Boolean
        Return Not Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Long, rt As Fraction) As Boolean
        Return Not Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Double, rt As Fraction) As Boolean
        Return Not Fraction.Create(lf).Equals(rt)
    End Operator
    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Decimal, rt As Fraction) As Boolean
        Return Not Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>比較を行います。</summary>
    ''' <param name="obj">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
        If TypeOf obj Is Fraction Then
            Return Me.CompareTo(CType(obj, Fraction))
        Else
            Throw New ArgumentException("比較ができません")
        End If
    End Function

    ''' <summary>比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Function CompareTo(other As Fraction) As Integer Implements IComparable(Of Fraction).CompareTo
        Dim ans = Multiple(Me.mDenominator, other.mDenominator)
        Dim lf = Me.mNumerator * (ans / Me.mDenominator)
        Dim rt = other.mNumerator * (ans / other.mDenominator)
        Return lf.CompareTo(rt)
    End Function

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Fraction, rt As Fraction) As Boolean
        Return (lf.CompareTo(rt) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Fraction, rt As Integer) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Fraction, rt As Long) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Fraction, rt As Double) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Fraction, rt As Decimal) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Integer, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Long, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Double, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Decimal, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) > 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Fraction, rt As Fraction) As Boolean
        Return (lf.CompareTo(rt) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Fraction, rt As Integer) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Fraction, rt As Long) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Fraction, rt As Double) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Fraction, rt As Decimal) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Integer, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Long, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Double, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Decimal, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) >= 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Fraction, rt As Fraction) As Boolean
        Return (lf.CompareTo(rt) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Fraction, rt As Integer) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Fraction, rt As Long) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Fraction, rt As Double) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Fraction, rt As Decimal) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Integer, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Long, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Double, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Decimal, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) < 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Fraction, rt As Fraction) As Boolean
        Return (lf.CompareTo(rt) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Fraction, rt As Integer) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Fraction, rt As Long) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Fraction, rt As Double) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Fraction, rt As Decimal) As Boolean
        Return (lf.CompareTo(Fraction.Create(rt)) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Integer, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Long, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Double, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Decimal, rt As Fraction) As Boolean
        Return (Fraction.Create(lf).CompareTo(rt) <= 0)
    End Operator

#End Region

#Region "文字列の変換"

    ''' <summary>文字列から分数へ変換します。</summary>
    ''' <param name="input">変換する文字列。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Parse(input As String) As Fraction
        Dim res As Fraction = Nothing
        If Fraction.TryParse(input, res) Then
            Return res
        Else
            Throw New FormatException("入力文字列の形式が正しくありません")
        End If
    End Function

    ''' <summary>文字列から分数へ変数します。</summary>
    ''' <param name="input">変換する文字列。</param>
    ''' <param name="outValue">変換した分数。</param>
    ''' <returns>変換できたら真。</returns>
    Public Shared Function TryParse(input As String, ByRef outValue As Fraction) As Boolean
        input = If(input?.Trim(), "")

        If input <> "" Then
            Dim hiNum = VarInteger.Zero
            Dim lowNum = VarInteger.Create(1)
            Dim dec As New VarInteger(10)
            Dim point = False

            Dim strIndex As Integer = 0
            Dim sign As Boolean = True
            If input.StartsWith("+") Then
                sign = True
                strIndex = 1
            ElseIf input.StartsWith("-") Then
                sign = False
                strIndex = 1
            Else
                sign = True
                strIndex = 0
            End If

            For i As Integer = strIndex To input.Length - 1
                If point Then
                    lowNum = lowNum.Multiplication(dec)
                End If

                Select Case input(i)
                    Case "0"c To "9"c
                        hiNum = hiNum.Multiplication(dec) + New VarInteger(AscW(input(i)) - AscW("0"))

                    Case "."c
                        If Not point Then
                            point = True
                        Else
                            Return False
                        End If

                    Case Else
                        Return False
                End Select
            Next

            outValue = New Fraction(New VarInteger(sign, hiNum.Raw), lowNum)
            Return True
        Else
            Return False
        End If
    End Function

#End Region

#Region "Cast"

    ''' <summary>Double型にキャストします。</summary>
    ''' <param name="self">分数。</param>
    ''' <returns>キャスト後の値。</returns>
    Public Shared Narrowing Operator CType(ByVal self As Fraction) As Double
        Dim num = self.mNumerator
        Dim den = self.mDenominator
        Do While num.CompareTo(MinValue.mNumerator) < 0 OrElse
                 num.CompareTo(MaxValue.mNumerator) > 0
            num.RightShift()
            den.RightShift()
        Loop
        If den <> VarInteger.Zero Then
            Return CLng(num) / CLng(den)
        Else
            Throw New OverflowException("浮動小数点へ変換出来ませんでした")
        End If
    End Operator

    ''' <summary>Long型にキャストします。</summary>
    ''' <param name="self">分数。</param>
    ''' <returns>キャスト後の値。</returns>
    Public Shared Narrowing Operator CType(ByVal self As Fraction) As Long
        Dim ans = self.mNumerator / self.mDenominator
        If ans.CompareTo(MinValue.mNumerator) >= 0 OrElse
           ans.CompareTo(MaxValue.mNumerator) <= 0 Then
            Return CLng(ans)
        Else
            Throw New OverflowException("64bit整数で表現できない")
        End If
    End Operator

    ''' <summary>Decimal型にキャストします。</summary>
    ''' <param name="self">分数。</param>
    ''' <returns>キャスト後の値。</returns>
    Public Shared Narrowing Operator CType(ByVal self As Fraction) As Decimal
        Dim res As Decimal
        If Decimal.TryParse(self.ToString(), res) Then
            Return res
        Else
            Throw New OverflowException("Decimalで表現できません")
        End If
    End Operator

    ''' <summary>Double型からキャストします。</summary>
    ''' <param name="self">数値。</param>
    ''' <returns>分数値。</returns>
    Public Shared Narrowing Operator CType(ByVal value As Double) As Fraction
        Return Fraction.Create(value)
    End Operator

    ''' <summary>Double型からキャストします。</summary>
    ''' <param name="self">数値。</param>
    ''' <returns>分数値。</returns>
    Public Shared Narrowing Operator CType(ByVal value As Long) As Fraction
        Return Fraction.Create(value)
    End Operator

    ''' <summary>Double型からキャストします。</summary>
    ''' <param name="self">数値。</param>
    ''' <returns>分数値。</returns>
    Public Shared Narrowing Operator CType(ByVal value As Decimal) As Fraction
        Return Fraction.Create(value)
    End Operator

#End Region

End Class
