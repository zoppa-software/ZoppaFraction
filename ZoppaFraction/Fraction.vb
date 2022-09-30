Option Strict On
Option Explicit On

''' <summary>分数。</summary>
Public Structure Fraction
    Implements IEquatable(Of Fraction), IComparable, IComparable(Of Fraction)

    ''' <summary>小数点以下の有効桁数。</summary>
    Public Const FEW_DIGITS As Integer = 9

    ' 分子
    Private ReadOnly mNumerator As Integer

    ' 分母
    Private ReadOnly mDenominator As UInteger

    ''' <summary>分子を取得します。</summary>
    ''' <returns>分子。</returns>
    Public ReadOnly Property Numerator As Integer
        Get
            Return Me.mNumerator
        End Get
    End Property

    ''' <summary>分母を取得します。</summary>
    ''' <returns>分母値。</returns>
    Public ReadOnly Property Denominator As UInteger
        Get
            Return Me.mDenominator
        End Get
    End Property

    ''' <summary>0値を取得します。</summary>
    ''' <returns>0値。</returns>
    Public Shared ReadOnly Property Zero As Fraction
        Get
            Return New Fraction(0, 1)
        End Get
    End Property

    ''' <summary>最大値を取得します。</summary>
    ''' <returns>最大値。</returns>
    Public Shared ReadOnly Property MaxValue As Fraction
        Get
            Return New Fraction(Integer.MaxValue - 1, 1)
        End Get
    End Property

    ''' <summary>最小値を取得します。</summary>
    ''' <returns>最小値。</returns>
    Public Shared ReadOnly Property MinValue As Fraction
        Get
            Return New Fraction(Integer.MinValue + 1, 1)
        End Get
    End Property

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    Private Sub New(num As Integer, den As UInteger)
        Me.mNumerator = num
        Me.mDenominator = den
    End Sub

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">小数値。</param>
    ''' <returns>分数。</returns>
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

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    ''' <returns>分数。</returns>
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

    ''' <summary>最大公約数を取得します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>最大公約数。</returns>
    Private Shared Function Euclidean(lf As Long, rt As Long) As Long
        Dim a As Long, b As Long
        If lf < rt Then
            a = rt
            b = lf
        Else
            a = lf
            b = rt
        End If

        Dim r = a Mod b
        Do While r <> 0
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
    Private Shared Function Multiple(lf As Long, rt As Long) As Long
        Dim ans = Euclidean(lf, rt)
        If lf > rt Then
            Return (lf \ ans) * rt
        Else
            Return (rt \ ans) * lf
        End If
    End Function

    ''' <summary>分数の情報を範囲内に丸めます。</summary>
    ''' <param name="numerator">分子。</param>
    ''' <param name="denominator">分母。</param>
    ''' <param name="divisor">最大公約数、</param>
    ''' <returns>丸めた分数。</returns>
    Private Shared Function RoundFraction(numerator As Long,
                                          denominator As Long,
                                          divisor As Long) As Fraction
        ' 桁を調整
        Do While numerator \ divisor > Integer.MaxValue OrElse
                 numerator \ divisor < Integer.MinValue
            numerator >>= 1
            denominator >>= 1
        Loop

        ' 分母がなくなったら範囲外
        If denominator <> 0 Then
            Return New Fraction(CInt(numerator \ divisor), CUInt(denominator \ divisor))
        Else
            Throw New OverflowException("表現できる数値の範囲を超えました")
        End If
    End Function

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列。</returns>
    Public Overrides Function ToString() As String
        Return $"{CDbl(Me)}"
    End Function

#Region "Add"

    ''' <summary>数値を加算します。</summary>
    ''' <param name="other">加算する値。</param>
    ''' <returns>加算結果。</returns>
    Public Function Add(other As Fraction) As Fraction
        Return (Me + other)
    End Function

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Fraction, rt As Fraction) As Fraction
        Dim ans = Multiple(lf.mDenominator, rt.mDenominator)
        Dim meVal = lf.mNumerator * (ans \ lf.mDenominator)
        Dim oyrVal = rt.mNumerator * (ans \ rt.mDenominator)
        Return RoundFraction(meVal + oyrVal, ans, 1)
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
    Public Shared Operator +(lf As Fraction, rt As Double) As Fraction
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
    Public Shared Operator +(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) + rt
    End Operator

#End Region

#Region "Subtraction"

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
        Return (Me - other)
    End Function

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Fraction, rt As Fraction) As Fraction
        Dim ans = Multiple(lf.mDenominator, rt.mDenominator)
        Dim meVal = lf.mNumerator * (ans \ lf.mDenominator)
        Dim oyrVal = rt.mNumerator * (ans \ rt.mDenominator)
        Return RoundFraction(meVal - oyrVal, ans, 1)
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
    Public Shared Operator -(lf As Fraction, rt As Double) As Fraction
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
    Public Shared Operator -(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) - rt
    End Operator

#End Region

#Region "Multiplication"

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="other">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Function Multiplication(other As Fraction) As Fraction
        Return (Me * other)
    End Function

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="lf">被乗数、</param>
    ''' <param name="rt">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Fraction, rt As Fraction) As Fraction
        Dim num = CLng(lf.mNumerator) * rt.mNumerator
        Dim den = CLng(lf.mDenominator) * rt.mDenominator
        Dim divisor = Euclidean(Math.Abs(num), den)
        Return RoundFraction(num, den, divisor)
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
    Public Shared Operator *(lf As Fraction, rt As Double) As Fraction
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
    Public Shared Operator *(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) * rt
    End Operator

#End Region

#Region "Division"

    ''' <summary>数値を除算します。</summary>
    ''' <param name="other">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function Division(other As Fraction) As Fraction
        Return (Me / other)
    End Function

    ''' <summary>数値を除算します。</summary>
    ''' <param name="lf">被除数、</param>
    ''' <param name="rt">除数。</param>
    ''' <returns>除算結果。</returns>
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
    Public Shared Operator /(lf As Fraction, rt As Double) As Fraction
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
    Public Shared Operator /(lf As Double, rt As Fraction) As Fraction
        Return Fraction.Create(lf) / rt
    End Operator

#End Region

#Region "Compare"

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
    Public Shared Operator =(lf As Fraction, rt As Double) As Boolean
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
    Public Shared Operator =(lf As Double, rt As Fraction) As Boolean
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
    Public Shared Operator <>(lf As Fraction, rt As Double) As Boolean
        Return Not lf.Equals(Fraction.Create(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Integer, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Double, rt As Fraction) As Boolean
        Return Fraction.Create(lf).Equals(rt)
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
        Dim lf = Me.mNumerator / Me.mDenominator
        Dim rt = other.mNumerator / other.mDenominator
        Return lf.CompareTo(rt)
    End Function

#End Region

#Region "Parse"

    ''' <summary>文字列から分数へ変換します。</summary>
    ''' <param name="input">変換する文字列。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Parse(input As String) As Fraction
        Dim iv As Integer, dv As Double
        If Integer.TryParse(input, iv) Then
            Return Fraction.Create(iv)
        ElseIf Double.TryParse(input, dv) Then
            Return Fraction.Create(dv)
        Else
            Throw New FormatException("文字列の変換に失敗しました")
        End If
    End Function

    ''' <summary>文字列から分数へ変数します。</summary>
    ''' <param name="input">変換する文字列。</param>
    ''' <param name="outValue">変換した分数。</param>
    ''' <returns>変換できたら真。</returns>
    Public Shared Function TryParse(input As String, ByRef outValue As Fraction) As Boolean
        Dim iv As Integer, dv As Double
        If Integer.TryParse(input, iv) Then
            outValue = Fraction.Create(iv)
            Return True
        ElseIf Double.TryParse(input, dv) Then
            outValue = Fraction.Create(dv)
            Return True
        Else
            Return False
        End If
    End Function

#End Region

    Public Shared Widening Operator CType(ByVal self As Fraction) As Double
        Return self.mNumerator / self.mDenominator
    End Operator

    Public Shared Widening Operator CType(ByVal self As Fraction) As Integer
        Return CInt(self.mNumerator / self.mDenominator)
    End Operator

End Structure
