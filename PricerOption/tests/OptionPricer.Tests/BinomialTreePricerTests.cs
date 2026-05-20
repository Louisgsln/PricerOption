using System;
using Xunit;
using OptionPricer.Models;
using OptionPricer.Pricing;

namespace OptionPricer.Tests
{
    public class BinomialTreePricerTests
    {
        [Fact]
        public void Constructor_InvalidSteps_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new BinomialTreePricer(0));
            Assert.Throws<ArgumentException>(() => new BinomialTreePricer(-5));
        }

        [Theory]
        [InlineData(OptionType.Call, 10.450580)]
        [InlineData(OptionType.Put, 5.573520)]
        public void Price_EuropeanOption_MatchesBlackScholesWithinTolerance(OptionType optionType, double expectedBsPrice)
        {
            // Arrange
            // S=100, K=100, T=1.0, r=0.05, vol=0.20, q=0.0
            var option = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, optionType, OptionStyle.European);
            var pricer = new BinomialTreePricer(200); // 200 steps

            // Act
            double price = pricer.Price(option);

            // Assert
            // Binomial tree converges to Black-Scholes. For 200 steps, a tolerance of 0.02 is safe.
            Assert.True(System.Math.Abs(expectedBsPrice - price) < 0.02, $"Binomial price {price} should be close to BSM price {expectedBsPrice}");
        }

        [Fact]
        public void Price_AmericanPutOption_HasEarlyExercisePremium()
        {
            // Arrange
            // For a Put, if S=90, K=100, T=1.0, r=0.10 (high interest rate), vol=0.20, q=0.0
            // The European Put has lower value because it cannot be exercised early.
            // The American Put should be worth more.
            var euroPut = new OptionContract(90, 100, 1.0, 0.10, 0.20, 0.0, OptionType.Put, OptionStyle.European);
            var amerPut = new OptionContract(90, 100, 1.0, 0.10, 0.20, 0.0, OptionType.Put, OptionStyle.American);

            var bsPricer = new BlackScholesPricer();
            var binPricer = new BinomialTreePricer(200);

            // Act
            double priceEuro = bsPricer.Price(euroPut);
            double priceAmer = binPricer.Price(amerPut);

            // Assert
            Assert.True(priceAmer > priceEuro, $"American Put price ({priceAmer}) should be strictly greater than European Put price ({priceEuro}) due to early exercise.");
            // American premium should be significant under these parameters (typically > 0.50)
            Assert.True((priceAmer - priceEuro) > 0.20);
        }

        [Fact]
        public void Price_AmericanCallOnNoDividendStock_EqualsEuropeanCall()
        {
            // Arrange
            // Without dividends, it is never optimal to exercise an American Call early.
            // Therefore, within the same binomial model, American Call Price == European Call Price.
            var euroCall = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Call, OptionStyle.European);
            var amerCall = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Call, OptionStyle.American);

            var binPricer = new BinomialTreePricer(200);

            // Act
            double priceEuroBin = binPricer.Price(euroCall);
            double priceAmerBin = binPricer.Price(amerCall);

            // Assert
            // They should be mathematically identical in the binomial lattice model
            Assert.Equal(priceEuroBin, priceAmerBin, 10);
        }
    }
}
