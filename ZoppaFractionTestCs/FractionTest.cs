using ZoppaFraction;

namespace ZoppaFractionTestCs
{
    public class FractionTest
    {
        [Fact]
        public void NumberTest()
        {
            var a1 = Fraction.Create(-1, 10);
            var a2 = a1 + a1 + a1 + a1 + a1;
            Assert.Equal(-0.5, (double)a2);

            var a3 = (Fraction)3 / 5 * 5;
            Assert.Equal(3, (double)a3);

            var n = Fraction.Parse("-0.1");
            Assert.Equal(-1, (double)(n + n + n + n + n + n + n + n + n + n));
        }
    }
}