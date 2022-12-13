using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Calculator
{
    class CalculatorImplementer
    {
        // todo: Надо бы все это обернуть в класс и написать утилитные функции
        private string validNumers = "0123456789.";
        private string validBraces = "()";
        private string validOperators = "+-*/^";

        // Получаем обратную польскую формулу
        public string GetRPNFormula(string formula)
        {
            if (!IsValidFormula(formula))
                return "";

            string result = "";
            List<string> list = GetRPN(formula);
            foreach (var it in list)
            {
                result += it + "'";
            }
            return result;
        }

        // Получем результат
        // Получаем в виде обратной польской натации и теперь нам удобно вычислиить значение через срэк
        public string GetResult(string formula)
        {
            if (!IsValidFormula(formula))
                return "";

            Stack<decimal> result = new Stack<decimal>();
            List<string> list = GetRPN(formula);
            foreach (var it in list)
            {
                if (validOperators.Contains(it) && result.Count > 1)
                {
                    decimal right = result.Pop();
                    decimal left = result.Pop();
                    result.Push(MakeOperation(left, right, it));
                }
                else if (validNumers.Contains(it[it.Length - 1]))
                {
                    var val = decimal.Parse(it, new NumberFormatInfo() { NumberDecimalSeparator = "." });
                    result.Push(val);
                }
            }

            return result.Pop().ToString();
        }

        private decimal MakeOperation(decimal left, decimal right, string operation)
        {
            decimal result = 0;

            if (operation == "+")
                result = left + right;
            else if (operation == "-")
                result = left - right;
            else if (operation == "*")
                result = left * right;
            else if (operation == "/")
            {
                if (right == 0)
                    return 0;
                result = left / right;
            }
            else if (operation == "^")
            {
                double dl = (double)left;
                double dr = (double)right;
                double r;
                if (dl > 0)
                {
                    r = Math.Pow(dl, dr);
                }
                else
                {
                    r = -1 * Math.Pow(-dl, dr);
                }
                result = (decimal)r;
            }
            return result;
        }

        // Проверка на валидность
        // todo: убрать в оттдельный класс и все вызовы из неё
        private bool IsValidFormula(string formula)
        {
            if (formula == null || formula.Length == 0)
                return false;

            return ChekValidCharacters(formula)
                && ChekValidBraces(formula)
                && ChekValidDots(formula)
                && ChekValidOperators(formula);
        }

        // Обратная полская натация 
        private List<string> GetRPN(string formula)
        {
            List<string> result = new List<string>();
            Stack<string> interim = new Stack<string>();
            bool isNumBefore = false;
            bool vateSign = true;
            foreach (char c in formula)
            {
                if (vateSign && (c == '+' || c == '-'))
                {
                    if (c == '-')
                    {
                        result.Add(c.ToString());
                        isNumBefore = true;
                    }
                }
                else if (validNumers.Contains(c))
                {
                    if (isNumBefore)
                        result[result.Count - 1] += c.ToString();
                    else
                        result.Add(c.ToString());
                    isNumBefore = true;
                }
                else
                {
                    isNumBefore = false;
                }

                if (')' == c)
                {
                    while (interim.Count > 0 && interim.Peek() != "(")
                    {
                        result.Add(interim.Pop());
                    }

                    if (interim.Count > 0)
                        interim.Pop();
                }

                if (!vateSign && validOperators.Contains(c))
                {
                    if (interim.Count != 0)
                    {
                        while (OperatorPriority(interim.Peek()) >= OperatorPriority(c.ToString()))
                        {
                            result.Add(interim.Pop());
                            if (interim.Count == 0)
                                break;
                        }
                    }
                    interim.Push(c.ToString());
                }

                if ('(' == c)
                {
                    interim.Push(c.ToString());
                    vateSign = true;
                }
                else
                {
                    vateSign = false;
                }
            }

            while (interim.Count != 0)
            {
                if (!validBraces.Contains(interim.Peek()))
                    result.Add(interim.Pop());
                else
                    interim.Pop();
            }
            return result;
        }

        // Проверка на валидные символы 
        private bool ChekValidCharacters(string formula)
        {
            if (formula == null)
                return false;

            bool bad = false;
            foreach (char c in formula)
            {
                bad = !validNumers.Contains(c);
                bad = !bad ? bad : !validBraces.Contains(c);
                bad = !bad ? bad : !validOperators.Contains(c);

                if (bad)
                    return false;
            }

            return !bad;
        }

        // Проверка на правельно колличество скобок
        private bool ChekValidBraces(string formula)
        {
            Stack<string> braces = new Stack<string>();
            int bOp = 0;
            for (int i = 0; i < formula.Length; i++)
            {
                var c = formula[i];
                if (c == '(')
                {
                    if (i > 0 && !validOperators.Contains(formula[i - 1]))
                        return false;

                    braces.Push(c.ToString());
                    bOp = i;
                }
                else if (c == ')')
                {
                    if (braces.Count == 0)
                        return false;
                    if (bOp == i - 1)
                        return false;

                    braces.Pop();
                }
            }
            return braces.Count == 0;
        }

        // Провека правильности использования точек в числах
        private bool ChekValidDots(string formula)
        {
            if (formula == ".")
                return false;

            bool dot = false;
            for (int i = 0; i < formula.Length; i++)
            {
                var c = formula[i];

                if (c == '.')
                {
                    if (dot)
                        return false; // в числе не может быть двух точек

                    bool leftHaveNum = (i > 0 && validNumers.Contains(formula[i - 1]));
                    bool rightHaveNum = (i + 1 < formula.Length && validNumers.Contains(formula[i + 1]));
                    if (!leftHaveNum && !rightHaveNum)
                        return false; // у точки должны быть чисал спарав или слева

                    dot = true;
                }
                else if (validBraces.Contains(c) || validOperators.Contains(c))
                {
                    dot = false;
                }
            }
            return true;
        }

        // Проверка правильности использования операторов
        private bool ChekValidOperators(string formula)
        {
            if (formula.Length > 0 && validOperators.Contains(formula[formula.Length - 1]))
                return false; // формула не может заканчиватся оператором

            for (int i = 0; i < formula.Length; i++)
            {
                var c = formula[i];

                if (validOperators.Contains(c))
                {
                    if (i + 1 < formula.Length && validOperators.Contains(formula[i + 1]))
                        return false; // формула не может содержать два оператора подряд

                    if (i + 1 < formula.Length && formula[i + 1] == ')')
                        return false; // оператор не может быть перед закрывающейся скобкой
                }
            }



            return true;
        }

        // Приоритет операдндов 
        private int OperatorPriority(string c)
        {
            if (c == "(")
                return 0;
            else if (c == ")")
                return 1;
            else if (c == "+" || c == "-")
                return 2;
            else if (c == "*" || c == "/")
                return 3;
            else if (c == "^")
                return 4;

            return 0;
        }
    }
}
