Option Strict On
Option Explicit On

''' <summary>分数。</summary>
Public Structure Fraction
    Implements IEquatable(Of Fraction), IComparable, IComparable(Of Fraction)

    ' 分子
    Private ReadOnly mNumerator As VariableInteger

    ' 分母
    Private ReadOnly mDenominator As VariableInteger

    ''' <summary>分子を取得します。</summary>
    ''' <returns>分子。</returns>
    Public ReadOnly Property Numerator As VariableInteger
        Get
            Return Me.mNumerator
        End Get
    End Property

    ''' <summary>分母を取得します。</summary>
    ''' <returns>分母値。</returns>
    Public ReadOnly Property Denominator As VariableInteger
        Get
            Return Me.mDenominator
        End Get
    End Property

    ''' <summary>0値を取得します。</summary>
    ''' <returns>0値。</returns>
    Public Shared ReadOnly Property Zero As Fraction
        Get
            Return New Fraction(VariableInteger.Zero, New VariableInteger(1))
        End Get
    End Property

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    Private Sub New(num As Long, den As ULong)
        Me.mNumerator = New VariableInteger(num)
        Me.mDenominator = New VariableInteger(den)
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    Private Sub New(num As VariableInteger, den As VariableInteger)
        Me.mNumerator = num
        Me.mDenominator = den
    End Sub

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">小数値。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Create(num As Double) As Fraction
        Dim tmp = BitConverter.ToInt64(BitConverter.GetBytes(num), 0)
        Dim flag = (tmp And &H8000000000000000) = 0
        Dim nexp = (tmp >> 52) And &H7FF
        Dim nnum = tmp And &HFFFFFFFFFFFFF Or &H10000000000000

        'Dim o = 1 / Math.Pow(2, nexp - 1023 - 52)
        'Dim o2 = nnum / o
        ''Dim ans = Math.Pow(2, nexp - 1023 - 52) * nnum
        ''Console.WriteLine("符号部 : {0:X}", (i >> 31) And 1)
        ''Console.WriteLine("指数部 : {0:X}", (i >> 23) And &HFF)
        ''Console.WriteLine("仮数部 : {0:X}", i And &H7FFFFF)

        Dim numerator = New VariableInteger(nnum)
        Dim denominator = New VariableInteger(CLng(1 / Math.Pow(2, nexp - 1023 - 52)))
        Dim divisor = Euclidean(numerator, denominator)
        Return New Fraction(If(flag, numerator, -numerator) / divisor, denominator / divisor)
    End Function

    ''' <summary>分数を作成します。</summary>
    ''' <param name="num">分子。</param>
    ''' <param name="den">分母。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Create(num As Integer, Optional den As UInteger = 1) As Fraction
        If den <> 0 Then
            If num <> 0 Then
                If den <> 1 Then
                    Dim numerator = New VariableInteger(num)
                    Dim denominator = New VariableInteger(den)
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
    Private Shared Function Euclidean(lf As VariableInteger,
                                      rt As VariableInteger) As VariableInteger
        Dim a As VariableInteger, b As VariableInteger
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
    Private Shared Function Multiple(lf As VariableInteger,
                                     rt As VariableInteger) As VariableInteger
        Dim ans = Euclidean(lf, rt)
        If lf.CompareTo(rt) > 0 Then
            Return (lf / ans) * rt
        Else
            Return (rt / ans) * lf
        End If
    End Function

    '''' <summary>文字列表現を取得します。</summary>
    '''' <returns>文字列。</returns>
    'Public Overrides Function ToString() As String
    '    Return $"{CDbl(Me)}"
    'End Function

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

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Fraction, rt As Fraction) As Boolean
        Return Not lf.Equals(rt)
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

#End Region

#Region "文字列の変換"

    '''' <summary>文字列から分数へ変換します。</summary>
    '''' <param name="input">変換する文字列。</param>
    '''' <returns>分数。</returns>
    'Public Shared Function Parse(input As String) As FractionOld
    '    Dim iv As Integer, dv As Double
    '    If Integer.TryParse(input, iv) Then
    '        Return FractionOld.Create(iv)
    '    ElseIf Double.TryParse(input, dv) Then
    '        Return FractionOld.Create(dv)
    '    Else
    '        Throw New FormatException("文字列の変換に失敗しました")
    '    End If
    'End Function

    '''' <summary>文字列から分数へ変数します。</summary>
    '''' <param name="input">変換する文字列。</param>
    '''' <param name="outValue">変換した分数。</param>
    '''' <returns>変換できたら真。</returns>
    'Public Shared Function TryParse(input As String, ByRef outValue As FractionOld) As Boolean
    '    Dim iv As Integer, dv As Double
    '    If Integer.TryParse(input, iv) Then
    '        outValue = FractionOld.Create(iv)
    '        Return True
    '    ElseIf Double.TryParse(input, dv) Then
    '        outValue = FractionOld.Create(dv)
    '        Return True
    '    Else
    '        Return False
    '    End If
    'End Function

#End Region

#Region "Cast"

    'Public Shared Widening Operator CType(ByVal self As FractionOld) As Double
    '    Return self.mNumerator / self.mDenominator
    'End Operator

    'Public Shared Widening Operator CType(ByVal self As FractionOld) As Integer
    '    Return CInt(self.mNumerator / self.mDenominator)
    'End Operator

#End Region

End Structure
