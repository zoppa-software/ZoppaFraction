Option Strict On
Option Explicit On
Imports System.Text

''' <summary>可変長変数。</summary>
Public Structure VariableInteger
    Implements IComparable(Of VariableInteger), IEquatable(Of VariableInteger)

    ' 符号
    Private ReadOnly mIsPlusSign As Boolean

    ' 値配列
    Private ReadOnly mValues As Byte()

    ''' <summary>0値を取得します。</summary>
    ''' <returns>0値。</returns>
    Public Shared ReadOnly Property Zero As VariableInteger = New VariableInteger(0)

    ''' <summary>ビットサイズを取得します。</summary>
    ''' <returns>最上位のビット桁数。</returns>
    Public ReadOnly Property BitSize As Integer
        Get
            Return GetBitSize(Me.mValues)
        End Get
    End Property

    Public ReadOnly Property IsZero As Boolean
        Get
            Return (Me.mValues.Length = 1 AndAlso Me.mValues(0) = 0)
        End Get
    End Property

    Public ReadOnly Property IsPlusSign As Boolean
        Get
            Return Me.mIsPlusSign
        End Get
    End Property

    Public ReadOnly Property IsMinusSign As Boolean
        Get
            Return Not Me.mIsPlusSign
        End Get
    End Property

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="value">格納する値。</param>
    Public Sub New(value As Integer)
        Dim int = value
        If value >= 0 Then
            Me.mIsPlusSign = True
        Else
            Me.mIsPlusSign = False
            int = -value
        End If
        Dim vals As New List(Of Byte)()
        Do While int <> 0
            vals.Add(CByte(int And &HFF))
            int >>= 8
        Loop
        Me.mValues = If(vals.Count > 0, vals.ToArray(), New Byte() {0})
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="value">格納する値。</param>
    Public Sub New(value As Long)
        Dim int = value
        If value >= 0 Then
            Me.mIsPlusSign = True
        Else
            Me.mIsPlusSign = False
            int = -value
        End If
        Dim vals As New List(Of Byte)()
        Do While int <> 0
            vals.Add(CByte(int And &HFF))
            int >>= 8
        Loop
        Me.mValues = If(vals.Count > 0, vals.ToArray(), New Byte() {0})
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="value">格納する値。</param>
    Public Sub New(value As ULong)
        Dim vals As New List(Of Byte)()
        Do While value <> 0
            vals.Add(CByte(value And &HFFUL))
            value >>= 8
        Loop
        Me.mIsPlusSign = True
        Me.mValues = If(vals.Count > 0, vals.ToArray(), New Byte() {0})
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="isPlusSign">符号。</param>
    ''' <param name="values">値リスト。</param>
    Private Sub New(isPlusSign As Boolean, values As Byte())
        Me.mIsPlusSign = isPlusSign
        Dim vals As New List(Of Byte)()
        For i As Integer = values.Length - 1 To 0 Step -1
            If values(i) <> 0 Then
                For j As Integer = 0 To i
                    vals.Add(values(j))
                Next
                Exit For
            End If
        Next
        Me.mValues = If(vals.Count > 0, vals.ToArray(), New Byte() {0})
    End Sub

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列表現。</returns>
    Public Overrides Function ToString() As String
        If Not Me.IsZero Then
            Dim ans As New List(Of Byte)()
            Dim ptr = New VariableInteger(Me.mIsPlusSign, Me.mValues)
            Dim den = New VariableInteger(10)
            Do
                Dim pair = ptr.DivisionAndRemainder(den)
                ans.Add(pair.Remainder.mValues(0))
                ptr = pair.Quotient
            Loop While Not ptr.IsZero

            Dim res As New StringBuilder()
            If Not Me.mIsPlusSign Then
                res.Append("-")
            End If
            ans.Reverse()
            For Each c In ans
                res.Append($"{c}")
            Next
            Return res.ToString()
        Else
            Return "0"
        End If
    End Function

    Public Function Abs() As VariableInteger
        Return New VariableInteger(True, Me.mValues)
    End Function

#Region "加算"

    ''' <summary>加算を行います。</summary>
    ''' <param name="selfValues">被加算値。</param>
    ''' <param name="otherValues">加算値。</param>
    Private Shared Sub Addition(selfValues As Byte(), otherValues As Byte())
        Dim ovflow As Integer = 0
        For i As Integer = 0 To Math.Max(selfValues.Length, otherValues.Length) - 1
            Dim lv As Integer = If(i < selfValues.Length, selfValues(i), 0)
            Dim rv As Integer = If(i < otherValues.Length, otherValues(i), 0)

            Dim v = lv + rv + ovflow

            ovflow = If(v > Byte.MaxValue, 1, 0)
            selfValues(i) = CByte(v And &HFF)
        Next
    End Sub

    ''' <summary>数値を加算します。</summary>
    ''' <param name="other">加算する値。</param>
    ''' <returns>加算結果。</returns>
    Public Function Addition(other As VariableInteger) As VariableInteger
        Dim tmp = New TempValue(Me.mValues, Math.Max(Me.mValues.Length, other.mValues.Length) + 1)
        If Me.mIsPlusSign Xor other.mIsPlusSign Then
            Dim minus = Subtraction(tmp.mValue, other.mValues)
            Return New VariableInteger(minus Xor Me.mIsPlusSign, tmp.mValue)
        Else
            Addition(tmp.mValue, other.mValues)
            Return New VariableInteger(Me.mIsPlusSign, tmp.mValue)
        End If
    End Function

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As VariableInteger, rt As VariableInteger) As VariableInteger
        Return lf.Addition(rt)
    End Operator

#End Region

#Region "引算"

    ''' <summary>符号を反転します。</summary>
    ''' <param name="self">反転する数値。</param>
    ''' <returns>反転結果。</returns>
    Public Shared Operator -(self As VariableInteger) As VariableInteger
        Return New VariableInteger(Not self.mIsPlusSign, self.mValues)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="selfValues">被引算値。</param>
    ''' <param name="otherValues">引算値。</param>
    ''' <returns>マイナス値になったら真。</returns>
    Private Shared Function Subtraction(selfValues As Byte(),
                                        otherValues As Byte()) As Boolean
        Dim ovflow As Byte = 0
        For i As Integer = 0 To Math.Max(selfValues.Length, otherValues.Length) - 1
            Dim lv As Integer = If(i < selfValues.Length, selfValues(i), 0)
            Dim rv As Integer = If(i < otherValues.Length, otherValues(i), 0)

            If lv - ovflow >= rv Then
                selfValues(i) = CByte(lv - ovflow - rv)
                ovflow = 0
            Else
                Dim ans = lv - ovflow + &H100
                selfValues(i) = CByte(ans - rv)
                ovflow = 1
            End If
        Next
        If ovflow > 0 Then
            For i As Integer = 0 To selfValues.Length - 1
                selfValues(i) = Not selfValues(i)
            Next
            Addition(selfValues, New Byte() {1})
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>数値を引算します。</summary>
    ''' <param name="other">引算する値。</param>
    ''' <returns>引算結果。</returns>
    Public Function Subtraction(other As VariableInteger) As VariableInteger
        Dim tmp = New TempValue(Me.mValues, Math.Max(Me.mValues.Length, other.mValues.Length) + 1)
        If Me.mIsPlusSign Xor other.mIsPlusSign Then
            Addition(tmp.mValue, other.mValues)
            Return New VariableInteger(Me.mIsPlusSign, tmp.mValue)
        Else
            Dim minus = Subtraction(tmp.mValue, other.mValues)
            Return New VariableInteger(minus Xor Me.mIsPlusSign, tmp.mValue)
        End If
    End Function

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As VariableInteger, rt As VariableInteger) As VariableInteger
        Return lf.Subtraction(rt)
    End Operator

#End Region

#Region "乗算"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="multiplier"></param>
    ''' <returns></returns>
    Public Function Multiplication(multiplier As VariableInteger) As VariableInteger
        Dim ls = If(Me.mIsPlusSign, 1, -1)
        Dim rs = If(multiplier.mIsPlusSign, 1, -1)

        Dim ans = New TempValue(New Byte() {0}, Me.mValues.Length + multiplier.mValues.Length + 1)

        Dim num = New TempValue(Me.mValues, Me.mValues.Length + multiplier.mValues.Length + 1)
        Dim multi = New TempValue(multiplier.mValues, multiplier.mValues.Length)

        Dim loopCnt = multiplier.BitSize
        For i As Integer = 0 To loopCnt
            Dim ad = RightShift(multi.mValue)
            If ad <> 0 Then
                Addition(ans.mValue, num.mValue)
            End If
            LeftShift(num.mValue)
        Next

        Return New VariableInteger(If(ls * rs > 0, True, False), ans.mValue)
    End Function

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator *(lf As VariableInteger, rt As VariableInteger) As VariableInteger
        Return lf.Multiplication(rt)
    End Operator

#End Region

#Region "除算"

    ''' <summary>数値を除算します。</summary>
    ''' <param name="divisor">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function Division(divisor As VariableInteger) As VariableInteger
        Dim res = Me.DivisionAndRemainder(divisor)
        Return res.Quotient
    End Function

    ''' <summary>数値を除算し、余りを取得します。</summary>
    ''' <param name="divisor">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function Modulo(divisor As VariableInteger) As VariableInteger
        Dim res = Me.DivisionAndRemainder(divisor)
        Return res.Remainder
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="divisor"></param>
    ''' <returns></returns>
    Public Function DivisionAndRemainder(divisor As VariableInteger) As DivisionAnswer
        Dim ls = If(Me.mIsPlusSign, 1, -1)
        Dim rs = If(divisor.mIsPlusSign, 1, -1)

        Dim lbitsz = GetBitSize(Me.mValues)
        Dim rbitsz = GetBitSize(divisor.mValues)

        If lbitsz >= rbitsz Then
            Dim num = New TempValue(Me.mValues, Me.mValues.Length)

            Dim div = New TempValue(divisor.mValues, Me.mValues.Length)
            LeftShift(div.mValue, lbitsz - rbitsz)

            Dim quotient As New TempValue(Me.mValues.Length)
            For i As Integer = lbitsz - rbitsz To 0 Step -1
                If num.CompareTo(div) >= 0 Then
                    SetBit(quotient.mValue, i)
                    Subtraction(num.mValue, div.mValue)
                End If
                RightShift(div.mValue)
            Next
            Return New DivisionAnswer(
                New VariableInteger(If(ls * rs > 0, True, False), quotient.mValue),
                New VariableInteger(Me.mIsPlusSign, num.mValue)
            )
        Else
            Return New DivisionAnswer(VariableInteger.Zero, Me)
        End If
    End Function

    ''' <summary>最上位のビット桁数を取得します。</summary>
    ''' <param name="values">値リスト。</param>
    ''' <returns>ビット桁数。</returns>
    Private Shared Function GetBitSize(values As Byte()) As Integer
        For i As Integer = values.Length - 1 To 0 Step -1
            Dim mask = &H80
            For j As Integer = 7 To 0 Step -1
                If (values(i) And mask) <> 0 Then
                    Return i * 8 + j
                End If
                mask >>= 1
            Next
        Next
        Return 0
    End Function

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As VariableInteger, rt As VariableInteger) As VariableInteger
        Return lf.Division(rt)
    End Operator

    ''' <summary>剰余算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>剰余算結果。</returns>
    Public Shared Operator Mod(lf As VariableInteger, rt As VariableInteger) As VariableInteger
        Return lf.Modulo(rt)
    End Operator

#End Region

#Region "シフト演算"

    ''' <summary>ビットを指定数左にシフトする。</summary>
    ''' <param name="values">値配列。</param>
    ''' <param name="nest">シフトする数。</param>
    Public Shared Sub LeftShift(values As Byte(), Optional nest As Integer = 1)
        Dim ovflow As Byte = 0
        For j As Integer = 0 To nest - 1
            For i As Integer = 0 To values.Length - 1
                Dim flag = CByte(If(values(i) > 127, 1, 0))
                values(i) = values(i) << 1 Or ovflow
                ovflow = flag
            Next
        Next
    End Sub

    ''' <summary>ビットを指定数右にシフトする。</summary>
    ''' <param name="values">値配列。</param>
    ''' <param name="nest">シフトする数。</param>
    ''' <returns>最下位ビット。</returns>
    Public Shared Function RightShift(values As Byte(), nest As Integer) As Byte
        Dim res = CByte(values(0) And 1)
        For j As Integer = 0 To nest - 1
            RightShift(values)
        Next
        Return res
    End Function

    ''' <summary>ビットを1ビット右にシフトする。</summary>
    ''' <param name="values">値配列。</param>
    ''' <returns>最下位ビット。</returns>
    Public Shared Function RightShift(values As Byte()) As Byte
        Dim res = CByte(values(0) And 1)
        Dim ovflow As Byte = 0
        For i As Integer = values.Length - 1 To 0 Step -1
            Dim flag = CByte(If((values(i) And 1) <> 0, &H80, 0))
            values(i) = values(i) >> 1 Or ovflow
            ovflow = flag
        Next
        Return res
    End Function

    ''' <summary>指定の桁位置にビットをセットする。</summary>
    ''' <param name="values">数値配列。</param>
    ''' <param name="figre">指定の桁。</param>
    Public Shared Sub SetBit(values As Byte(), figre As Integer)
        Dim bcnt = figre \ 8
        Dim bflg = 1 << (figre - 8 * bcnt)
        values(bcnt) = CByte(values(bcnt) Or bflg)
    End Sub

#End Region

#Region "比較"

    ''' <summary>等しければ真を返します。</summary>
    ''' <param name="obj">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is FractionOld Then
            Return Me.Equals(CType(obj, FractionOld))
        Else
            Return False
        End If
    End Function

    ''' <summary>等しければ真を返します。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Overloads Function Equals(other As VariableInteger) As Boolean Implements IEquatable(Of VariableInteger).Equals
        For i As Integer = Math.Max(Me.mValues.Length, other.mValues.Length) - 1 To 0 Step -1
            Dim lv = If(i < Me.mValues.Length, Me.mValues(i), 0)
            Dim rv = If(i < other.mValues.Length, other.mValues(i), 0)
            If lv <> rv Then
                Return False
            End If
        Next
        Return True
    End Function

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As VariableInteger, rt As VariableInteger) As Boolean
        Return lf.Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As VariableInteger, rt As VariableInteger) As Boolean
        Return Not lf.Equals(rt)
    End Operator

    ''' <summary>比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Function CompareTo(other As VariableInteger) As Integer Implements IComparable(Of VariableInteger).CompareTo
        If Me.mIsPlusSign = other.mIsPlusSign Then
            For i As Integer = Math.Max(Me.mValues.Length, other.mValues.Length) - 1 To 0 Step -1
                Dim lv = If(i < Me.mValues.Length, Me.mValues(i), 0)
                Dim rv = If(i < other.mValues.Length, other.mValues(i), 0)

                If lv > rv Then
                    Return If(Me.mIsPlusSign, 1, -1)
                ElseIf lv < rv Then
                    Return If(Me.mIsPlusSign, -1, 1)
                End If
            Next
            Return 0
        Else
            Return If(Me.mIsPlusSign, 1, -1)
        End If
    End Function

#End Region

    Private Structure TempValue
        Implements IComparable(Of TempValue)

        Public mValue As Byte()

        Public Sub New(figre As Integer)
            Me.mValue = New Byte(figre - 1) {}
        End Sub

        Public Sub New(values As Byte(), figre As Integer)
            Me.mValue = New Byte(figre - 1) {}
            For i As Integer = 0 To values.Length - 1
                Me.mValue(i) = values(i)
            Next
        End Sub

        Public Function CompareTo(other As TempValue) As Integer Implements IComparable(Of TempValue).CompareTo
            For i As Integer = Me.mValue.Length - 1 To 0 Step -1
                If Me.mValue(i) > other.mValue(i) Then
                    Return 1
                ElseIf Me.mValue(i) < other.mValue(i) Then
                    Return -1
                End If
            Next
            Return 0
        End Function

    End Structure

    ''' <summary>除算結果。</summary>
    Public Structure DivisionAnswer

        ''' <summary>商。</summary>
        Public ReadOnly Property Quotient As VariableInteger

        ''' <summary>余。</summary>
        Public ReadOnly Property Remainder As VariableInteger

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="quot">商。</param>
        ''' <param name="remd">余。</param>
        Public Sub New(quot As VariableInteger, remd As VariableInteger)
            Me.Quotient = quot
            Me.Remainder = remd
        End Sub

    End Structure

End Structure
