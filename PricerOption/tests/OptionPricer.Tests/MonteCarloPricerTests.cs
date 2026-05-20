using System;
using Xunit;
using OptionPricer.Models;
using OptionPricer.Pricing;

namespace OptionPricer.Tests
{
    public class MonteCarloPricerTests
    {
        [Fact]
        public void Constructor_InvalidPaths_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new MonteCarloPricer(1));
            Assert.Throws<ArgumentException>(() => new MonteCarloPricer(-10));
        }

        [Fact]
        public void Price_AmericanOption_ThrowsNotSupportedException()
        {
            // Arrange
            var option = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Call, OptionStyle.American);
            var pricer = new MonteCarloPricer(1000);

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => pricer.Price(option));
        }

        [Theory]
        [InlineData(OptionType.Call, 10.450580)]
        [InlineData(OptionType.Put, 5.573520)]
        public void Price_EuropeanOption_ConvergesWithinThreeStandardErrors(OptionType optionType, double bsPrice)
        {
            // Arrange
            // S=100, K=100, T=1.0, r=0.05, vol=0.20, q=0.0
            var option = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, optionType, OptionStyle.European);
            var pricer = new MonteCarloPricer(100000, 42); // 100,000 paths, seed 42

            // Act
            double mcPrice = pricer.Price(option);
            double se = pricer.LastStandardError;

            // Assert
            double difference = System.Math.Abs(mcPrice - bsPrice);
            double limit = 3.0 * se; // 99.7% confidence interval

            Assert.True(difference <= limit, $"Monte Carlo price {mcPrice} is outside 3 standard errors (limit {limit:F6}, difference {difference:F6}) of BSM price {bsPrice}");
        }
    }
}
