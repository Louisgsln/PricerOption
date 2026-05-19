using Xunit;
using OptionPricer.Models;
using OptionPricer.Pricing;
using OptionPricer.Solvers;
using System;

namespace OptionPricer.Tests
{
    public class ImpliedVolatilitySolverTests
    {
        private readonly BlackScholesPricer _pricer = new BlackScholesPricer();
        private readonly ImpliedVolatilitySolver _solver = new ImpliedVolatilitySolver();

        [Theory]
        [InlineData(OptionType.Call, 100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]
        [InlineData(OptionType.Put, 100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]
        [InlineData(OptionType.Call, 110.0, 100.0, 0.5, 0.03, 0.35, 0.02)]
        [InlineData(OptionType.Put, 90.0, 100.0, 0.75, 0.01, 0.15, 0.01)]
        public void Solve_ShouldRecoverVolatilityPrecisely(
            OptionType optionType,
            double spot,
            double strike,
            double maturity,
            double rate,
            double targetVolatility,
            double dividend)
        {
            // Arrange
            // 1. Create a contract with the target volatility to generate a "market price"
            var contract = new OptionContract(spot, strike, maturity, rate, targetVolatility, dividend, optionType);
            double price = _pricer.Price(contract);

            // 2. Create the contract with a dummy volatility (to represent the unknown volatility state)
            var optionWithoutVol = new OptionContract(spot, strike, maturity, rate, volatility: 0.10, dividend, optionType);

            // Act
            var result = _solver.Solve(optionWithoutVol, price);

            // Assert
            Assert.True(result.IsSuccessful, $"Solving failed: {result.Message}");
            Assert.Equal(targetVolatility, result.ImpliedVolatility, precision: 5);
            Assert.True(result.Iterations > 0, "Iteration count should be greater than zero");
        }

        [Fact]
        public void Solve_WithArbitrageViolatingPrice_ShouldReturnUnsuccessful()
        {
            // Arrange
            // A market price of 50 for a Call on 100 Strike when Spot is 100 and Maturity is 1.0 (interest rate = 5%)
            // is above the maximum possible Call price (which is Spot * e^-qT = 100) but wait:
            // What if price is too low? Call option cannot be priced below intrinsic value.
            // Minimum price = Spot - Strike * e^(-r*T) = 100 - 100 * e^(-0.05) = 100 - 95.12 = 4.88
            // Let's set a market price of 1.0 (well below 4.88)
            var optionWithoutVol = new OptionContract(100.0, 100.0, 1.0, 0.05, 0.20, 0.0, OptionType.Call);
            double invalidPrice = 1.0; 

            // Act
            var result = _solver.Solve(optionWithoutVol, invalidPrice);

            // Assert
            Assert.False(result.IsSuccessful);
            Assert.Contains("violates arbitrage bounds", result.Message);
            Assert.Equal(0.0, result.ImpliedVolatility);
        }
    }
}
