using System;

namespace OptionPricer.Maths{
    /// <summary>
    /// Provides mathematical functions for the Standard Normal Distribution.
    /// </summary>
    public static class NormalDistribution
    {
        private const double OneOverRootTwoPi = 0.3989422804014327; // 1 / sqrt(2 * pi)

        /// <summary>
        /// Computes the Probability Density Function (PDF) of the standard normal distribution.
        /// Formula: phi(x) = (1 / sqrt(2 * pi)) * exp(-x^2 / 2)
        /// </summary>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The PDF value at x.</returns>
        public static double Pdf(double x)
        {
            return OneOverRootTwoPi * System.Math.Exp(-0.5 * x * x);
        }

        /// <summary>
        /// Computes the Cumulative Distribution Function (CDF) of the standard normal distribution.
        /// Uses the highly accurate Abramowitz and Stegun polynomial approximation (formula 26.2.17).
        /// Absolute error is guaranteed to be less than 7.5e-8.
        /// Formula for x >= 0: N(x) = 1 - phi(x) * (b1*t + b2*t^2 + b3*t^3 + b4*t^4 + b5*t^5) where t = 1 / (1 + p*x)
        /// For x < 0: N(x) = 1 - N(-x)
        /// </summary>
        /// <param name="x">The value to evaluate.</param>
        /// <returns>The cumulative probability value (between 0 and 1).</returns>
        public static double Cdf(double x)
        {
            if (double.IsNaN(x))
                throw new ArgumentException("Input cannot be NaN.", nameof(x));

            if (x < 0.0)
            {
                return 1.0 - Cdf(-x);
            }

            const double p = 0.2316419;
            const double b1 = 0.319381530;
            const double b2 = -0.356563782;
            const double b3 = 1.781477937;
            const double b4 = -1.821255978;
            const double b5 = 1.330274429;

            double t = 1.0 / (1.0 + p * x);
            double poly = t * (b1 + t * (b2 + t * (b3 + t * (b4 + t * b5))));
            
            // To ensure numerical stability near the tail, we clamp the CDF to [0.0, 1.0]
            double cdf = 1.0 - Pdf(x) * poly;
            return System.Math.Clamp(cdf, 0.0, 1.0);
        }
    }
}
