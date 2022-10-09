Option Strict On
Option Explicit On

Imports System.Runtime.Serialization
Imports System.Text

''' <summary>可変長整数。</summary>
<Serializable()>
Public NotInheritable Class VarInteger
    Inherits MarshalByRefObject
    Implements IComparable(Of VarInteger), IEquatable(Of VarInteger), ICloneable, ISerializable

    ' 符号
    Private ReadOnly mIsPlusSign As Boolean

    ' 値配列
    Private ReadOnly mValue As ByteArray

    ''' <summary>0値を取得します。</summary>
    ''' <returns>0値。</returns>
    Public Shared ReadOnly Property Zero As VarInteger = New VarInteger(0)

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

    ''' <summary>生値のバイト配列を取得します。</summary>
    ''' <returns>生値のバイト。</returns>
    Friend ReadOnly Property Raw As Byte()
        Get
            Return Me.mValue.Raw
        End Get
    End Property

    ''' <summary>デフォルトコンストラクタ。</summary>
    Public Sub New()
        Me.mIsPlusSign = True
        Me.mValue = New ByteArray(New Byte() {0})
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="value">格納する値。</param>
    Public Sub New(value As Integer)
        Dim vals = BitConverter.GetBytes(value)
        Dim tmp As ByteArray
        If value >= 0 Then
            Me.mIsPlusSign = True
            tmp = New ByteArray(vals)
        Else
            For i As Integer = 0 To vals.Length - 1
                vals(i) = CByte(vals(i) Xor &HFF)
            Next
            Me.mIsPlusSign = False
            tmp = New ByteArray(vals)
            tmp.Addition(New ByteArray(New Byte() {1}))
        End If
        Me.mValue = tmp.TrimCopy()
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="value">格納する値。</param>
    Public Sub New(value As Long)
        Dim tmp As ByteArray
        Dim vals = BitConverter.GetBytes(value)
        If value >= 0 Then
            Me.mIsPlusSign = True
            tmp = New ByteArray(vals)
        Else
            For i As Integer = 0 To vals.Length - 1
                vals(i) = CByte(vals(i) Xor &HFF)
            Next
            Me.mIsPlusSign = False
            tmp = New ByteArray(vals)
            tmp.Addition(New ByteArray(New Byte() {1}))
        End If
        Me.mValue = tmp.TrimCopy()
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
    Private Sub New(isPlusSign As Boolean, other As ByteArray)
        Me.mIsPlusSign = isPlusSign
        Me.mValue = other.TrimCopy()
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="isPlusSign">符号。</param>
    ''' <param name="other">コピー元。</param>
    Friend Sub New(isPlusSign As Boolean, other As Byte())
        Me.New(isPlusSign, New ByteArray(other))
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="other">コピー元。</param>
    Private Sub New(other As VarInteger)
        Me.mIsPlusSign = other.mIsPlusSign
        Me.mValue = other.mValue.TrimCopy()
    End Sub

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="info">インフォメーション。</param>
    ''' <param name="context">コンテキスト。</param>
    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        Me.mIsPlusSign = info.GetBoolean("IsPlusSign")
        Me.mValue = New ByteArray(CType(info.GetValue("Value", GetType(Byte())), Byte()))
    End Sub

    ''' <summary>可変長整数を取得する。</summary>
    ''' <param name="value">格納する値。</param>
    Public Shared Function Create(value As Integer) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数を取得する。</summary>
    ''' <param name="value">格納する値。</param>
    Public Shared Function Create(value As Long) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>可変長整数を取得する。</summary>
    ''' <param name="value">格納する値。</param>
    Public Shared Function Create(value As ULong) As VarInteger
        Return New VarInteger(value)
    End Function

    ''' <summary>シリアライズオブジェクトを取得する。</summary>
    ''' <param name="info">インフォメーション。</param>
    ''' <param name="context">コンテキスト。</param>
    Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
        info.AddValue("IsPlusSign", Me.mIsPlusSign)
        info.AddValue("Value", Me.mValue.Raw)
    End Sub

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列表現。</returns>
    Public Overrides Function ToString() As String
        If Not Me.IsZero Then
            Dim ans As New List(Of Byte)()
            Dim ptr = New VarInteger(True, Me.mValue)
            Dim den = New VarInteger(10)
            Do
                Dim pair = ptr.DivisionAndRemainder(den)
                ans.Add(CByte(CSByte(pair.Remainder)))
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

    ''' <summary>絶対値を取得します。</summary>
    ''' <returns>正の可変長整数。</returns>
    Public Function Abs() As VarInteger
        Return New VarInteger(True, Me.mValue)
    End Function

    ''' <summary>インスタンスをコピーします。</summary>
    ''' <returns>可変長整数。</returns>
    Public Function Clone() As Object Implements ICloneable.Clone
        Return New VarInteger(Me)
    End Function

    ''' <summary>ビットをビット左にシフトする。</summary>
    ''' <param name="nest"></param>
    Public Function LeftShift(Optional nest As Integer = 1) As VarInteger
        Dim cnt = nest \ 8 + 1
        Dim val = New ByteArray(Me.mValue, Me.mValue.Length + cnt)
        val.LeftShift(nest)
        Return New VarInteger(Me.mIsPlusSign, val)
    End Function

    ''' <summary>ビットを1ビット右にシフトする。</summary>
    Public Sub RightShift()
        Me.mValue.RightShift()
    End Sub

    ''' <summary>数値にキャストします。</summary>
    ''' <param name="self">可変長整数。</param>
    ''' <returns>数値。</returns>
    Public Shared Narrowing Operator CType(ByVal self As VarInteger) As Long
        Return self.mValue.GetLong(self.mIsPlusSign)
    End Operator

    ''' <summary>数値にキャストします。</summary>
    ''' <param name="self">可変長整数。</param>
    ''' <returns>数値。</returns>
    Public Shared Narrowing Operator CType(ByVal self As VarInteger) As Integer
        Return self.mValue.GetInteger(self.mIsPlusSign)
    End Operator

#Region "加算"

    ''' <summary>数値を加算します。</summary>
    ''' <param name="other">加算する値。</param>
    ''' <returns>加算結果。</returns>
    Public Function Addition(other As VarInteger) As VarInteger
        Dim tmp = New ByteArray(Me.mValue, Math.Max(Me.mValue.Length, other.mValue.Length) + 1)
        If Me.mIsPlusSign Xor other.mIsPlusSign Then
            Dim minus = tmp.Subtraction(other.mValue)
            Return New VarInteger(minus Xor Me.mIsPlusSign, tmp)
        Else
            tmp.Addition(other.mValue)
            Return New VarInteger(Me.mIsPlusSign, tmp)
        End If
    End Function

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As VarInteger, rt As VarInteger) As VarInteger
        Return lf.Addition(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As VarInteger, rt As Integer) As VarInteger
        Return lf.Addition(New VarInteger(rt))
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As VarInteger, rt As Long) As VarInteger
        Return lf.Addition(New VarInteger(rt))
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Integer, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Addition(rt)
    End Operator

    ''' <summary>加算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>加算結果。</returns>
    Public Shared Operator +(lf As Long, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Addition(rt)
    End Operator

#End Region

#Region "引算"

    ''' <summary>符号を反転します。</summary>
    ''' <param name="self">反転する数値。</param>
    ''' <returns>反転結果。</returns>
    Public Shared Operator -(self As VarInteger) As VarInteger
        Return New VarInteger(Not self.mIsPlusSign, self.mValue)
    End Operator

    ''' <summary>数値を引算します。</summary>
    ''' <param name="other">引算する値。</param>
    ''' <returns>引算結果。</returns>
    Public Function Subtraction(other As VarInteger) As VarInteger
        Dim tmp = New ByteArray(Me.mValue, Math.Max(Me.mValue.Length, other.mValue.Length) + 1)
        If Me.mIsPlusSign Xor other.mIsPlusSign Then
            tmp.Addition(other.mValue)
            Return New VarInteger(Me.mIsPlusSign, tmp)
        Else
            Dim minus = tmp.Subtraction(other.mValue)
            Return New VarInteger(minus Xor Me.mIsPlusSign, tmp)
        End If
    End Function

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As VarInteger, rt As VarInteger) As VarInteger
        Return lf.Subtraction(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As VarInteger, rt As Integer) As VarInteger
        Return lf.Subtraction(New VarInteger(rt))
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As VarInteger, rt As Long) As VarInteger
        Return lf.Subtraction(New VarInteger(rt))
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Integer, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Subtraction(rt)
    End Operator

    ''' <summary>引算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>引算結果。</returns>
    Public Shared Operator -(lf As Long, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Subtraction(rt)
    End Operator

#End Region

#Region "乗算"

    ''' <summary>数値を乗算します。</summary>
    ''' <param name="multiplier">乗数。</param>
    ''' <returns>乗算結果。</returns>
    Public Function Multiplication(multiplier As VarInteger) As VarInteger
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

        Return New VarInteger(Not (Me.mIsPlusSign Xor multiplier.mIsPlusSign), ans)
    End Function

    ''' <summary>乗算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As VarInteger, rt As VarInteger) As VarInteger
        Return lf.Multiplication(rt)
    End Operator

    ''' <summary>乗算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As VarInteger, rt As Integer) As VarInteger
        Return lf.Multiplication(New VarInteger(rt))
    End Operator

    ''' <summary>乗算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As VarInteger, rt As Long) As VarInteger
        Return lf.Multiplication(New VarInteger(rt))
    End Operator

    ''' <summary>乗算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Integer, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Multiplication(rt)
    End Operator

    ''' <summary>乗算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>乗算結果。</returns>
    Public Shared Operator *(lf As Long, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Multiplication(rt)
    End Operator

#End Region

#Region "除算"

    ''' <summary>数値を除算します。</summary>
    ''' <param name="divisor">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function Division(divisor As VarInteger) As VarInteger
        Dim res = Me.DivisionAndRemainder(divisor)
        Return res.Quotient
    End Function

    ''' <summary>数値を除算し、余りを取得します。</summary>
    ''' <param name="divisor">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function Modulo(divisor As VarInteger) As VarInteger
        Dim res = Me.DivisionAndRemainder(divisor)
        Return res.Remainder
    End Function

    ''' <summary>数値を除算します。</summary>
    ''' <param name="divisor">除数。</param>
    ''' <returns>除算結果。</returns>
    Public Function DivisionAndRemainder(divisor As VarInteger) As DivisionAnswer
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
                New VarInteger(Not (Me.mIsPlusSign Xor divisor.mIsPlusSign), quotient),
                New VarInteger(Me.mIsPlusSign, num)
            )
        Else
            Return New DivisionAnswer(VarInteger.Zero, Me)
        End If
    End Function

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As VarInteger, rt As VarInteger) As VarInteger
        Return lf.Division(rt)
    End Operator

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As VarInteger, rt As Integer) As VarInteger
        Return lf.Division(New VarInteger(rt))
    End Operator

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As VarInteger, rt As Long) As VarInteger
        Return lf.Division(New VarInteger(rt))
    End Operator

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Integer, rt As VarInteger) As VarInteger
        Return New VarInteger(lf).Division(rt)
    End Operator

    ''' <summary>除算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>除算結果。</returns>
    Public Shared Operator /(lf As Long, rt As VarInteger) As VarInteger
        Return New VarInteger(lf).Division(rt)
    End Operator

    ''' <summary>剰余算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>剰余算結果。</returns>
    Public Shared Operator Mod(lf As VarInteger, rt As VarInteger) As VarInteger
        Return lf.Modulo(rt)
    End Operator

    ''' <summary>剰余算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>剰余算結果。</returns>
    Public Shared Operator Mod(lf As VarInteger, rt As Integer) As VarInteger
        Return lf.Modulo(New VarInteger(rt))
    End Operator

    ''' <summary>剰余算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>剰余算結果。</returns>
    Public Shared Operator Mod(lf As VarInteger, rt As Long) As VarInteger
        Return lf.Modulo(New VarInteger(rt))
    End Operator

    ''' <summary>剰余算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>剰余算結果。</returns>
    Public Shared Operator Mod(lf As Integer, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Modulo(rt)
    End Operator

    ''' <summary>剰余算を行います。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>剰余算結果。</returns>
    Public Shared Operator Mod(lf As Long, rt As VarInteger) As VarInteger
        Return VarInteger.Create(lf).Modulo(rt)
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
    Public Overloads Function Equals(other As VarInteger) As Boolean Implements IEquatable(Of VarInteger).Equals
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
    Public Shared Operator =(lf As VarInteger, rt As VarInteger) As Boolean
        Return lf.Equals(rt)
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As VarInteger, rt As Integer) As Boolean
        Return lf.Equals(New VarInteger(rt))
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As VarInteger, rt As Long) As Boolean
        Return lf.Equals(New VarInteger(rt))
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Integer, rt As VarInteger) As Boolean
        Return VarInteger.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しいか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator =(lf As Long, rt As VarInteger) As Boolean
        Return VarInteger.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As VarInteger, rt As VarInteger) As Boolean
        Return Not lf.Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As VarInteger, rt As Integer) As Boolean
        Return Not lf.Equals(New VarInteger(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As VarInteger, rt As Long) As Boolean
        Return Not lf.Equals(New VarInteger(rt))
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Integer, rt As VarInteger) As Boolean
        Return Not VarInteger.Create(lf).Equals(rt)
    End Operator

    ''' <summary>等しくないか比較します。</summary>
    ''' <param name="lf">左辺値。</param>
    ''' <param name="rt">右辺値。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <>(lf As Long, rt As VarInteger) As Boolean
        Return Not VarInteger.Create(lf).Equals(rt)
    End Operator

    ''' <summary>比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Function CompareTo(other As VarInteger) As Integer Implements IComparable(Of VarInteger).CompareTo
        If Me.mIsPlusSign = other.mIsPlusSign Then
            Dim res = Me.mValue.CompareTo(other.mValue)
            Return If(Me.mIsPlusSign, res, -res)
        Else
            Return If(Me.mIsPlusSign, 1, -1)
        End If
    End Function

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As VarInteger, rt As VarInteger) As Boolean
        Return (lf.CompareTo(rt) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As VarInteger, rt As Integer) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As VarInteger, rt As Long) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Integer, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) > 0)
    End Operator

    ''' <summary>大なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >(lf As Long, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) > 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As VarInteger, rt As VarInteger) As Boolean
        Return (lf.CompareTo(rt) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As VarInteger, rt As Integer) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As VarInteger, rt As Long) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Integer, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) >= 0)
    End Operator

    ''' <summary>以上比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator >=(lf As Long, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) >= 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As VarInteger, rt As VarInteger) As Boolean
        Return (lf.CompareTo(rt) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As VarInteger, rt As Integer) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As VarInteger, rt As Long) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Integer, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) < 0)
    End Operator

    ''' <summary>小なり比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <(lf As Long, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) < 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As VarInteger, rt As VarInteger) As Boolean
        Return (lf.CompareTo(rt) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As VarInteger, rt As Integer) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As VarInteger, rt As Long) As Boolean
        Return (lf.CompareTo(New VarInteger(rt)) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Integer, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) <= 0)
    End Operator

    ''' <summary>以下比較を行います。</summary>
    ''' <param name="other">比較対象。</param>
    ''' <returns>比較結果。</returns>
    Public Shared Operator <=(lf As Long, rt As VarInteger) As Boolean
        Return (VarInteger.Create(lf).CompareTo(rt) <= 0)
    End Operator

#End Region

#Region "文字列の変換"

    ''' <summary>文字列から分数へ変換します。</summary>
    ''' <param name="input">変換する文字列。</param>
    ''' <returns>分数。</returns>
    Public Shared Function Parse(input As String) As VarInteger
        Dim res As VarInteger = Nothing
        If VarInteger.TryParse(input, res) Then
            Return res
        Else
            Throw New FormatException("入力文字列の形式が正しくありません")
        End If
    End Function

    ''' <summary>文字列から分数へ変数します。</summary>
    ''' <param name="input">変換する文字列。</param>
    ''' <param name="outValue">変換した分数。</param>
    ''' <returns>変換できたら真。</returns>
    Public Shared Function TryParse(input As String, ByRef outValue As VarInteger) As Boolean
        input = If(input?.Trim(), "")

        If input <> "" Then
            Dim number = VarInteger.Zero
            Dim dec As New VarInteger(10)

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
                Select Case input(i)
                    Case "0"c To "9"c
                        number = number.Multiplication(dec) + New VarInteger(AscW(input(i)) - AscW("0"))

                    Case Else
                        Return False
                End Select
            Next

            outValue = New VarInteger(sign, number.Raw)
            Return True
        Else
            Return False
        End If
    End Function

#End Region

    ''' <summary>バイト配列情報。</summary>
    Friend NotInheritable Class ByteArray
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

        ''' <summary>生値のバイト配列を取得します。</summary>
        ''' <returns>生値のバイト。</returns>
        Public ReadOnly Property Raw As Byte()
            Get
                Dim res = New Byte(Me.mValue.Length - 1) {}
                Array.Copy(Me.mValue, res, Me.mValue.Length)
                Return res
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
        Public Sub New(source As Byte())
            Me.mValue = If(source.Length > 0, source, New Byte() {0})
        End Sub

        ''' <summary>格納するバイト長を調整してコピーします。</summary>
        ''' <returns>バイト配列。</returns>
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
        Public Sub LeftShift()
            Dim ovflow As Byte = 0
            For i As Integer = 0 To Me.mValue.Length - 1
                Dim flag = CByte(If(Me.mValue(i) > 127, 1, 0))
                Me.mValue(i) = Me.mValue(i) << 1 Or ovflow
                ovflow = flag
            Next
        End Sub

        ''' <summary>ビットを指定数左にシフトする。</summary>
        ''' <param name="nest">シフトする数。</param>
        Public Sub LeftShift(nest As Integer)
            Dim skpByte = nest \ 8
            Dim skpBit = nest And 7

            If skpByte > 0 Then
                For i As Integer = Me.mValue.Length - skpByte - 1 To 0 Step -1
                    Me.mValue(i + skpByte) = Me.mValue(i)
                    Me.mValue(i) = 0
                Next
            End If

            If skpBit > 0 Then
                Dim ovflow As Byte = 0
                For i As Integer = 0 To Me.mValue.Length - 1
                    Dim flag = Me.mValue(i) >> (8 - skpBit)
                    Me.mValue(i) = (Me.mValue(i) << skpBit) Or ovflow
                    ovflow = flag
                Next
            End If
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
        ''' <param name="sign">符号。</param>
        ''' <returns>数値。</returns>
        Public Function GetLong(sign As Boolean) As Long
            If Me.mValue.Length <= 8 Then
                Dim res As ULong = 0
                For i As Integer = Me.mValue.Length - 1 To 0 Step -1
                    res = res << 8 Or Me.mValue(i)
                Next
                If sign Then
                    If (res And &H8000000000000000UL) = 0 Then Return CLng(res)
                Else
                    Return CLng(-res)
                End If
            End If
            Throw New OverflowException("Longに変換したらオーバーフローします")
        End Function

        ''' <summary>バイト配列の値を数値に変換します。</summary>
        ''' <param name="sign">符号。</param>
        ''' <returns>数値。</returns>
        Public Function GetInteger(sign As Boolean) As Integer
            If Me.mValue.Length <= 4 Then
                Dim res As UInteger = 0
                For i As Integer = Me.mValue.Length - 1 To 0 Step -1
                    res = res << 8 Or Me.mValue(i)
                Next
                If sign Then
                    If (res And &H80000000) = 0 Then Return CInt(res)
                Else
                    Return CInt(-res)
                End If
            End If
            Throw New OverflowException("Integerに変換したらオーバーフローします")
        End Function

    End Class

    ''' <summary>除算結果。</summary>
    Public Structure DivisionAnswer

        ''' <summary>商。</summary>
        Public ReadOnly Property Quotient As VarInteger

        ''' <summary>余。</summary>
        Public ReadOnly Property Remainder As VarInteger

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="quot">商。</param>
        ''' <param name="remd">余。</param>
        Public Sub New(quot As VarInteger, remd As VarInteger)
            Me.Quotient = quot
            Me.Remainder = remd
        End Sub

    End Structure

End Class
