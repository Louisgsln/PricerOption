namespace OptionPricer.Models
{
    /// <summary>
    /// Represents the exercise style of an option: European (at maturity only) or American (any time before maturity).
    /// </summary>
    public enum OptionStyle
    {
        European,
        American
    }
}
