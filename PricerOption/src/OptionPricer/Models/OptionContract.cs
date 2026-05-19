using System;

namespace OptionPricer.Models
{
    /// <summary>
    /// Represents the parameters for a European vanilla option contract.
    /// </summary>
    public class OptionContract
    {
        public double Spot { get; }
        public double Strike { get; }
        public double MaturityInYears { get; }
        public double RiskFreeRate { get; }
        public double Volatility { get; }
        public double DividendYield { get; }
        public OptionType OptionType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionContract"/> class.
        /// </summary>
        /// <param name="spot">Current spot price of the underlying asset (must be > 0).</param>
        /// <param name="strike">Strike price (must be > 0).</param>
        /// <param name="maturityInYears">Time to maturity in years (must be > 0).</param>
        /// <param name="riskFreeRate">Annualized risk-free interest rate (can be negative).</param>
        /// <param name="volatility">Annualized volatility of the underlying asset (must be > 0).</param>
        /// <param name="dividendYield">Annualized dividend yield of the underlying asset (must be >= 0).</param>
        /// <param name="optionType">The option type: Call or Put.</param>
        /// <exception cref="ArgumentException">Thrown when validation checks fail.</exception>
        public OptionContract(
            double spot,
            double strike,
            double maturityInYears,
            double riskFreeRate,
            double volatility,
            double dividendYield = 0.0,
            OptionType optionType = OptionType.Call)
        {
            if (spot <= 0)
                throw new ArgumentException("Spot price must be strictly positive.", nameof(spot));
            if (strike <= 0)
                throw new ArgumentException("Strike price must be strictly positive.", nameof(strike));
            if (maturityInYears <= 0)
                throw new ArgumentException("Maturity in years must be strictly positive.", nameof(maturityInYears));
            if (volatility <= 0)
                throw new ArgumentException("Volatility must be strictly positive.", nameof(volatility));
            if (dividendYield < 0)
                throw new ArgumentException("Dividend yield cannot be negative.", nameof(dividendYield));

            Spot = spot;
            Strike = strike;
            MaturityInYears = maturityInYears;
            RiskFreeRate = riskFreeRate;
            Volatility = volatility;
            DividendYield = dividendYield;
            OptionType = optionType;
        }

        /// <summary>
        /// Creates a new OptionContract instance with a modified volatility.
        /// Useful for implied volatility solver and scenario testing.
        /// </summary>
        public OptionContract WithVolatility(double newVolatility)
        {
            return new OptionContract(
                Spot,
                Strike,
                MaturityInYears,
                RiskFreeRate,
                newVolatility,
                DividendYield,
                OptionType
            );
        }
    }
}
