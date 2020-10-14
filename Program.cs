using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MatchSolver
{
    enum DigitType
    {
        Number, Symbol
    }

    public static class MatchDigitsStore
    {
        public static List<List<bool>> store = new List<List<bool>>
        {
            new List<bool> { true, true, true, false, true, true, true },
            new List<bool> { false, false, true, false, false, true, false },
            new List<bool> { true, false, true, true, true, false, true },
            new List<bool> { true, false, true, true, false, true, true },
            new List<bool> { false, true, true, true, false, true, false },
            new List<bool> { true, true, false, true, false, true, true },
            new List<bool> { true, true, false, true, true, true, true },
            new List<bool> { true, false, true, false, false, true, false },
            new List<bool> { true, true, true, true, true, true, true },
            new List<bool> { true, true, true, true, false, true, true }
        };
    };

    class MatchDigit
    {
        public MatchDigit(int digit)
        {
            MatchPositions = new List<bool>(MatchDigitsStore.store[digit].ToArray());
            DigitType = DigitType.Number;
        }

        public MatchDigit(char symbol)
        {
            try
            {
                MatchPositions = symbol switch
                {
                    '-' => new List<bool> { true, false, false },
                    '=' => new List<bool> { true, true, false },
                    '+' => new List<bool> { true, false, true },
                    _ => throw new InvalidOperationException("Wrong symbol found while processing equation: " + symbol),
                };
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Environment.Exit(-1);
            }
            DigitType = DigitType.Symbol;
        }

        public DigitType DigitType { get; set; }
        public List<bool> MatchPositions { get; set; }

        public int ToInteger()
        {
            if (DigitType == DigitType.Symbol)
            {
                throw new InvalidOperationException("Cannot get integer value for a symbol.");
            }
            foreach (List<bool> digitPositions in MatchDigitsStore.store)
            {
                if (MatchPositions.SequenceEqual(digitPositions))
                {
                    return MatchDigitsStore.store.IndexOf(digitPositions);
                }
            }
            return -1;
        }

        public char ToSymbol()
        {
            if (DigitType == DigitType.Number)
            {
                throw new InvalidOperationException("Cannot get char value for a number.");
            }
            if (MatchPositions.SequenceEqual(new List<bool> { true, false, false }))
            {
                return '-';
            }
            else if (MatchPositions.SequenceEqual(new List<bool> { true, true, false }))
            {
                return '=';
            }
            else if (MatchPositions.SequenceEqual(new List<bool> { true, false, true }))
            {
                return '+';
            }
            else
            {
                return 'i';
            }
        }

        public override string ToString()
        {
            if (DigitType == DigitType.Number)
            {
                return ToInteger().ToString();
            }
            else
            {
                if (ToSymbol() == 'i')
                {
                    return "-1";
                }
                else
                {
                    return ToSymbol().ToString();
                }
            }
        }

        internal bool IsValid()
        {
            if (DigitType == DigitType.Number)
            {
                return ToInteger() != -1;
            }
            else
            {
                return ToSymbol() != 'i';
            }
        }
    }

    class Program
    {
        static string inputEquation = "";
        static bool verbose = false;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: MathSolver <equation> [--verbose]");
                Environment.Exit(-1);
            }
            else
            {
                var argsList = args.ToList();
                if (argsList.Contains("--verbose"))
                {
                    verbose = true;
                    argsList.Remove("--verbose");
                }
                inputEquation = argsList.FirstOrDefault();
            }
            if (String.IsNullOrWhiteSpace(inputEquation))
            {
                Console.WriteLine("An equation is required.");
                Environment.Exit(-1);
            }
            
            List<MatchDigit> origEquation = ConstructEquation(inputEquation);
            for (int firstDigitIndex = 0; firstDigitIndex < origEquation.Count; firstDigitIndex++)
            {
                MatchDigit matchDigitToRemoveFrom = origEquation[firstDigitIndex];


                for (int i = 0; i < matchDigitToRemoveFrom.MatchPositions.Count; i++)
                {
                    if (matchDigitToRemoveFrom.MatchPositions[i] == true)
                    {
                        matchDigitToRemoveFrom.MatchPositions[i] = false;
                        for (int secondDigitIndex = 0; secondDigitIndex < origEquation.Count; secondDigitIndex++)
                        {
                            MatchDigit matchDigitToAddTo = origEquation[secondDigitIndex];


                            for (int j = 0; j < matchDigitToAddTo.MatchPositions.Count; j++)
                            {
                                if (i==j)
                                {
                                    continue;
                                }
                                if (matchDigitToAddTo.MatchPositions[j] == false)
                                {
                                    matchDigitToAddTo.MatchPositions[j] = true;
                                    if (matchDigitToAddTo.IsValid() && matchDigitToRemoveFrom.IsValid())
                                    {
                                        if (verbose)
                                        {
                                            Console.WriteLine(PrintEquation(origEquation, firstDigitIndex, i, secondDigitIndex, j));
                                        }
                                        if (EvaluateEquation(origEquation))
                                        {
                                            Console.WriteLine("SOLVED: " + PrintEquation(origEquation, firstDigitIndex, i, secondDigitIndex, j));
                                        }
                                    }
                                    matchDigitToAddTo.MatchPositions[j] = false;
                                }
                            }
                        }
                        matchDigitToRemoveFrom.MatchPositions[i] = true;
                    }
                }
            }

            Console.WriteLine("Done.");
        }

        private static List<MatchDigit> ConstructEquation(string inputEquation)
        {
            List<MatchDigit> equation = new List<MatchDigit>(inputEquation.Length);
            foreach (char character in inputEquation)
            {
                if (Char.IsDigit(character))
                {
                    equation.Add(new MatchDigit(Convert.ToInt32(character.ToString())));
                }
                else
                {
                    equation.Add(new MatchDigit(character));
                }
            }
            return equation;
        }

        static bool EvaluateEquation(List<MatchDigit> origEquation)
        {
            List<int> equationSubs = new List<int>();
            int currentValue = 0;
            char currentSymbol = '+';
            for (int i = 0; i < origEquation.Count; i++)
            {
                MatchDigit currentDigit = origEquation[i];
                if (currentDigit.DigitType == DigitType.Number)
                {
                    if (currentSymbol == '+')
                    {
                        currentValue += currentDigit.ToInteger(); 
                    }
                    else if (currentSymbol == '-')
                    {
                        currentValue -= currentDigit.ToInteger();
                    }
                    else
                    {
                        equationSubs.Add(currentValue);
                        currentValue = currentDigit.ToInteger();
                    }
                }
                else
                {
                    currentSymbol = currentDigit.ToSymbol();
                }
            }
            equationSubs.Add(currentValue);

            if (equationSubs.Count < 2)
            {
                return false;
            }
            foreach (int subExpValue in equationSubs)
            {
                if (subExpValue != equationSubs.First())
                {
                    return false;
                }
            }
            return true;
        }

        static string PrintEquation(List<MatchDigit> equation, int fromDigit, int fromPos, int toDigit, int toPos)
        {
            StringBuilder sb = new StringBuilder($"Moved from {fromDigit} position {fromPos} to {toDigit} position {toPos} : ");
            foreach (MatchDigit digit in equation)
            {
                sb.Append(digit);
            }
            return sb.ToString();
        }
    }
}
