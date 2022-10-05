Option Strict On
Option Explicit On
Imports System.Text

''' <summary>可変長変数。</summary>
Public NotInheritable Class VariableInteger
    Implements IComparable(Of VariableInteger), IEquatable(Of VariableInteger), ICloneable

    ' 符号
    Private ReadOnly mIsPlusSign As Boolean

    ' 値配列
    Private ReadOnly mValue As ByteArray

    ''' <summary>0値を取得します。</summary>
    ''' <returns>0値。</returns>
    Public Shared ReadOnly Property Zero As VariableInteger = New VariableInteger(0)

    ''' <summary>ビットサイズを取得します。</summary>
    ''' <returns>最上位のビット桁数。</returns>
    Public ReadOnly Property BitSize As Integer
        Get
            Return Me.mValue.BitSize
        End Get
    End Property

    ''' <summary>配列が0ならば真を返す。</summary>
    ''' <returns>0ならば真。</returns>
    Public ReadOnly Property IsZero As Boolean
        Get
            Return Me.mValue.IsZero
        End Get
    End Property

    ''' <summary>変数が正の値ならば真を返す。</summary>
    ''' <returns>正の値ならば真。</returns>
    Public ReadOnly Property IsPlusSign As Boolean
        Get
            Return Me.mIsPlusSign
        End Get
    End Property

    ''' <summary>変数が負の値ならば真を返す。</summary>
    ''' <returns>負の値ならば真。</returns>
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
        Me.mValue = New ByteArray(vals)
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
        Me.mValue = New ByteArray(vals)
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
        Me.mValue = New ByteArray(vals)
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="isPlusSign">符号。</param>
    ''' <param name="other">コピー元。</param>
    Private Sub New(isPlusSign As Boolean, other As VariableInteger)
        Me.mIsPlusSign = isPlusSign
        Me.mValue = other.mValue.TrimCopy()
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="isPlusSign">符号。</param>
    ''' <param name="other">コピー元。</param>
    Private Sub New(isPlusSign As Boolean, other As ByteArray)
        Me.mIsPlusSign = isPlusSign
        Me.mValue = other.TrimCopy()
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="other">コピー元。</param>
    Private Sub New(other As VariableInteger)
        Me.mIsPlusSign = other.mIsPlusSign
        Me.mValue = other.mValue.TrimCopy()
    End Sub

    '''' <summary>文字列表現を取得します。</summary>
    '''' <returns>文字列表現。</returns>
    'Public Overrides Function ToString() As String
    '    If Not Me.IsZero Then
    '        Dim ans As New List(Of Byte)()
    '        Dim ptr = New VariableInteger(Me)
    '        Dim den = New VariableInteger(10)
    '        Do
    '            Dim pair = ptr.DivisionAndRemainder(den)
    '            ans.Add(pair.Remainder.mValue(0))
    '            ptr = pair.Quotient
    '        Loop While Not ptr.IsZero

    '        Dim res As New StringBuilder()
    '        If Not Me.mIsPlusSign Then
    '            res.Append("-")
    '        End If
    '        ans.Reverse()
    '        For Each c In ans
    '            res.Append($"{c}")
    '        Next
    '        Return res.ToString()
    '    Else
    '        Return "0"
    '    End If
    'End Function

    ''' <summary>絶対値を取得します。</summary>
    ''' <returns>正の可変長変数。</returns>
    Public Function Abs() As VariableInteger
        Return New VariableInteger(True, Me)
    End Function

    ''' <summary>インスタンスをコピーします。</summary>
    ''' <returns>可変長変数。</returns>
    Public Function Clone() As Object Implements ICloneable.Clone
        Return New VariableInteger(Me)
    End Function

    ''' <summary>数値にキャストします。</summary>
    ''' <param name="self">可変長変数。</param>
    ''' <returns>数値。</returns>
    Public Shared Narrowing Operator CType(ByVal self As VariableInteger) As Long
        Return If(self.mIsPlusSign, CLng(self.mValue), -CLng(self.mValue))
    End Operator

#Region "加算"

    ''' <summary>数値を加算します。</summary>
    ''' <param name="other">加算する値。</param>
    ''' <returns>加算結果。</returns>
    Public Function Addition(other As VariableInteger) As VariableInteger
        Dim tmp = New ByteArray(Me.mValue, Math.Max(Me.mValue.Length, other.mValue.Length) + 1)
        If Me.mIsPlusSign Xor other.mIsPlusSign Then
            Dim minus = tmp.Subtraction(other.mValue)
            Return New VariableInteger(minus Xor Me.mIsPlusSign, tmp)
        Else
            tmp.Addition(other.mValue)
            Return New VariableInteger(Me.mIsPlusSign, tmp)
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
        Return New VariableInteger(Not self.mIsPlusSign, self)
    End Operator

    ''' <summary>数値を引算します。</summary>
    ''' <param name="other">引算する値。</param>
    ''' <returns>引算結果。</returns>
    Public Function Subtraction(other As VariableInteger) As VariableInteger
        Dim tmp = New ByteArray(Me.mValue, Math.Max(Me.mValue.Length, other.mValue.Length) + 1)
        If Me.mIsPlusSign Xor other.mIsPlusSign Then
            tmp.Addition(other.mValue)
            Return New VariableInteger(Me.mIsPlusSign, tmp)
        Else
            Dim minus = tmp.Subtraction(other.mValue)
            Return New VariableInteger(minus Xor Me.mIsPlusSign, tmp)
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

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="multiplier">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Function Multiplication(multiplier As VariableInteger) As VariableInteger
        ' 結果格納領域を生成
        Dim ans = New ByteArray(Me.mValue.Length + multiplier.mValue.Length + 1)

        ' 被乗数、乗数の領域を作成
        Dim num = New ByteArray(Me.mValue, Me.mValue.Length + multiplier.mValue.Length + 1)
        Dim multi = multiplier.mValue.TrimCopy()

        ' 二進のビットの立っている桁で加算を繰り返す
        Dim loopCnt = multiplier.BitSize
        For i As Integer = 0 To loopCnt
            Dim ad = multi.RightShift
            If ad <> 0 Then
                ans.Addition(num)
            End If
            num.LeftShift()
        Next

        Return New VariableInteger(Not (Me.mIsPlusSign Xor multiplier.mIsPlusSign), ans)
    End Function

    ''' <summary>乗算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>乗算結果。</returns>
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

    ''' <summary>数値を除算します。</summary>
    ''' <param name="divisor">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function DivisionAndRemainder(divisor As VariableInteger) As DivisionAnswer
        Dim lbitsz = Me.mValue.BitSize
        Dim rbitsz = divisor.mValue.BitSize
        If lbitsz >= rbitsz Then
            ' 結果格納領域を生成
            Dim quotient As New ByteArray(Me.mValue.Length)

            ' 被除数、除数の領域を作成
            Dim num = New ByteArray(Me.mValue, Me.mValue.Length)
            Dim div = New ByteArray(divisor.mValue, Me.mValue.Length)
            div.LeftShift(lbitsz - rbitsz)

            ' 上位の桁から除数を引き算を繰り返す
            For i As Integer = lbitsz - rbitsz To 0 Step -1
                If num.CompareTo(div) >= 0 Then
                    quotient.SetBit(i)
                    num.Subtraction(div)
                End If
                div.RightShift()
            Next
            Return New DivisionAnswer(
                New VariableInteger(Not (Me.mIsPlusSign Xor divisor.mIsPlusSign), quotient),
                New VariableInteger(Me.mIsPlusSign, num)
            )
        Else
            Return New DivisionAnswer(VariableInteger.Zero, Me)
        End If
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
        If Me.mIsPlusSign = other.mIsPlusSign Then
            Return Me.mValue.Equals(other.mValue)
        Else
            Return False
        End If
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
            Dim res = Me.mValue.CompareTo(other.mValue)
            Return If(Me.mIsPlusSign, res, -res)
        Else
            Return If(Me.mIsPlusSign, 1, -1)
        End If
    End Function

#End Region

    ''' <summary>バイト配列情報。</summary>
    Private NotInheritable Class ByteArray
        Implements IComparable(Of ByteArray)

        ' バイト値
        Private ReadOnly mValue As Byte()

        ''' <summary>最上位のビット桁数を取得します。</summary>
        ''' <returns>ビット桁数。</returns>
        Public ReadOnly Property BitSize As Integer
            Get
                For i As Integer = Me.mValue.Length - 1 To 0 Step -1
                    Dim mask = &H80
                    For j As Integer = 7 To 0 Step -1
                        If (Me.mValue(i) And mask) <> 0 Then
                            Return i * 8 + j
                        End If
                        mask >>= 1
                    Next
                Next
                Return 0
            End Get
        End Property

        ''' <summary>配列が0ならば真を返す。</summary>
        ''' <returns>0ならば真。</returns>
        Public ReadOnly Property IsZero As Boolean
            Get
                Return (Me.mValue.Length = 1 AndAlso Me.mValue(0) = 0)
            End Get
        End Property

        ''' <summary>バイト配列長を取得します。</summary>
        ''' <returns>バイト配列長。</returns>
        Public ReadOnly Property Length As Integer
            Get
                Return Me.mValue.Length
            End Get
        End Property

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="length">配列の長さ。</param>
        Public Sub New(length As Integer)
            Me.mValue = New Byte(length - 1) {}
        End Sub

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="source">元の配列。</param>
        ''' <param name="length">配列の長さ。</param>
        Public Sub New(source As ByteArray, length As Integer)
            Me.mValue = New Byte(length - 1) {}
            Array.Copy(source.mValue, Me.mValue, source.mValue.Length)
        End Sub

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="source">元の値リスト。</param>
        Public Sub New(source As List(Of Byte))
            Me.mValue = If(source.Count > 0, source.ToArray(), New Byte() {0})
        End Sub

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="source">元の値リスト。</param>
        Private Sub New(source As Byte())
            Me.mValue = If(source.Length > 0, source, New Byte() {0})
        End Sub

        Public Function TrimCopy() As ByteArray
            Dim vals As New List(Of Byte)()
            For i As Integer = Me.mValue.Length - 1 To 0 Step -1
                If Me.mValue(i) <> 0 Then
                    For j As Integer = 0 To i
                        vals.Add(Me.mValue(j))
                    Next
                    Exit For
                End If
            Next
            Return New ByteArray(vals)
        End Function

        ''' <summary>加算を行います。</summary>
        ''' <param name="other">加算値。</param>
        Public Sub Addition(other As ByteArray)
            Dim ovflow As Integer = 0
            For i As Integer = 0 To Math.Max(Me.mValue.Length, other.mValue.Length) - 1
                Dim lv As Integer = If(i < Me.mValue.Length, Me.mValue(i), 0)
                Dim rv As Integer = If(i < other.mValue.Length, other.mValue(i), 0)

                Dim v = lv + rv + ovflow

                ovflow = If(v > Byte.MaxValue, 1, 0)
                Me.mValue(i) = CByte(v And &HFF)
            Next
        End Sub

        ''' <summary>引算を行います。</summary>
        ''' <param name="other">引算値。</param>
        ''' <returns>マイナス値になったら真。</returns>
        Public Function Subtraction(other As ByteArray) As Boolean
            Dim ovflow As Byte = 0
            For i As Integer = 0 To Math.Max(Me.mValue.Length, other.mValue.Length) - 1
                Dim lv As Integer = If(i < Me.mValue.Length, Me.mValue(i), 0)
                Dim rv As Integer = If(i < other.mValue.Length, other.mValue(i), 0)

                If lv - ovflow >= rv Then
                    Me.mValue(i) = CByte(lv - ovflow - rv)
                    ovflow = 0
                Else
                    Dim ans = lv - ovflow + &H100
                    Me.mValue(i) = CByte(ans - rv)
                    ovflow = 1
                End If
            Next

            ' マイナスならばビット反転 + 1
            If ovflow > 0 Then
                For i As Integer = 0 To Me.mValue.Length - 1
                    Me.mValue(i) = Not Me.mValue(i)
                Next
                Me.Addition(New ByteArray(New Byte() {1}))
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>指定の桁位置にビットをセットする。</summary>
        ''' <param name="figre">指定の桁。</param>
        Public Sub SetBit(figre As Integer)
            Dim bcnt = figre \ 8
            Dim bflg = 1 << (figre - 8 * bcnt)
            Me.mValue(bcnt) = CByte(Me.mValue(bcnt) Or bflg)
        End Sub

        ''' <summary>ビットを指定数左にシフトする。</summary>
        ''' <param name="nest">シフトする数。</param>
        Public Sub LeftShift(Optional nest As Integer = 1)
            Dim ovflow As Byte = 0
            For j As Integer = 0 To nest - 1
                For i As Integer = 0 To Me.mValue.Length - 1
                    Dim flag = CByte(If(Me.mValue(i) > 127, 1, 0))
                    Me.mValue(i) = Me.mValue(i) << 1 Or ovflow
                    ovflow = flag
                Next
            Next
        End Sub

        ''' <summary>ビットを1ビット右にシフトする。</summary>
        ''' <returns>最下位ビット。</returns>
        Public Function RightShift() As Byte
            Dim res = CByte(Me.mValue(0) And 1)
            Dim ovflow As Byte = 0
            For i As Integer = Me.mValue.Length - 1 To 0 Step -1
                Dim flag = CByte(If((Me.mValue(i) And 1) <> 0, &H80, 0))
                Me.mValue(i) = Me.mValue(i) >> 1 Or ovflow
                ovflow = flag
            Next
            Return res
        End Function

        ''' <summary>値が等しければ真を返す。</summary>
        ''' <param name="obj">比較対象。</param>
        ''' <returns>等しければ真。</returns>
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, ByteArray)
            If other IsNot Nothing Then
                For i As Integer = Math.Max(Me.mValue.Length, other.mValue.Length) - 1 To 0 Step -1
                    Dim lv = If(i < Me.mValue.Length, Me.mValue(i), 0)
                    Dim rv = If(i < other.mValue.Length, other.mValue(i), 0)
                    If lv <> rv Then
                        Return False
                    End If
                Next
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>比較を行います。</summary>
        ''' <param name="other">比較対象。</param>
        ''' <returns>比較結果。</returns>
        Public Function CompareTo(other As ByteArray) As Integer Implements IComparable(Of ByteArray).CompareTo
            For i As Integer = Math.Max(Me.mValue.Length, other.mValue.Length) - 1 To 0 Step -1
                Dim lv = If(i < Me.mValue.Length, Me.mValue(i), 0)
                Dim rv = If(i < other.mValue.Length, other.mValue(i), 0)
                If lv > rv Then
                    Return 1
                ElseIf lv < rv Then
                    Return -1
                End If
            Next
            Return 0
        End Function

        ''' <summary>バイト配列の値を数値に変換します。</summary>
        ''' <param name="self">バイト配列。</param>
        ''' <returns>数値。</returns>
        Public Shared Narrowing Operator CType(ByVal self As ByteArray) As Long
            If self.mValue.Length <= 16 Then
                Dim res As ULong = 0
                For i As Integer = self.mValue.Length - 1 To 0 Step -1
                    res = res << 8 Or self.mValue(i)
                Next
                If res <= Long.MaxValue Then
                    Return CLng(res)
                End If
            End If
            Throw New OverflowException("Longに変換したらオーバーフローします")
        End Operator

    End Class

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

End Class
