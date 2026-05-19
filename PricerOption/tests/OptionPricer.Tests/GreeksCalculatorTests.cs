using Xunit;
using OptionPricer.Models;
using OptionPricer.Greeks;
using System;

namespace OptionPricer.Tests
{
    public class GreeksCalculatorTests
    {
        [Theory]
        [InlineData(100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]
        [InlineData(120.0, 100.0, 0.5, 0.03, 0.25, 0.02)]
        [InlineData(80.0, 100.0, 1.5, -0.02, 0.30, 0.01)]
        public void Delta_Call_ShouldBeBetweenZeroAndOne(
            double spot, double strike, double maturity, double rate, double volatility, double dividend)
        {
            // Arrange
            var option = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Call);

            // Act
            double delta = GreeksCalculator.CalculateDelta(option);

            // Assert
            Assert.True(delta > 0.0, $"Call Delta {delta} should be strictly greater than 0");
            Assert.True(delta <= 1.0, $"Call Delta {delta} should be less than or equal to 1");
        }

        [Theory]
        [InlineData(100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]
        [InlineData(120.0, 100.0, 0.5, 0.03, 0.25, 0.02)]
        [InlineData(80.0, 100.0, 1.5, -0.02, 0.30, 0.01)]
        public void Delta_Put_ShouldBeBetweenMinusOneAndZero(
            double spot, double strike, double maturity, double rate, double volatility, double dividend)
        {
            // Arrange
            var option = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Put);

            // Act
            double delta = GreeksCalculator.CalculateDelta(option);

            // Assert
            Assert.True(delta < 0.0, $"Put Delta {delta} should be strictly less than 0");
            Assert.True(delta >= -1.0, $"Put Delta {delta} should be greater than or equal to -1");
        }

        [Theory]
        [InlineData(100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]
        [InlineData(120.0, 100.0, 0.5, 0.03, 0.25, 0.02)]
        [InlineData(80.0, 100.0, 1.5, -0.02, 0.30, 0.01)]
        public void Gamma_ShouldBePositive(
            double spot, double strike, double maturity, double rate, double volatility, double dividend)
        {
            // Arrange
            var callOption = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Call);
            var putOption = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Put);

            // Act
            double gammaCall = GreeksCalculator.CalculateGamma(callOption);
            double gammaPut = GreeksCalculator.CalculateGamma(putOption);

            // Assert
            Assert.True(gammaCall > 0.0, "Gamma for call must be strictly positive");
            Assert.Equal(gammaCall, gammaPut, precision: 9); // Gamma is identical for call and put
        }

        [Theory]
        [InlineData(100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]
        [InlineData(120.0, 100.0, 0.5, 0.03, 0.25, 0.02)]
        [InlineData(80.0, 100.0, 1.5, -0.02, 0.30, 0.01)]
        public void Vega_ShouldBePositive(
            double spot, double strike, double maturity, double rate, double volatility, double dividend)
        {
            // Arrange
            var callOption = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Call);
            var putOption = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Put);

            // Act
            double vegaCall = GreeksCalculator.CalculateVega(callOption);
            double vegaPut = GreeksCalculator.CalculateVega(putOption);

            // Assert
            Assert.True(vegaCall > 0.0, "Vega for call must be strictly positive");
            Assert.Equal(vegaCall, vegaPut, precision: 9); // Vega is identical for call and put
        }

        [Fact]
        public void Greeks_ForKnownCallScenario_ShouldBeAccurate()
        {
            // Arrange
            var option = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Call);

            // Act
            var greeks = GreeksCalculator.CalculateAll(option);

            // Assert
            Assert.Equal(0.63683, greeks.Delta, precision: 5);
            Assert.Equal(0.01876, greeks.Gamma, precision: 5);
            Assert.Equal(0.37524, greeks.Vega, precision: 5);
            Assert.Equal(-0.01757, greeks.Theta, precision: 5);
            Assert.Equal(0.53232, greeks.Rho, precision: 5);
        }
    }
}
