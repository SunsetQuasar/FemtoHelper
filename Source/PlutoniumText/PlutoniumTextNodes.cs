using Celeste.Mod.FemtoHelper.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.FemtoHelper;

public static class PlutoniumTextNodes
{

    public class Node
    {

    }
    public class Text(string str) : Node
    {
        public readonly string StringText = str;
    }
    public class Counter(string k) : Node
    {
        public readonly string Key = k;
    }

    public class Slider(string k, bool t, int d) : Node
    {
        public readonly string Key = k;
        public readonly bool Truncate = t;
        public readonly int Decimals = d;
    }
    public class Flag(string k, string on, string off) : Node
    {
        public readonly string Key = k;
        public readonly string StrIfOn = on;
        public readonly string StrIfOff = off;
    }

    public class ExpressionAsFlag(string k, string on, string off) : Node
    {
        public string Exp = k;
        public string StrIfOn = on;
        public string StrIfOff = off;
    }
    public class ExpressionAsCounter(string k) : Node
    {
        public string Exp = k;
    }

    private static readonly string[] BigNumberNames = ["", "", "million", "billion", "trillion", "quadrillion", "quintillion", "sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion"];

    private static string LimitedDecimals(float f, int decimals)
    {
        if (decimals < 0) return f.ToString();

        string format = "0.";

        for (int i = 0; i < decimals; i++)
        {
            format += "0";
        }

        return f.ToString(format);
    }

    private static string GetShorthandNumber(float f)
    {
        if (f < 1000000) return f.ToString();
        int orderOfMagnitudeTriplets = (int)(MathF.Log10(f) / 3);
        double num = Math.Round(f / Math.Pow(10, 3 * orderOfMagnitudeTriplets), 3);
        string str = num.ToString();
        if (orderOfMagnitudeTriplets < BigNumberNames.Length - 1) return str + " " + BigNumberNames[orderOfMagnitudeTriplets];
        else return f.ToString();
    }

    public static List<Node> Parse(string dialogId, bool truncateSliders, int decimals)
    {
        List<Node> nodes = [];

        string[] splitString = [.. SimpleText.MyRegex().Split(Dialog.Get(dialogId)).Where(c => !string.IsNullOrEmpty(c))];

        for (int i = 0; i < splitString.Length; i++)
        {
            if (splitString[i] == "{")
            {
                i++;

                for (; i < splitString.Length && splitString[i] != "}"; i++)
                {
                    if (string.IsNullOrWhiteSpace(splitString[i])) continue;

                    string[] splitOnceAgain = splitString[i].Split(';');
                    if (splitOnceAgain.Length == 3)
                    {
                        nodes.Add(new Flag(splitOnceAgain[0], splitOnceAgain[1], splitOnceAgain[2]));
                    }
                    else if (splitOnceAgain.Length == 4 && splitOnceAgain[0] == "exp")
                    {
                        nodes.Add(new ExpressionAsFlag(splitOnceAgain[1], splitOnceAgain[2], splitOnceAgain[3]));
                    }
                    else if (splitOnceAgain.Length == 2 && splitOnceAgain[0] == "exp")
                    {
                        nodes.Add(new ExpressionAsCounter(splitOnceAgain[1]));
                    }
                    else
                    {
                        if (splitString[i][0] == '@')
                        {
                            nodes.Add(new Slider(splitString[i].Remove(0, 1), truncateSliders, decimals));
                        }
                        else
                        {
                            nodes.Add(new Counter(splitString[i]));
                        }
                    }
                }
            }
            else
            {
                nodes.Add(new Text(splitString[i]));
            }
        }

        return nodes;
    }

    public static string ConstructString(List<Node> nodes, Level level)
    {
        var result = "";
        foreach (var n in nodes)
        {
            switch (n)
            {
                case Text t:
                    result += t.StringText;
                    break;
                case Counter c:
                    result += level.Session.GetCounter(c.Key).ToString();
                    break;
                case Slider s:
                    result += s.Truncate ? GetShorthandNumber(level.Session.GetSlider(s.Key)) : LimitedDecimals(level.Session.GetSlider(s.Key), s.Decimals);
                    break;
                case Flag f:
                    result += level.Session.GetFlag(f.Key) ? f.StrIfOn : f.StrIfOff;
                    break;
                case ExpressionAsFlag ef:
                    result += EvaluateExpressionAsBool(ef.Exp, level.Session) ? ef.StrIfOn : ef.StrIfOff;
                    break;
                case ExpressionAsCounter ec:
                    result += EvaluateExpressionAsInt(ec.Exp, level.Session).ToString();
                    break;
            }
        }
        return result;
    }
}
