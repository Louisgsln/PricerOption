using System;
using OptionPricer.Models;

namespace OptionPricer.Pricing
{
    /// <summary>
    /// Implements a Monte Carlo simulation option pricing model.
    /// Supports European Call and Put options with continuous dividend yields.
    /// Incorporates Antithetic Variates for variance reduction and computes the simulation's standard error.
    /// </summary>
    public class MonteCarloPricer : IPricer
    {
        public int Paths { get; }
        public int Seed { get; }
        
        /// <summary>
        /// Gets the Standard Error of the last executed simulation run.
        /// </summary>
        public double LastStandardError { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonteCarloPricer"/> class.
        /// </summary>
        /// <param name="paths">The total number of simulated asset paths (default is 100,000).</param>
        /// <param name="seed">The random number generator seed (default is 42 for reproducible results).</param>
        /// <exception cref="ArgumentException">Thrown if paths is less than 2.</exception>
        public MonteCarloPricer(int paths = 100_000, int seed = 42)
        {
            if (paths < 2)
                throw new ArgumentException("Number of paths must be at least 2.", nameof(paths));
            Paths = paths;
            Seed = seed;
        }

        /// <summary>
        /// Prices a European option using Monte Carlo simulation.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if the option style is American.</exception>
        public double Price(OptionContract option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            if (option.OptionStyle == OptionStyle.American)
                throw new NotSupportedException("Monte Carlo simulation model does not support American style options.");

            double S = option.Spot;
            double K = option.Strike;
            double T = option.MaturityInYears;
            double r = option.RiskFreeRate;
            double sigma = option.Volatility;
            double q = option.DividendYield;

            // Ensure we have an even number of paths for antithetic pairs
            int numPairs = Paths / 2;
            int totalPaths = numPairs * 2;

            double drift = (r - q - 0.5 * sigma * sigma) * T;
            double volTerm = sigma * System.Math.Sqrt(T);

            var rand = new System.Random(Seed);

            double sumPayoffs = 0.0;
            double sumSqPayoffs = 0.0;

            for (int i = 0; i < numPairs; i++)
            {
                // Box-Muller transform to generate standard normal Z
                double u1 = rand.NextDouble();
                while (u1 == 0.0) u1 = rand.NextDouble(); // Avoid log(0)
                double u2 = rand.NextDouble();

                double logTerm = System.Math.Sqrt(-2.0 * System.Math.Log(u1));
                double angleTerm = 2.0 * System.Math.PI * u2;
                double z = logTerm * System.Math.Cos(angleTerm);

                // Simulate terminal stock price
                // Path 1
                double sT1 = S * System.Math.Exp(drift + volTerm * z);
                // Path 2 (Antithetic)
                double sT2 = S * System.Math.Exp(drift - volTerm * z);

                // Compute payoffs
                double payoff1;
                double payoff2;

                if (option.OptionType == OptionType.Call)
                {
                    payoff1 = System.Math.Max(0.0, sT1 - K);
                    payoff2 = System.Math.Max(0.0, sT2 - K);
                }
                else
                {
                    payoff1 = System.Math.Max(0.0, K - sT1);
                    payoff2 = System.Math.Max(0.0, K - sT2);
                }

                sumPayoffs += payoff1 + payoff2;
                sumSqPayoffs += (payoff1 * payoff1) + (payoff2 * payoff2);
            }

            double discountFactor = System.Math.Exp(-r * T);
            double avgPayoff = sumPayoffs / totalPaths;
            double price = discountFactor * avgPayoff;

            // Calculate standard deviation of payoffs
            double variance = (sumSqPayoffs / totalPaths) - (avgPayoff * avgPayoff);
            double stdDev = System.Math.Sqrt(System.Math.Max(0.0, variance));

            // Standard Error of Monte Carlo estimate
            LastStandardError = discountFactor * (stdDev / System.Math.Sqrt(totalPaths));

            return price;
        }
    }
}
