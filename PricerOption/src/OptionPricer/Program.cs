using System;
using OptionPricer.Models;
using OptionPricer.Pricing;
using OptionPricer.Greeks;
using OptionPricer.Solvers;

namespace OptionPricer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "OptionPricer - Quantitative Analytics Engine";
            bool running = true;

            while (running)
            {
                DisplayHeader();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=================================================");
                Console.WriteLine("                MAIN MENU                        ");
                Console.WriteLine("=================================================");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("1. Price European Option");
                Console.WriteLine("2. Calculate Greeks");
                Console.WriteLine("3. Solve Implied Volatility");
                Console.WriteLine("4. Run Sample Scenario");
                Console.WriteLine("5. Exit");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("=================================================");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Select an option (1-5): ");
                Console.ForegroundColor = ConsoleColor.White;

                string choice = Console.ReadLine() ?? string.Empty;
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        PriceOptionMenu();
                        break;
                    case "2":
                        CalculateGreeksMenu();
                        break;
                    case "3":
                        SolveImpliedVolMenu();
                        break;
                    case "4":
                        RunSampleScenario();
                        break;
                    case "5":
                        running = false;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Goodbye! Thank you for using OptionPricer.");
                        Console.ResetColor();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid selection. Please enter a number between 1 and 5.");
                        Console.ResetColor();
                        break;
                }

                if (running && choice != "5")
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("\nPress any key to return to the main menu...");
                    Console.ReadKey();
                }
            }
        }

        static void DisplayHeader()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"
   ____        _   _             _____      _                 
  / __ \      | | (_)           |  __ \    (_)                
 | |  | |_ __ | |_ _  ___  _ __ | |__) | __ _  ___ ___ _ __  
 | |  | | '_ \| __| |/ _ \| '_ \|  ___/ '__| |/ __/ _ \ '__| 
 | |__| | |_) | |_| | (_) | | | | |   | |  | | (_|  __/ |    
  \____/| .__/ \__|_|\___/|_| |_|_|   |_|  |_|\___\___|_|    
        | |                                                   
        |_|                                                   
");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("   European Option Pricing & Analytics Engine (.NET 8)");
            Console.WriteLine();
            Console.ResetColor();
        }

        static void PriceOptionMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Price European Option ---");
            Console.ResetColor();

            var type = ReadOptionType();
            double spot = ReadDouble("Spot Price (S > 0): ", s => s > 0, "Spot price must be strictly positive.");
            double strike = ReadDouble("Strike Price (K > 0): ", k => k > 0, "Strike price must be strictly positive.");
            double maturity = ReadDouble("Maturity in Years (T > 0): ", t => t > 0, "Maturity must be strictly positive.");
            double rate = ReadDouble("Risk-Free Interest Rate (r, e.g. 0.05 for 5%): ", _ => true, "Please enter a valid rate.");
            double vol = ReadDouble("Volatility (sigma, e.g. 0.20 for 20% > 0): ", v => v > 0, "Volatility must be strictly positive.");
            double div = ReadOptionalDouble("Dividend Yield (q, e.g. 0.02 for 2%)", 0.0);

            try
            {
                var contract = new OptionContract(spot, strike, maturity, rate, vol, div, type);
                var pricer = new BlackScholesPricer();
                double price = pricer.Price(contract);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($">>> {type} Price: {price:F6}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error pricing option: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void CalculateGreeksMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Calculate Option Greeks ---");
            Console.ResetColor();

            var type = ReadOptionType();
            double spot = ReadDouble("Spot Price (S > 0): ", s => s > 0, "Spot price must be strictly positive.");
            double strike = ReadDouble("Strike Price (K > 0): ", k => k > 0, "Strike price must be strictly positive.");
            double maturity = ReadDouble("Maturity in Years (T > 0): ", t => t > 0, "Maturity must be strictly positive.");
            double rate = ReadDouble("Risk-Free Interest Rate (r, e.g. 0.05 for 5%): ", _ => true, "Please enter a valid rate.");
            double vol = ReadDouble("Volatility (sigma, e.g. 0.20 for 20% > 0): ", v => v > 0, "Volatility must be strictly positive.");
            double div = ReadOptionalDouble("Dividend Yield (q, e.g. 0.02 for 2%)", 0.0);

            try
            {
                var contract = new OptionContract(spot, strike, maturity, rate, vol, div, type);
                var greeks = GreeksCalculator.CalculateAll(contract);
                var pricer = new BlackScholesPricer();
                double price = pricer.Price(contract);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">>> Results & Greeks <<<");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Option Price : {price:F6}");
                Console.WriteLine($"Delta        : {greeks.Delta:F6}");
                Console.WriteLine($"Gamma        : {greeks.Gamma:F6}");
                Console.WriteLine($"Vega (1% vol): {greeks.Vega:F6}");
                Console.WriteLine($"Theta (1day) : {greeks.Theta:F6}");
                Console.WriteLine($"Rho (1% rate): {greeks.Rho:F6}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error calculating Greeks: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void SolveImpliedVolMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Solve Implied Volatility ---");
            Console.ResetColor();

            var type = ReadOptionType();
            double spot = ReadDouble("Spot Price (S > 0): ", s => s > 0, "Spot price must be strictly positive.");
            double strike = ReadDouble("Strike Price (K > 0): ", k => k > 0, "Strike price must be strictly positive.");
            double maturity = ReadDouble("Maturity in Years (T > 0): ", t => t > 0, "Maturity must be strictly positive.");
            double rate = ReadDouble("Risk-Free Interest Rate (r, e.g. 0.05 for 5%): ", _ => true, "Please enter a valid rate.");
            double div = ReadOptionalDouble("Dividend Yield (q, e.g. 0.02 for 2%)", 0.0);
            double mktPrice = ReadDouble("Observed Option Market Price: ", p => p >= 0, "Market price cannot be negative.");

            try
            {
                // We construct the contract with a dummy volatility (e.g. 0.20) because the solver
                // uses WithVolatility to evaluate pricing at different volatility guesses.
                var dummyVol = 0.20;
                var contract = new OptionContract(spot, strike, maturity, rate, dummyVol, div, type);

                var solver = new ImpliedVolatilitySolver();
                var result = solver.Solve(contract, mktPrice);

                Console.WriteLine();
                if (result.IsSuccessful)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(">>> Solving Successful <<<");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Implied Volatility : {result.ImpliedVolatility:P4} (or {result.ImpliedVolatility:F6})");
                    Console.WriteLine($"Iterations         : {result.Iterations}");
                    Console.WriteLine($"Solver Method      : {result.Message}");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(">>> Solving Failed <<<");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Iterations         : {result.Iterations}");
                    Console.WriteLine($"Reason             : {result.Message}");
                }
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error solving implied volatility: {ex.Message}");
                Console.ResetColor();
            }
        }

        static void RunSampleScenario()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Running Sample Scenario ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Parameters:");
            Console.WriteLine("  Spot = 100");
            Console.WriteLine("  Strike = 100");
            Console.WriteLine("  Maturity = 1.0 Year");
            Console.WriteLine("  Risk-free rate = 5% (0.05)");
            Console.WriteLine("  Volatility = 20% (0.20)");
            Console.WriteLine("  Dividend Yield = 0%");
            Console.WriteLine("  Option Type = Call");
            Console.WriteLine();

            try
            {
                var contract = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Call);
                var pricer = new BlackScholesPricer();
                double price = pricer.Price(contract);
                var greeks = GreeksCalculator.CalculateAll(contract);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">>> Scenario Results <<<");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Price         : {price:F6} (Expected: ~10.4506)");
                Console.WriteLine($"Delta         : {greeks.Delta:F6} (Expected: ~0.6368)");
                Console.WriteLine($"Gamma         : {greeks.Gamma:F6} (Expected: ~0.0188)");
                Console.WriteLine($"Vega (1%)     : {greeks.Vega:F6} (Expected: ~0.3752)");
                Console.WriteLine($"Theta (1 day) : {greeks.Theta:F6} (Expected: ~-0.0176)");
                Console.WriteLine($"Rho (1%)      : {greeks.Rho:F6} (Expected: ~0.5323)");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error during sample scenario: {ex.Message}");
                Console.ResetColor();
            }
        }

        #region User Input Helpers

        static double ReadDouble(string prompt, Func<double, bool> validator, string errorMessage)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(prompt);
                Console.ForegroundColor = ConsoleColor.White;
                string input = Console.ReadLine() ?? string.Empty;
                if (double.TryParse(input, out double result) && validator(result))
                {
                    return result;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorMessage);
                Console.ResetColor();
            }
        }

        static double ReadOptionalDouble(string prompt, double defaultValue)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{prompt} (default {defaultValue}): ");
            Console.ForegroundColor = ConsoleColor.White;
            string input = Console.ReadLine() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                return defaultValue;
            }
            while (true)
            {
                if (double.TryParse(input, out double result) && result >= 0)
                {
                    return result;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a valid non-negative number.");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{prompt} (default {defaultValue}): ");
                Console.ForegroundColor = ConsoleColor.White;
                input = Console.ReadLine() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(input))
                {
                    return defaultValue;
                }
            }
        }

        static OptionType ReadOptionType()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Option Type (C for Call, P for Put): ");
                Console.ForegroundColor = ConsoleColor.White;
                string input = Console.ReadLine()?.Trim().ToUpper() ?? string.Empty;
                if (input == "C" || input == "CALL")
                    return OptionType.Call;
                if (input == "P" || input == "PUT")
                    return OptionType.Put;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid option type. Please enter 'C' or 'P'.");
                Console.ResetColor();
            }
        }

        #endregion
    }
}
