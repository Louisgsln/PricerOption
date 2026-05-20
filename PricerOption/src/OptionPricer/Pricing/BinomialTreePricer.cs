using System;
using OptionPricer.Models;

namespace OptionPricer.Pricing
{
    /// <summary>
    /// Implements the Cox-Ross-Rubinstein (CRR) Binomial Tree pricing model.
    /// Supports both European and American options, including continuous dividend yields.
    /// </summary>
    public class BinomialTreePricer : IPricer
    {
        public int Steps { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinomialTreePricer"/> class.
        /// </summary>
        /// <param name="steps">The number of steps in the binomial lattice (default is 200).</param>
        /// <exception cref="ArgumentException">Thrown if steps is less than 1.</exception>
        public BinomialTreePricer(int steps = 200)
        {
            if (steps < 1)
                throw new ArgumentException("Number of steps in the tree must be at least 1.", nameof(steps));
            Steps = steps;
        }

        /// <summary>
        /// Prices an option using the Cox-Ross-Rubinstein binomial tree model.
        /// </summary>
        public double Price(OptionContract option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            double S = option.Spot;
            double K = option.Strike;
            double T = option.MaturityInYears;
            double r = option.RiskFreeRate;
            double sigma = option.Volatility;
            double q = option.DividendYield;

            double dt = T / Steps;
            double sqrtDt = System.Math.Sqrt(dt);

            // CRR Parameters:
            // u = e^(sigma * sqrt(dt))
            // d = e^(-sigma * sqrt(dt)) = 1 / u
            double u = System.Math.Exp(sigma * sqrtDt);
            double d = 1.0 / u;

            // Risk-neutral probability: p = (e^((r - q) * dt) - d) / (u - d)
            double expTerm = System.Math.Exp((r - q) * dt);
            double p = (expTerm - d) / (u - d);
            double discountFactor = System.Math.Exp(-r * dt);

            // 1. Initialize option values at maturity (Step N)
            double[] optionValues = new double[Steps + 1];
            for (int j = 0; j <= Steps; j++)
            {
                // Spot price at node (Steps, j)
                double spotAtNode = S * System.Math.Pow(u, j) * System.Math.Pow(d, Steps - j);
                
                if (option.OptionType == OptionType.Call)
                {
                    optionValues[j] = System.Math.Max(0.0, spotAtNode - K);
                }
                else
                {
                    optionValues[j] = System.Math.Max(0.0, K - spotAtNode);
                }
            }

            // 2. Step backward through the tree
            for (int i = Steps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= i; j++)
                {
                    // Continuation value: expected discounted value
                    double continuationValue = discountFactor * (p * optionValues[j + 1] + (1.0 - p) * optionValues[j]);

                    if (option.OptionStyle == OptionStyle.American)
                    {
                        // Asset price at node (i, j)
                        double spotAtNode = S * System.Math.Pow(u, j) * System.Math.Pow(d, i - j);
                        
                        // Early exercise payoff
                        double exercisePayoff = option.OptionType == OptionType.Call
                            ? System.Math.Max(0.0, spotAtNode - K)
                            : System.Math.Max(0.0, K - spotAtNode);

                        optionValues[j] = System.Math.Max(continuationValue, exercisePayoff);
                    }
                    else
                    {
                        optionValues[j] = continuationValue;
                    }
                }
            }

            return optionValues[0];
        }
    }
}
