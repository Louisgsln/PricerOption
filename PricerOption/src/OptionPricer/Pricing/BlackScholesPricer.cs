using System;
using OptionPricer.Models;
using OptionPricer.Maths;

namespace OptionPricer.Pricing
{
    /// <summary>
    /// Implements the Black-Scholes-Merton pricing model for European Call and Put options.
    /// Supports positive dividend yields.
    /// </summary>
    public class BlackScholesPricer : IPricer
    {
        /// <summary>
        /// Prices a European option using the Black-Scholes-Merton formula.
        /// </summary>
        /// <param name="option">The option contract parameters.</param>
        /// <returns>The theoretical option price.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the option parameter is null.</exception>
        public double Price(OptionContract option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            if (option.OptionStyle == OptionStyle.American)
                throw new NotSupportedException("Black-Scholes analytical model does not support American style options.");

            double S = option.Spot;
            double K = option.Strike;
            double T = option.MaturityInYears;
            double r = option.RiskFreeRate;
            double q = option.DividendYield;

            var (d1, d2) = CalculateD1D2(option);

            // BSM Pricing Formulas:
            // Call = S * e^(-q*T) * N(d1) - K * e^(-r*T) * N(d2)
            // Put  = K * e^(-r*T) * N(-d2) - S * e^(-q*T) * N(-d1)
            if (option.OptionType == OptionType.Call)
            {
                double callPrice = S * System.Math.Exp(-q * T) * NormalDistribution.Cdf(d1) 
                                 - K * System.Math.Exp(-r * T) * NormalDistribution.Cdf(d2);
                return System.Math.Max(0.0, callPrice); // Option price cannot be negative in practice
            }
            else
            {
                double putPrice = K * System.Math.Exp(-r * T) * NormalDistribution.Cdf(-d2) 
                                - S * System.Math.Exp(-q * T) * NormalDistribution.Cdf(-d1);
                return System.Math.Max(0.0, putPrice); // Option price cannot be negative in practice
            }
        }

        /// <summary>
        /// Helper utility to calculate the d1 and d2 components of the Black-Scholes formulas.
        /// Formula:
        /// d1 = [ ln(S/K) + (r - q + sigma^2/2)T ] / [ sigma * sqrt(T) ]
        /// d2 = d1 - sigma * sqrt(T)
        /// </summary>
        /// <param name="option">The option contract parameters.</param>
        /// <returns>A tuple containing d1 and d2.</returns>
        public static (double D1, double D2) CalculateD1D2(OptionContract option)
        {
            double S = option.Spot;
            double K = option.Strike;
            double T = option.MaturityInYears;
            double r = option.RiskFreeRate;
            double sigma = option.Volatility;
            double q = option.DividendYield;

            double sqrtT = System.Math.Sqrt(T);
            double d1 = (System.Math.Log(S / K) + (r - q + 0.5 * sigma * sigma) * T) / (sigma * sqrtT);
            double d2 = d1 - sigma * sqrtT;

            return (d1, d2);
        }
    }
}
