using System;
using OptionPricer.Models;
using OptionPricer.Maths;
using OptionPricer.Pricing;

namespace OptionPricer.Greeks
{
    /// <summary>
    /// Holds the computed Greeks for an option.
    /// </summary>
    public class GreeksResult
    {
        public double Delta { get; }
        public double Gamma { get; }
        public double Vega { get; }
        public double Theta { get; }
        public double Rho { get; }

        public GreeksResult(double delta, double gamma, double vega, double theta, double rho)
        {
            Delta = delta;
            Gamma = gamma;
            Vega = vega;
            Theta = theta;
            Rho = rho;
        }
    }

    /// <summary>
    /// Computes the primary Greeks (Delta, Gamma, Vega, Theta, Rho) 
    /// analytically for European options using the Black-Scholes-Merton model.
    /// </summary>
    public static class GreeksCalculator
    {
        /// <summary>
        /// Computes all primary Greeks for a given option contract.
        /// </summary>
        public static GreeksResult CalculateAll(OptionContract option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            double delta = CalculateDelta(option);
            double gamma = CalculateGamma(option);
            double vega = CalculateVega(option);
            double theta = CalculateTheta(option);
            double rho = CalculateRho(option);

            return new GreeksResult(delta, gamma, vega, theta, rho);
        }

        /// <summary>
        /// Computes Delta: sensitivity of the option price to a change in the underlying price.
        /// Call Delta = e^(-q*T) * N(d1)
        /// Put Delta  = -e^(-q*T) * N(-d1)
        /// </summary>
        public static double CalculateDelta(OptionContract option)
        {
            var (d1, _) = BlackScholesPricer.CalculateD1D2(option);
            double q = option.DividendYield;
            double T = option.MaturityInYears;

            double expTerm = Math.Exp(-q * T);

            if (option.OptionType == OptionType.Call)
            {
                return expTerm * NormalDistribution.Cdf(d1);
            }
            else
            {
                return -expTerm * NormalDistribution.Cdf(-d1);
            }
        }

        /// <summary>
        /// Computes Gamma: sensitivity of Delta to a change in the underlying price (same for Call and Put).
        /// Gamma = [ e^(-q*T) * phi(d1) ] / [ S * sigma * sqrt(T) ]
        /// </summary>
        public static double CalculateGamma(OptionContract option)
        {
            var (d1, _) = BlackScholesPricer.CalculateD1D2(option);
            double S = option.Spot;
            double sigma = option.Volatility;
            double T = option.MaturityInYears;
            double q = option.DividendYield;

            double numerator = Math.Exp(-q * T) * NormalDistribution.Pdf(d1);
            double denominator = S * sigma * Math.Sqrt(T);

            return numerator / denominator;
        }

        /// <summary>
        /// Computes Vega: sensitivity of the option price to a change in volatility.
        /// Expressed for a 1% change in volatility (divided by 100).
        /// Vega (1%) = [ S * e^(-q*T) * phi(d1) * sqrt(T) ] / 100
        /// </summary>
        public static double CalculateVega(OptionContract option)
        {
            var (d1, _) = BlackScholesPricer.CalculateD1D2(option);
            double S = option.Spot;
            double T = option.MaturityInYears;
            double q = option.DividendYield;

            double vegaAnnual = S * Math.Exp(-q * T) * NormalDistribution.Pdf(d1) * Math.Sqrt(T);
            return vegaAnnual / 100.0;
        }

        /// <summary>
        /// Computes Theta: sensitivity of the option price to the passage of time.
        /// Expressed per day (divided by 365).
        /// Call Theta (annual) = -[ S * sigma * e^(-q*T) * phi(d1) ] / [ 2 * sqrt(T) ] 
        ///                       + q * S * e^(-q*T) * N(d1) 
        ///                       - r * K * e^(-r*T) * N(d2)
        /// Put Theta (annual)  = -[ S * sigma * e^(-q*T) * phi(d1) ] / [ 2 * sqrt(T) ] 
        ///                       - q * S * e^(-q*T) * N(-d1) 
        ///                       + r * K * e^(-r*T) * N(-d2)
        /// </summary>
        public static double CalculateTheta(OptionContract option)
        {
            var (d1, d2) = BlackScholesPricer.CalculateD1D2(option);
            double S = option.Spot;
            double K = option.Strike;
            double T = option.MaturityInYears;
            double r = option.RiskFreeRate;
            double sigma = option.Volatility;
            double q = option.DividendYield;

            double term1 = -(S * sigma * Math.Exp(-q * T) * NormalDistribution.Pdf(d1)) / (2.0 * Math.Sqrt(T));

            if (option.OptionType == OptionType.Call)
            {
                double term2 = q * S * Math.Exp(-q * T) * NormalDistribution.Cdf(d1);
                double term3 = r * K * Math.Exp(-r * T) * NormalDistribution.Cdf(d2);
                double thetaAnnual = term1 + term2 - term3;
                return thetaAnnual / 365.0; // Expressed per calendar day
            }
            else
            {
                double term2 = q * S * Math.Exp(-q * T) * NormalDistribution.Cdf(-d1);
                double term3 = r * K * Math.Exp(-r * T) * NormalDistribution.Cdf(-d2);
                double thetaAnnual = term1 - term2 + term3;
                return thetaAnnual / 365.0; // Expressed per calendar day
            }
        }

        /// <summary>
        /// Computes Rho: sensitivity of the option price to a change in the risk-free interest rate.
        /// Expressed for a 1% change in the interest rate (divided by 100).
        /// Call Rho (1%) = [ K * T * e^(-r*T) * N(d2) ] / 100
        /// Put Rho (1%)  = -[ K * T * e^(-r*T) * N(-d2) ] / 100
        /// </summary>
        public static double CalculateRho(OptionContract option)
        {
            var (_, d2) = BlackScholesPricer.CalculateD1D2(option);
            double K = option.Strike;
            double T = option.MaturityInYears;
            double r = option.RiskFreeRate;

            double expTerm = Math.Exp(-r * T);

            if (option.OptionType == OptionType.Call)
            {
                double rhoAnnual = K * T * expTerm * NormalDistribution.Cdf(d2);
                return rhoAnnual / 100.0;
            }
            else
            {
                double rhoAnnual = -K * T * expTerm * NormalDistribution.Cdf(-d2);
                return rhoAnnual / 100.0;
            }
        }
    }
}
