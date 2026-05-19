using OptionPricer.Models;

namespace OptionPricer.Pricing
{
    /// <summary>
    /// Defines a contract for option pricing models.
    /// </summary>
    public interface IPricer
    {
        /// <summary>
        /// Prices a given European vanilla option contract.
        /// </summary>
        /// <param name="option">The option contract parameters.</param>
        /// <returns>The theoretical price of the option.</returns>
        double Price(OptionContract option);
    }
}
