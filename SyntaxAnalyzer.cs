using System;
using System.Collections.Generic;
using System.Globalization;

namespace comp
{
    public class SyntaxAnalyzer
    {
        private readonly List<LexicalAnalyzer.Token> tokens;
        private int currentPos;
        private int tempCounter;
        private readonly InternalRepresentationResult result;

        public SyntaxAnalyzer(List<LexicalAnalyzer.Token> tokens)
        {
            this.tokens = tokens ?? new List<LexicalAnalyzer.Token>();
            currentPos = 0;
            tempCounter = 1;
            result = new InternalRepresentationResult();
        }

        public List<SyntaxError> Parse()
        {
            return AnalyzeProgram().Errors;
        }

        public InternalRepresentationResult AnalyzeProgram()
        {
            result.Errors.Clear();
            result.Tetrads.Clear();
            result.Poliz.Clear();
            result.Warning = "";
            result.PolizValue = null;
            currentPos = 0;
            tempCounter = 1;

            bool hasLexicalErrors = false;

            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].IsError)
                {
                    hasLexicalErrors = true;

                    result.Errors.Add(new SyntaxError
                    {
                        Fragment = tokens[i].Value,
                        Location = "строка " + tokens[i].Line + ", позиция " + (tokens[i].StartPos + 1),
                        Description = tokens[i].Type
                    });
                }
            }

            if (hasLexicalErrors)
            {
                result.Warning = "Есть лексические ошибки. Синтаксический анализ, тетрады и ПОЛИЗ не формируются.";
                return result;
            }

            while (currentPos < tokens.Count)
            {
                int line = Current().Line;
                List<LexicalAnalyzer.Token> expressionTokens = ReadExpressionLine(line);

                if (expressionTokens.Count == 0)
                    continue;

                AnalyzeExpression(expressionTokens, line);
            }

            if (result.Errors.Count > 0)
            {
                result.Tetrads.Clear();
                result.Poliz.Clear();
                result.PolizValue = null;
                result.Warning = "Есть лексические или синтаксические ошибки. Тетрады и ПОЛИЗ не формируются.";
            }

            return result;
        }

        private List<LexicalAnalyzer.Token> ReadExpressionLine(int line)
        {
            var lineTokens = new List<LexicalAnalyzer.Token>();
            bool semicolonFound = false;

            while (currentPos < tokens.Count && tokens[currentPos].Line == line)
            {
                LexicalAnalyzer.Token token = tokens[currentPos];

                if (semicolonFound)
                {
                    AddError(token, "конец выражения", "Лишняя лексема после ';'");
                    currentPos++;
                    continue;
                }

                if (IsSeparator(token))
                {
                    semicolonFound = true;
                    currentPos++;
                    continue;
                }

                lineTokens.Add(token);
                currentPos++;
            }

            return lineTokens;
        }

        private void AnalyzeExpression(List<LexicalAnalyzer.Token> expressionTokens, int line)
        {
            var parser = new ExpressionParser(expressionTokens, line, this, tempCounter);
            ExpressionParseResult expressionResult = parser.Parse();
            tempCounter = parser.TempCounter;

            if (expressionResult.Errors.Count > 0)
            {
                result.Errors.AddRange(expressionResult.Errors);
                return;
            }

            for (int i = 0; i < expressionResult.Tetrads.Count; i++)
                expressionResult.Tetrads[i].Number = result.Tetrads.Count + i + 1;

            result.Tetrads.AddRange(expressionResult.Tetrads);

            bool onlyIntegerNumbers = true;

            for (int i = 0; i < expressionTokens.Count; i++)
            {
                if (IsIdentifier(expressionTokens[i]))
                {
                    onlyIntegerNumbers = false;
                    break;
                }
            }

            if (onlyIntegerNumbers)
            {
                var polizBuilder = new PolizBuilder(expressionTokens);
                PolizResult polizResult = polizBuilder.BuildAndEvaluate();

                result.Poliz = polizResult.Poliz;
                result.PolizValue = polizResult.Value;

                if (polizResult.Errors.Count > 0)
                {
                    result.Warning = JoinPolizErrors(polizResult.Errors);
                }
                else
                {
                    result.Warning = "Выражение состоит только из целых чисел. ПОЛИЗ успешно построен и вычислен.";
                }
            }
            else
            {
                result.Poliz.Clear();
                result.PolizValue = null;
                result.Warning = "ПОЛИЗ с вычислением формируется только для выражений из целых чисел. Для выражений с id построены только тетрады.";
            }
        }

        private string JoinPolizErrors(List<SyntaxError> errors)
        {
            var messages = new List<string>();

            for (int i = 0; i < errors.Count; i++)
                messages.Add(errors[i].Description);

            return string.Join("; ", messages.ToArray());
        }

        private bool IsSeparator(LexicalAnalyzer.Token token)
        {
            return token.Code == (int)LexicalAnalyzer.TokenType.SEPARATOR && token.Value == ";";
        }

        private bool IsIdentifier(LexicalAnalyzer.Token token)
        {
            return token.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER;
        }

        private LexicalAnalyzer.Token Current()
        {
            return currentPos < tokens.Count ? tokens[currentPos] : null;
        }

        internal void AddError(LexicalAnalyzer.Token token, string expected, string description)
        {
            string fragment = token != null ? token.Value : "<конец строки>";
            string location = token != null
                ? "строка " + token.Line + ", позиция " + (token.StartPos + 1)
                : "конец ввода";

            result.Errors.Add(new SyntaxError
            {
                Fragment = fragment,
                Location = location,
                Description = description + ". Ожидалось: " + expected
            });
        }

        internal static bool IsNumber(LexicalAnalyzer.Token token)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.NUMBER;
        }

        internal static bool IsIdentifierToken(LexicalAnalyzer.Token token)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER;
        }

        internal static bool IsOperator(LexicalAnalyzer.Token token, string value)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.OPERATOR &&
                   token.Value == value;
        }

        internal static bool IsLeftParen(LexicalAnalyzer.Token token)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.LEFT_PAREN;
        }

        internal static bool IsRightParen(LexicalAnalyzer.Token token)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.RIGHT_PAREN;
        }

        private class ExpressionParser
        {
            private readonly List<LexicalAnalyzer.Token> expressionTokens;
            private readonly int line;
            private readonly SyntaxAnalyzer owner;
            private int pos;
            private readonly ExpressionParseResult parseResult;

            public int TempCounter { get; private set; }

            public ExpressionParser(
                List<LexicalAnalyzer.Token> expressionTokens,
                int line,
                SyntaxAnalyzer owner,
                int tempCounter)
            {
                this.expressionTokens = expressionTokens;
                this.line = line;
                this.owner = owner;
                TempCounter = tempCounter;
                pos = 0;
                parseResult = new ExpressionParseResult();
            }

            public ExpressionParseResult Parse()
            {
                if (expressionTokens.Count == 0)
                    return parseResult;

                ParseE();

                if (pos < expressionTokens.Count && parseResult.Errors.Count == 0)
                {
                    LexicalAnalyzer.Token token = Current();

                    if (IsRightParen(token))
                    {
                        parseResult.Errors.Add(CreateError(
                            token,
                            "конец выражения",
                            "Лишняя закрывающая скобка"));
                    }
                    else
                    {
                        parseResult.Errors.Add(CreateError(
                            token,
                            "конец выражения",
                            "Лишняя лексема"));
                    }
                }

                return parseResult;
            }

            private string ParseE()
            {
                string left = ParseT();
                return ParseA(left);
            }

            private string ParseA(string left)
            {
                while (IsOperator(Current(), "+") || IsOperator(Current(), "-"))
                {
                    string op = Current().Value;
                    pos++;

                    if (IsEnd() ||
                        IsOperator(Current(), "+") ||
                        IsOperator(Current(), "-") ||
                        IsOperator(Current(), "*") ||
                        IsOperator(Current(), "/") ||
                        IsRightParen(Current()))
                    {
                        parseResult.Errors.Add(CreateError(
                            Current(),
                            "операнд после '" + op + "'",
                            "Пропущен операнд"));

                        return left;
                    }

                    string right = ParseT();

                    if (parseResult.Errors.Count > 0)
                        return left;

                    left = AddTetrad(op, left, right);
                }

                return left;
            }

            private string ParseT()
            {
                string left = ParseF();
                return ParseB(left);
            }

            private string ParseB(string left)
            {
                while (IsOperator(Current(), "*") || IsOperator(Current(), "/"))
                {
                    string op = Current().Value;
                    pos++;

                    if (IsEnd() ||
                        IsOperator(Current(), "+") ||
                        IsOperator(Current(), "-") ||
                        IsOperator(Current(), "*") ||
                        IsOperator(Current(), "/") ||
                        IsRightParen(Current()))
                    {
                        parseResult.Errors.Add(CreateError(
                            Current(),
                            "операнд после '" + op + "'",
                            "Пропущен операнд"));

                        return left;
                    }

                    string right = ParseF();

                    if (parseResult.Errors.Count > 0)
                        return left;

                    left = AddTetrad(op, left, right);
                }

                return left;
            }

            private string ParseF()
            {
                LexicalAnalyzer.Token token = Current();

                if (IsNumber(token) || IsIdentifierToken(token))
                {
                    pos++;
                    return token.Value;
                }

                if (IsLeftParen(token))
                {
                    pos++;

                    if (IsRightParen(Current()))
                    {
                        parseResult.Errors.Add(CreateError(
                            Current(),
                            "выражение внутри скобок",
                            "Пустые скобки"));

                        pos++;
                        return "?";
                    }

                    string value = ParseE();

                    if (IsRightParen(Current()))
                    {
                        pos++;
                        return value;
                    }

                    parseResult.Errors.Add(CreateError(
                        Current(),
                        "')'",
                        "Не закрыта скобка"));

                    return value;
                }

                if (IsRightParen(token))
                {
                    parseResult.Errors.Add(CreateError(
                        token,
                        "операнд или '('",
                        "Лишняя закрывающая скобка"));

                    pos++;
                    return "?";
                }

                parseResult.Errors.Add(CreateError(
                    token,
                    "num, id или '('",
                    "Пропущен операнд"));

                if (!IsEnd())
                    pos++;

                return "?";
            }

            private string AddTetrad(string op, string arg1, string arg2)
            {
                string temp = "t" + TempCounter.ToString(CultureInfo.InvariantCulture);
                TempCounter++;

                parseResult.Tetrads.Add(new Tetrad
                {
                    Number = parseResult.Tetrads.Count + 1,
                    Operation = op,
                    Arg1 = arg1,
                    Arg2 = arg2,
                    Result = temp
                });

                return temp;
            }

            private LexicalAnalyzer.Token Current()
            {
                return pos < expressionTokens.Count ? expressionTokens[pos] : null;
            }

            private bool IsEnd()
            {
                return pos >= expressionTokens.Count;
            }

            private SyntaxError CreateError(
                LexicalAnalyzer.Token token,
                string expected,
                string description)
            {
                string fragment = token != null ? token.Value : "<конец выражения>";
                string location = token != null
                    ? "строка " + token.Line + ", позиция " + (token.StartPos + 1)
                    : "строка " + line + ", конец выражения";

                return new SyntaxError
                {
                    Fragment = fragment,
                    Location = location,
                    Description = description + ". Ожидалось: " + expected
                };
            }
        }

        private class PolizBuilder
        {
            private readonly List<LexicalAnalyzer.Token> expressionTokens;
            private readonly List<SyntaxError> errors;

            public PolizBuilder(List<LexicalAnalyzer.Token> expressionTokens)
            {
                this.expressionTokens = expressionTokens;
                errors = new List<SyntaxError>();
            }

            public PolizResult BuildAndEvaluate()
            {
                var output = new List<string>();
                var operations = new Stack<LexicalAnalyzer.Token>();

                for (int i = 0; i < expressionTokens.Count; i++)
                {
                    LexicalAnalyzer.Token token = expressionTokens[i];

                    if (IsNumber(token))
                    {
                        output.Add(token.Value);
                        continue;
                    }

                    if (IsIdentifierToken(token))
                    {
                        errors.Add(CreateError(
                            token,
                            "целое число",
                            "ПОЛИЗ с вычислением поддерживает только целые числа"));

                        continue;
                    }

                    if (IsOperatorToken(token))
                    {
                        while (operations.Count > 0 &&
                               IsOperatorToken(operations.Peek()) &&
                               Priority(operations.Peek().Value) >= Priority(token.Value))
                        {
                            output.Add(operations.Pop().Value);
                        }

                        operations.Push(token);
                        continue;
                    }

                    if (IsLeftParen(token))
                    {
                        operations.Push(token);
                        continue;
                    }

                    if (IsRightParen(token))
                    {
                        while (operations.Count > 0 && !IsLeftParen(operations.Peek()))
                        {
                            output.Add(operations.Pop().Value);
                        }

                        if (operations.Count == 0)
                        {
                            errors.Add(CreateError(
                                token,
                                "'(' перед ')'",
                                "Лишняя закрывающая скобка"));
                        }
                        else
                        {
                            operations.Pop();
                        }
                    }
                }

                while (operations.Count > 0)
                {
                    LexicalAnalyzer.Token op = operations.Pop();

                    if (IsLeftParen(op))
                    {
                        errors.Add(CreateError(
                            op,
                            "')'",
                            "Не закрыта скобка"));
                    }
                    else
                    {
                        output.Add(op.Value);
                    }
                }

                int? value = null;

                if (errors.Count == 0)
                    value = Evaluate(output);

                return new PolizResult
                {
                    Poliz = output,
                    Value = value,
                    Errors = errors
                };
            }

            private int? Evaluate(List<string> poliz)
            {
                var stack = new Stack<int>();

                for (int i = 0; i < poliz.Count; i++)
                {
                    int number;

                    if (int.TryParse(poliz[i], out number))
                    {
                        stack.Push(number);
                        continue;
                    }

                    if (stack.Count < 2)
                    {
                        errors.Add(new SyntaxError
                        {
                            Fragment = poliz[i],
                            Location = "ПОЛИЗ",
                            Description = "Недостаточно операндов для операции '" + poliz[i] + "'"
                        });

                        return null;
                    }

                    int right = stack.Pop();
                    int left = stack.Pop();

                    switch (poliz[i])
                    {
                        case "+":
                            stack.Push(left + right);
                            break;

                        case "-":
                            stack.Push(left - right);
                            break;

                        case "*":
                            stack.Push(left * right);
                            break;

                        case "/":
                            if (right == 0)
                            {
                                errors.Add(new SyntaxError
                                {
                                    Fragment = "/",
                                    Location = "ПОЛИЗ",
                                    Description = "Деление на ноль при вычислении ПОЛИЗ"
                                });

                                return null;
                            }

                            stack.Push(left / right);
                            break;
                    }
                }

                if (stack.Count != 1)
                {
                    errors.Add(new SyntaxError
                    {
                        Fragment = string.Join(" ", poliz.ToArray()),
                        Location = "ПОЛИЗ",
                        Description = "Ошибка вычисления: после обработки остались лишние значения"
                    });

                    return null;
                }

                return stack.Pop();
            }

            private int Priority(string op)
            {
                if (op == "*" || op == "/")
                    return 2;

                if (op == "+" || op == "-")
                    return 1;

                return 0;
            }

            private bool IsOperatorToken(LexicalAnalyzer.Token token)
            {
                return token != null &&
                       token.Code == (int)LexicalAnalyzer.TokenType.OPERATOR;
            }

            private SyntaxError CreateError(
                LexicalAnalyzer.Token token,
                string expected,
                string description)
            {
                return new SyntaxError
                {
                    Fragment = token != null ? token.Value : "<конец>",
                    Location = token != null
                        ? "строка " + token.Line + ", позиция " + (token.StartPos + 1)
                        : "конец ввода",
                    Description = description + ". Ожидалось: " + expected
                };
            }
        }
    }

    public class InternalRepresentationResult
    {
        public List<SyntaxError> Errors { get; set; }
        public List<Tetrad> Tetrads { get; set; }
        public List<string> Poliz { get; set; }
        public int? PolizValue { get; set; }
        public string Warning { get; set; }

        public InternalRepresentationResult()
        {
            Errors = new List<SyntaxError>();
            Tetrads = new List<Tetrad>();
            Poliz = new List<string>();
            Warning = "";
        }
    }

    public class ExpressionParseResult
    {
        public List<SyntaxError> Errors { get; set; }
        public List<Tetrad> Tetrads { get; set; }

        public ExpressionParseResult()
        {
            Errors = new List<SyntaxError>();
            Tetrads = new List<Tetrad>();
        }
    }

    public class PolizResult
    {
        public List<string> Poliz { get; set; }
        public int? Value { get; set; }
        public List<SyntaxError> Errors { get; set; }
    }

    public class Tetrad
    {
        public int Number { get; set; }
        public string Operation { get; set; }
        public string Arg1 { get; set; }
        public string Arg2 { get; set; }
        public string Result { get; set; }
    }

    public class SyntaxError
    {
        public string Fragment { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }
}