# ZoppaFraction
数値を分数表現で保持するライブラリです

## 説明
浮動小数点で`0.1`を 10回加算しても `1` にならない場合があります。  
これは二進数の小数表現の誤差であり浮動小数点の仕組み上仕方がないことです。  
また、Decimal型を使用しても `1/3` を 3回加算すると誤差がでることがあります。  
このような誤差を無くすため有理数ライブラリは数値を分数で保持し、誤差を無くしています。  
本ライブラリは簡単なものですが、上記方法を採用し、誤差を無くすことを目標としています。

``` vb
' CFra(1.0)でFractionクラスを生成し、計算します
Dim ans = CFra(1.0) / 3.0 * 3.0
Assert.Equal(ans, 1)

' 浮動小数点では以下の結果は 1になりません
Dim a = 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1

' Fractionクラスでは 1/10を内部に保持しているため 1になります
Dim n = Fraction.Parse("0.1")
Assert.Equal(CDbl(n + n + n + n + n + n + n + n + n + n), 1)
```
``` cs
// C#だと以下のようなイメージです
var a1 = Fraction.Create(-1, 10);
var a2 = a1 + a1 + a1 + a1 + a1;
Assert.Equal(-0.5, (double)a2);

var a3 = (Fraction)3 / 5 * 5;
Assert.Equal(3, (double)a3);

var n = Fraction.Parse("-0.1");
Assert.Equal(-1, (double)(n + n + n + n + n + n + n + n + n + n));
```

## 比較、または特徴
* 簡単な機能のみ提供しています  
  
## 依存関係  
ライブラリは .NET Standard 2.0 で記述しています。そのため、.net framework 4.6.1以降、.net core 2.0以降で使用できます。  
その他のライブラリへの依存関係はありません。  

## 使い方
### **分数**
  
``` vb
' 
``` 
インスタンス生成は以下の方法があります。  
||方法|  
|----------|---------|  
| Integerから生成する。 |`Create`メソッドを使用する。</br>Fraction.Create(`Integer`)</br>Fraction.Create(`Integer(分子)`, `UInteger(分母)`)|
| 〃|`ChangeFraction`拡張メソッドを使用する。`Integer`.ChangeFraction()|
| 〃|キャストを使用する。CType(`Integer`, Fraction)|
| 〃|`CFra`メソッドを使用する。CFra(`Integer`)|
| Longから生成する。 |`Create`メソッドを使用する。</br>Fraction.Create(`Long`)</br>Fraction.Create(`Long(分子)`, `ULong(分母)`)|
| 〃|`ChangeFraction`拡張メソッドを使用する。`Long`.ChangeFraction()|
| 〃|キャストを使用する。CType(`Long`, Fraction)|
| 〃|`CFra`メソッドを使用する。CFra(`Long`)|
| Doubleから生成する。 |`Create`メソッドを使用する。Fraction.Create(`Double`)|
| 〃|`ChangeFraction`拡張メソッドを使用する。`Double`.ChangeFraction()|
| 〃|キャストを使用する。CType(`Double`, Fraction)|
| 〃|`CFra`メソッドを使用する。CFra(`Double`)|
| Decimalから生成する。 |`Create`メソッドを使用する。Fraction.Create(`Decimal`)|
| 〃|`ChangeFraction`拡張メソッドを使用する。`Decimal`.ChangeFraction()|
| 〃|キャストを使用する。CType(`Decimal`, Fraction)|
| 〃|`CFra`メソッドを使用する。CFra(`Decimal`)|
| 文字列から生成する。 |`Parse`メソッドを使用します。Fraction.Parse(`文字列`)、Fraction.TryParse(`文字列`)| 

### **多倍長整数**  
分数の分子、分母は **多倍長整数** を使用して実装しています。  
多倍長変数も四則演算をサポートしています。  
``` vb
' 以下のようにLong(64bit)で表現できない40桁の整数の四則演算をサポートします
Dim longValue = VarInteger.Parse("1234567890123456789012345678901234567890") + 5
Assert.Equal(longValue.ToString(), "1234567890123456789012345678901234567895")
``` 
インスタンス生成は以下の方法があります。  
||方法|  
|----------|---------|  
| Integerから生成する。 |コンストラクタを使用する。New VarInteger(`Integer`)|  
| 〃|`Create`メソッドを使用する。VarInteger.Create(`Integer`)|
| 〃|`ChangeVarInteger`拡張メソッドを使用する。`Integer`.ChangeVarInteger()|
| 〃|キャストを使用する。CType(`Integer`, VarInteger)|
| 〃|`CVarInt`メソッドを使用する。CVarInt(`Integer`)|
| Longから生成する。 |コンストラクタを使用する。New VarInteger(`Long`)|  
| 〃|`Create`メソッドを使用する。VarInteger.Create(`Long`)|
| 〃|`ChangeVarInteger`拡張メソッドを使用する。`Long`.ChangeVarInteger()|
| 〃|キャストを使用する。CType(`Long`, VarInteger)|
| 〃|`CVarInt`メソッドを使用する。CVarInt(`Long`)|
| 文字列から生成する。 |`Parse`メソッドを使用します。VarInteger.Parse(`文字列`)、VarInteger.TryParse(`文字列`)| 

## インストール
ソースをビルドして `ZoppaFraction.dll` ファイルを生成して参照してください。  
Nugetにライブラリを公開しています。[ZoppaFraction](https://www.nuget.org/packages/ZoppaFraction/)を参照してください。

## 作成情報
* 造田　崇（zoppa software）
* ミウラ第3システムカンパニー 
* takashi.zouta@kkmiuta.jp

## ライセンス
[apache 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)
