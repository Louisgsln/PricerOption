using System;
using OptionPricer.Models;
using OptionPricer.Pricing;
using OptionPricer.Greeks;

namespace OptionPricer.Solvers
{
    /// <summary>
    /// Represents the result of an implied volatility solving operation.
    /// </summary>
    public class ImpliedVolatilityResult
    {
        public double ImpliedVolatility { get; }
        public int Iterations { get; }
        public bool IsSuccessful { get; }
        public string Message { get; }

        public ImpliedVolatilityResult(double impliedVolatility, int iterations, bool isSuccessful, string message = "")
        {
            ImpliedVolatility = impliedVolatility;
            Iterations = iterations;
            IsSuccessful = isSuccessful;
            Message = message;
        }
    }

    /// <summary>
    /// Solves for the implied volatility of a European option given its market price
    /// using a hybrid Newton-Raphson and Bisection numerical root-finding algorithm.
    /// </summary>
    public class ImpliedVolatilitySolver
    {
        private const double Tolerance = 1e-6;
        private const int MaxIterations = 100;
        private const double MinVol = 0.0001; // 0.01%
        private const double MaxVol = 5.0;    // 500%

        /// <summary>
        /// Solves for the implied volatility of the given option contract.
        /// </summary>
        /// <param name="optionWithoutVolatility">An OptionContract instance where the volatility property will be solved for.</param>
        /// <param name="marketPrice">The observed market price of the option.</param>
        /// <returns>An ImpliedVolatilityResult containing the solved volatility, iteration count, and success status.</returns>
        public ImpliedVolatilityResult Solve(OptionContract optionWithoutVolatility, double marketPrice)
        {
            if (optionWithoutVolatility == null)
                throw new ArgumentNullException(nameof(optionWithoutVolatility));

            double S = optionWithoutVolatility.Spot;
            double K = optionWithoutVolatility.Strike;
            double T = optionWithoutVolatility.MaturityInYears;
            double r = optionWithoutVolatility.RiskFreeRate;
            double q = optionWithoutVolatility.DividendYield;

            // 1. Physical arbitrage bounds check
            double expQ = Math.Exp(-q * T);
            double expR = Math.Exp(-r * T);

            double minPrice;
            double maxPrice;

            if (optionWithoutVolatility.OptionType == OptionType.Call)
            {
                minPrice = Math.Max(0.0, S * expQ - K * expR);
                maxPrice = S * expQ;
            }
            else
            {
                minPrice = Math.Max(0.0, K * expR - S * expQ);
                maxPrice = K * expR;
            }

            // Add a small epsilon to account for floating-point inaccuracies
            const double boundsEpsilon = 1e-9;
            if (marketPrice < (minPrice - boundsEpsilon) || marketPrice > (maxPrice + boundsEpsilon))
            {
                return new ImpliedVolatilityResult(
                    0.0, 
                    0, 
                    false, 
                    $"Market price {marketPrice:F6} violates arbitrage bounds. Min: {minPrice:F6}, Max: {maxPrice:F6}"
                );
            }

            var pricer = new BlackScholesPricer();
            int iterations = 0;
            bool newtonFailed = false;

            // 2. Newton-Raphson method
            // Initial guess: 20% volatility
            double sigma = 0.20; 

            while (iterations < MaxIterations)
            {
                iterations++;

                OptionContract currentOption = optionWithoutVolatility.WithVolatility(sigma);
                double price = pricer.Price(currentOption);
                double f = price - marketPrice;

                // Check convergence
                if (Math.Abs(f) < Tolerance)
                {
                    return new ImpliedVolatilityResult(sigma, iterations, true, "Newton-Raphson converged.");
                }

                // Compute Vega derivative: f'(sigma) = Vega (annual)
                // GreeksCalculator.CalculateVega returns Vega for a 1% change, so multiply by 100
                double vegaAnnual = GreeksCalculator.CalculateVega(currentOption) * 100.0;

                // Avoid division by zero/very small Vega
                if (Math.Abs(vegaAnnual) < 1e-7)
                {
                    newtonFailed = true;
                    break;
                }

                double nextSigma = sigma - f / vegaAnnual;

                // Check if Newton-Raphson steps out of bounds
                if (nextSigma < MinVol || nextSigma > MaxVol)
                {
                    newtonFailed = true;
                    break;
                }

                sigma = nextSigma;
            }

            // 3. Bisection fallback
            if (newtonFailed || iterations >= MaxIterations)
            {
                double lowerVol = MinVol;
                double upperVol = MaxVol;

                double priceLower = pricer.Price(optionWithoutVolatility.WithVolatility(lowerVol)) - marketPrice;
                double priceUpper = pricer.Price(optionWithoutVolatility.WithVolatility(upperVol)) - marketPrice;

                // Check sign change
                if (priceLower * priceUpper > 0)
                {
                    return new ImpliedVolatilityResult(
                        0.0, 
                        iterations, 
                        false, 
                        "No root exists within volatility bounds [0.01% - 500%]."
                    );
                }

                while (iterations < MaxIterations)
                {
                    iterations++;
                    double midVol = lowerVol + 0.5 * (upperVol - lowerVol);
                    double priceMid = pricer.Price(optionWithoutVolatility.WithVolatility(midVol)) - marketPrice;

                    // Check convergence (either function value is close to zero, or interval is tiny)
                    if (Math.Abs(priceMid) < Tolerance || (upperVol - lowerVol) * 0.5 < Tolerance)
                    {
                        return new ImpliedVolatilityResult(midVol, iterations, true, "Bisection fallback converged.");
                    }

                    if (priceLower * priceMid < 0)
                    {
                        upperVol = midVol;
                        priceUpper = priceMid;
                    }
                    else
                    {
                        lowerVol = midVol;
                        priceLower = priceMid;
                    }
                }
            }

            return new ImpliedVolatilityResult(
                0.0, 
                iterations, 
                false, 
                "Solver exceeded maximum iterations without reaching convergence."
            );
        }
    }
}
