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
        //public static Dictionary<int, List<bool>> store = new Dictionary<int, List<bool>>
        //{
        //    { 0, new List<bool> { true, true, true, false, true, true, true } },
        //    { 1, new List<bool> { false, false, true, false, false, true, false } },
        //    { 2, new List<bool> { true, false, true, true, true, false, true } },
        //    { 3, new List<bool> { true, false, true, true, false, true, true } },
        //    { 4, new List<bool> { false, true, true, true, false, true, false } },
        //    { 5, new List<bool> { true, true, false, true, false, true, true } },
        //    { 6, new List<bool> { true, true, false, true, true, true, true } },
        //    { 7, new List<bool> { true, false, true, false, false, true, false } },
        //    { 8, new List<bool> { true, true, true, true, true, true, true } },
        //    { 9, new List<bool> { true, true, true, true, false, true, true } },
        //};

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
            MatchPositions = symbol switch
            {
                '-' => new List<bool> { true, false, false },
                '=' => new List<bool> { true, true, false },
                '+' => new List<bool> { true, false, true },
                _ => throw new InvalidOperationException("Wrong char for symbol type MatchDigit object."),
            };
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
        static void Main(string[] args)
        {
            List<MatchDigit> origEquation = new List<MatchDigit> { new MatchDigit(2), new MatchDigit('-'), new MatchDigit(2), new MatchDigit('+'), new MatchDigit(8), new MatchDigit('='), new MatchDigit(2) };
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
                                        if (args.Length == 1 && args[0] == "--verbose")
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
            Console.ReadLine();
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
                        //currentSymbol = '+';
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
