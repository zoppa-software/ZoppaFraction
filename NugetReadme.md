# ZoppaFraction
数値を分数表現で保持するライブラリです

## 説明
浮動小数点で`0.1`を 10回加算しても `1` にならない場合があります。  
これは二進数の小数表現の誤差であり浮動小数点の仕組み上仕方がないことです。  
また、Decimal型を使用しても `1/3` を 3回加算すると誤差がでることがあります。  
このような誤差を無くすため有理数ライブラリは数値を分数で保持し、誤差を無くしています。  
本ライブラリは簡単なものですが、上記方法を採用し、誤差を無くすことを目標としています。

``` vb
Dim ans = CFra(1.0) / 3.0 * 3.0
Assert.Equal(ans, 1)

' 浮動小数点では以下の結果は 1になりません
Dim a = 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1 + 0.1

' こちらは 1になります
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

以上、簡単な説明となります。**ライブラリの詳細は[Githubのページ](https://github.com/zoppa-software/ZoppaFraction)を参照してください。**