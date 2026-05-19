using Xunit;
using OptionPricer.Models;
using OptionPricer.Pricing;
using System;

namespace OptionPricer.Tests
{
    public class BlackScholesPricerTests
    {
        private readonly BlackScholesPricer _pricer = new BlackScholesPricer();

        [Fact]
        public void Price_Call_Atm_ShouldBeCorrect()
        {
            // Arrange
            // Spot=100, Strike=100, T=1.0, r=0.05, vol=0.20, q=0.0, Call
            var option = new OptionContract(
                spot: 100.0,
                strike: 100.0,
                maturityInYears: 1.0,
                riskFreeRate: 0.05,
                volatility: 0.20,
                dividendYield: 0.0,
                optionType: OptionType.Call
            );

            // Act
            double price = _pricer.Price(option);

            // Assert
            // Analytical BSM Call Price with these parameters is ~10.45058
            Assert.Equal(10.45058, price, precision: 5);
        }

        [Fact]
        public void Price_Put_Atm_ShouldBeCorrect()
        {
            // Arrange
            // Spot=100, Strike=100, T=1.0, r=0.05, vol=0.20, q=0.0, Put
            var option = new OptionContract(
                spot: 100.0,
                strike: 100.0,
                maturityInYears: 1.0,
                riskFreeRate: 0.05,
                volatility: 0.20,
                dividendYield: 0.0,
                optionType: OptionType.Put
            );

            // Act
            double price = _pricer.Price(option);

            // Assert
            // Analytical BSM Put Price with these parameters is ~5.57352
            Assert.Equal(5.57352, price, precision: 5);
        }

        [Theory]
        [InlineData(100.0, 100.0, 1.0, 0.05, 0.20, 0.0)]     // ATM, no div
        [InlineData(110.0, 100.0, 0.5, 0.03, 0.25, 0.02)]    // ITM call, with div
        [InlineData(80.0, 100.0, 1.5, -0.01, 0.30, 0.01)]    // OTM call, negative rate, with div
        public void PutCallParity_ShouldHold(
            double spot, 
            double strike, 
            double maturity, 
            double rate, 
            double volatility, 
            double dividend)
        {
            // Arrange
            var callOption = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Call);
            var putOption = new OptionContract(spot, strike, maturity, rate, volatility, dividend, OptionType.Put);

            // Act
            double callPrice = _pricer.Price(callOption);
            double putPrice = _pricer.Price(putOption);

            // Put-Call Parity formula with continuous dividend:
            // C - P = S * e^(-q*T) - K * e^(-r*T)
            double lhs = callPrice - putPrice;
            double rhs = spot * Math.Exp(-dividend * maturity) - strike * Math.Exp(-rate * maturity);

            // Assert
            Assert.Equal(rhs, lhs, precision: 6);
        }
    }
}
