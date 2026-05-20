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

                string? choice = Console.ReadLine();
                if (choice == null)
                {
                    running = false;
                    break;
                }
                choice = choice.Trim();
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
                    if (!Console.IsInputRedirected)
                    {
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.ReadLine();
                    }
                }
            }
        }

        static void DisplayHeader()
        {
            try
            {
                Console.Clear();
            }
            catch (System.IO.IOException)
            {
                // Suppress error when run in redirected/non-interactive consoles
            }
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
            Console.WriteLine("   Option Pricing & Analytics Engine (.NET 8)");
            Console.WriteLine();
            Console.ResetColor();
        }

        static void PriceOptionMenu()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("--- Price Option ---");
            Console.ResetColor();

            var style = ReadOptionStyle();
            var type = ReadOptionType();
            double spot = ReadDouble("Spot Price (S > 0): ", s => s > 0, "Spot price must be strictly positive.");
            double strike = ReadDouble("Strike Price (K > 0): ", k => k > 0, "Strike price must be strictly positive.");
            double maturity = ReadDouble("Maturity in Years (T > 0): ", t => t > 0, "Maturity must be strictly positive.");
            double rate = ReadDouble("Risk-Free Interest Rate (r, e.g. 0.05 for 5%): ", _ => true, "Please enter a valid rate.");
            double vol = ReadDouble("Volatility (sigma, e.g. 0.20 for 20% > 0): ", v => v > 0, "Volatility must be strictly positive.");
            double div = ReadOptionalDouble("Dividend Yield (q, e.g. 0.02 for 2%)", 0.0);

            try
            {
                var contract = new OptionContract(spot, strike, maturity, rate, vol, div, type, style);
                var pricer = ReadPricingEngine();
                double price = pricer.Price(contract);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($">>> {style} {type} Price using {pricer.GetType().Name}: {price:F6}");
                
                if (pricer is MonteCarloPricer mcPricer)
                {
                    Console.Write($" (Std Error: ±{mcPricer.LastStandardError:F6})");
                }
                Console.WriteLine();
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

            Console.WriteLine("Note: Greeks are computed using analytical formulas and require European style.");
            var type = ReadOptionType();
            double spot = ReadDouble("Spot Price (S > 0): ", s => s > 0, "Spot price must be strictly positive.");
            double strike = ReadDouble("Strike Price (K > 0): ", k => k > 0, "Strike price must be strictly positive.");
            double maturity = ReadDouble("Maturity in Years (T > 0): ", t => t > 0, "Maturity must be strictly positive.");
            double rate = ReadDouble("Risk-Free Interest Rate (r, e.g. 0.05 for 5%): ", _ => true, "Please enter a valid rate.");
            double vol = ReadDouble("Volatility (sigma, e.g. 0.20 for 20% > 0): ", v => v > 0, "Volatility must be strictly positive.");
            double div = ReadOptionalDouble("Dividend Yield (q, e.g. 0.02 for 2%)", 0.0);

            try
            {
                var contract = new OptionContract(spot, strike, maturity, rate, vol, div, type, OptionStyle.European);
                var greeks = GreeksCalculator.CalculateAll(contract);
                var pricer = new BlackScholesPricer();
                double price = pricer.Price(contract);

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">>> Results & Analytical Greeks <<<");
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

            Console.WriteLine("Note: Implied volatility solving is supported for European style options.");
            var type = ReadOptionType();
            double spot = ReadDouble("Spot Price (S > 0): ", s => s > 0, "Spot price must be strictly positive.");
            double strike = ReadDouble("Strike Price (K > 0): ", k => k > 0, "Strike price must be strictly positive.");
            double maturity = ReadDouble("Maturity in Years (T > 0): ", t => t > 0, "Maturity must be strictly positive.");
            double rate = ReadDouble("Risk-Free Interest Rate (r, e.g. 0.05 for 5%): ", _ => true, "Please enter a valid rate.");
            double div = ReadOptionalDouble("Dividend Yield (q, e.g. 0.02 for 2%)", 0.0);
            double mktPrice = ReadDouble("Observed Option Market Price: ", p => p >= 0, "Market price cannot be negative.");

            try
            {
                var dummyVol = 0.20;
                var contract = new OptionContract(spot, strike, maturity, rate, dummyVol, div, type, OptionStyle.European);

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
            Console.WriteLine("--- Running Sample Scenario (Comparison & American Premium) ---");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Base Parameters (European Call):");
            Console.WriteLine("  Spot = 100, Strike = 100, Maturity = 1.0 Year, Rate = 5%, Vol = 20%, Div = 0%");
            Console.WriteLine();

            try
            {
                // 1. Compare European pricing across models
                var euroCall = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Call, OptionStyle.European);
                
                var bsPricer = new BlackScholesPricer();
                var binPricer = new BinomialTreePricer(200);
                var mcPricer = new MonteCarloPricer(100000, 42);

                double bsPrice = bsPricer.Price(euroCall);
                double binPrice = binPricer.Price(euroCall);
                double mcPrice = mcPricer.Price(euroCall);
                double mcErr = mcPricer.LastStandardError;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(">>> European Call Pricing Comparison <<<");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {"Pricing Model",-30} | {"Price",-10} | {"Difference vs BSM",-18}");
                Console.WriteLine(new string('-', 68));
                Console.WriteLine($"  {"Black-Scholes (Analytical)",-30} | {bsPrice,-10:F6} | {"Benchmark",-18}");
                Console.WriteLine($"  {"Binomial Tree (CRR, 200 steps)",-30} | {binPrice,-10:F6} | {binPrice - bsPrice,18:F6}");
                Console.WriteLine($"  {"Monte Carlo (100k paths)",-30} | {mcPrice,-10:F6} | {mcPrice - bsPrice,18:F6} (SE: ±{mcErr:F6})");
                Console.WriteLine();

                // 2. American Premium on Put option (where early exercise is prominent)
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("American Early Exercise Premium Demonstration:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("  Put Parameters: Spot = 100, Strike = 100, Maturity = 1.0 Year, Rate = 5%, Vol = 20%, Div = 0%");
                
                var euroPut = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Put, OptionStyle.European);
                var amerPut = new OptionContract(100, 100, 1.0, 0.05, 0.20, 0.0, OptionType.Put, OptionStyle.American);

                double priceEuroPut = bsPricer.Price(euroPut);
                double priceAmerPut = binPricer.Price(amerPut);
                double premium = priceAmerPut - priceEuroPut;

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n>>> American Put Premium Results <<<");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  European Put Price (Black-Scholes) : {priceEuroPut:F6}");
                Console.WriteLine($"  American Put Price (Binomial CRR)  : {priceAmerPut:F6}");
                Console.WriteLine($"  American Early Exercise Premium    : {premium:F6} ({premium / priceEuroPut:P2})");
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
                string? input = Console.ReadLine();
                if (input == null)
                    throw new OperationCanceledException("Console input stream was closed.");

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
            string? input = Console.ReadLine();
            if (input == null)
                throw new OperationCanceledException("Console input stream was closed.");

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
                input = Console.ReadLine();
                if (input == null)
                    throw new OperationCanceledException("Console input stream was closed.");

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
                string? input = Console.ReadLine();
                if (input == null)
                    throw new OperationCanceledException("Console input stream was closed.");

                string trimmed = input.Trim().ToUpper();
                if (trimmed == "C" || trimmed == "CALL")
                    return OptionType.Call;
                if (trimmed == "P" || trimmed == "PUT")
                    return OptionType.Put;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid option type. Please enter 'C' or 'P'.");
                Console.ResetColor();
            }
        }

        static OptionStyle ReadOptionStyle()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("Option Style (E for European, A for American): ");
                Console.ForegroundColor = ConsoleColor.White;
                string? input = Console.ReadLine();
                if (input == null)
                    throw new OperationCanceledException("Console input stream was closed.");

                string trimmed = input.Trim().ToUpper();
                if (trimmed == "E" || trimmed == "EUROPEAN")
                    return OptionStyle.European;
                if (trimmed == "A" || trimmed == "AMERICAN")
                    return OptionStyle.American;

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid option style. Please enter 'E' or 'A'.");
                Console.ResetColor();
            }
        }

        static IPricer ReadPricingEngine()
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Select Pricing Model:");
                Console.WriteLine("  1. Black-Scholes-Merton (Analytical)");
                Console.WriteLine("  2. Binomial Tree (Cox-Ross-Rubinstein)");
                Console.WriteLine("  3. Monte Carlo Simulation");
                Console.Write("Choice (1-3): ");
                Console.ForegroundColor = ConsoleColor.White;
                string? input = Console.ReadLine();
                if (input == null)
                    throw new OperationCanceledException("Console input stream was closed.");

                string trimmed = input.Trim();
                if (trimmed == "1")
                    return new BlackScholesPricer();
                if (trimmed == "2")
                    return new BinomialTreePricer();
                if (trimmed == "3")
                    return new MonteCarloPricer();

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid pricing model choice. Please select 1, 2, or 3.");
                Console.ResetColor();
            }
        }

        #endregion
    }
}
