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
### **可変長変数**

  
## インストール
ソースをビルドして `ZoppaFraction.dll` ファイルを生成して参照してください。  
Nugetにライブラリを公開しています。[ZoppaFraction](https://www.nuget.org/packages/ZoppaFraction/)を参照してください。

## 作成情報
* 造田　崇（zoppa software）
* ミウラ第3システムカンパニー 
* takashi.zouta@kkmiuta.jp

## ライセンス
[apache 2.0](https://www.apache.org/licenses/LICENSE-2.0.html)
