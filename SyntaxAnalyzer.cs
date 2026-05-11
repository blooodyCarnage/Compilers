using System;
using System.Collections.Generic;

namespace comp
{
    public class SyntaxAnalyzer
    {
        private List<LexicalAnalyzer.Token> tokens;
        private int currentPos;
        private int currentLineEnd;
        private int currentLineNumber;
        private List<SyntaxError> errors;

        public SyntaxAnalyzer(List<LexicalAnalyzer.Token> tokens)
        {
            this.tokens = tokens;
            currentPos = 0;
            currentLineEnd = 0;
            currentLineNumber = 1;
            errors = new List<SyntaxError>();
        }

        public List<SyntaxError> Parse()
        {
            while (currentPos < tokens.Count)
            {
                currentLineNumber = tokens[currentPos].Line;
                currentLineEnd = FindLineEnd(currentPos);

                int startPos = currentPos;

                ParseDeclaration();

                if (currentPos < currentLineEnd)
                {
                    AddError("конец объявления", CurrentToken());
                    currentPos = currentLineEnd;
                }

                if (currentPos == startPos)
                    currentPos++;
            }

            return errors;
        }

        private void ParseDeclaration()
        {
            MatchKeyword("DECLARE", NextAfter_DECLARE);
            MatchIdentifier();
            MatchKeyword("CONSTANT", NextAfter_CONSTANT);
            MatchKeyword("INTEGER", NextAfter_INTEGER);
            MatchOperator(":=");
            MatchNumber();
            MatchSemicolon();
        }

        private int FindLineEnd(int start)
        {
            int line = tokens[start].Line;
            int pos = start;

            while (pos < tokens.Count && tokens[pos].Line == line)
                pos++;

            return pos;
        }

        private void MatchKeyword(string expected, Func<LexicalAnalyzer.Token, bool> isNextElement)
        {
            if (IsKeyword(expected))
            {
                currentPos++;
                return;
            }

            if (currentPos >= currentLineEnd)
            {
                AddError($"'{expected}'", CurrentToken());
                return;
            }

            var current = CurrentToken();

            if (LooksLikeKeyword(current.Value, expected))
            {
                AddKeywordError(expected, current);
                currentPos++;
                return;
            }

            AddError($"'{expected}'", current);

            if (isNextElement(current))
                return;

            currentPos++;
        }

        private void MatchIdentifier()
        {
            if (IsIdentifier())
            {
                currentPos++;
                return;
            }

            AddError("идентификатор", CurrentToken());

            if (currentPos >= currentLineEnd)
                return;

            if (IsKeyword("CONSTANT"))
                return;

            SkipUntil(t => IsKeywordAt(t, "CONSTANT"));
        }

        private void MatchOperator(string expected)
        {
            if (IsOperator(expected))
            {
                currentPos++;
                return;
            }

            AddError($"'{expected}'", CurrentToken());

            if (currentPos >= currentLineEnd)
                return;

            if (IsNumber() || IsMinus())
                return;

            if (CurrentToken().Code == (int)LexicalAnalyzer.TokenType.OPERATOR)
            {
                currentPos++;
                return;
            }

            SkipUntil(t =>
                t.Code == (int)LexicalAnalyzer.TokenType.NUMBER ||
                t.Value == ";");
        }

        private void MatchNumber()
        {
            if (IsNumber())
            {
                currentPos++;
                return;
            }

            if (IsMinus())
            {
                currentPos++;

                if (IsNumber())
                {
                    currentPos++;
                    return;
                }

                AddError("число после '-'", CurrentToken());

                if (currentPos >= currentLineEnd)
                    return;

                if (IsSemicolon())
                    return;

                SkipUntil(t => t.Value == ";");
                return;
            }

            AddError("число", CurrentToken());

            if (currentPos >= currentLineEnd)
                return;

            if (IsSemicolon())
                return;

            SkipUntil(t => t.Value == ";");
        }

        private void MatchSemicolon()
        {
            if (IsSemicolon())
            {
                currentPos++;
                return;
            }

            AddError("';'", CurrentToken());

            if (currentPos >= currentLineEnd)
                return;

            SkipUntil(t => t.Value == ";");

            if (IsSemicolon())
                currentPos++;
        }

        private bool NextAfter_DECLARE(LexicalAnalyzer.Token token)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER &&
                   !LooksLikeAnyKeyword(token.Value);
        }

        private bool NextAfter_CONSTANT(LexicalAnalyzer.Token token)
        {
            return IsKeywordAt(token, "INTEGER");
        }

        private bool NextAfter_INTEGER(LexicalAnalyzer.Token token)
        {
            return token != null &&
                   !token.IsError &&
                   token.Code == (int)LexicalAnalyzer.TokenType.OPERATOR &&
                   token.Value == ":=";
        }

        private bool LooksLikeAnyKeyword(string value)
        {
            return LooksLikeKeyword(value, "DECLARE") ||
                   LooksLikeKeyword(value, "CONSTANT") ||
                   LooksLikeKeyword(value, "INTEGER");
        }

        private bool LooksLikeKeyword(string value, string keyword)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (string.Equals(value, keyword, StringComparison.OrdinalIgnoreCase))
                return true;

            if (Math.Abs(value.Length - keyword.Length) > 2)
                return false;

            int distance = LevenshteinDistance(
                value.ToUpperInvariant(),
                keyword.ToUpperInvariant()
            );

            return distance <= 2;
        }

        private int LevenshteinDistance(string a, string b)
        {
            int[,] dp = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= b.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int cost = a[i - 1] == b[j - 1] ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(
                            dp[i - 1, j] + 1,
                            dp[i, j - 1] + 1
                        ),
                        dp[i - 1, j - 1] + cost
                    );
                }
            }

            return dp[a.Length, b.Length];
        }

        private bool IsKeyword(string expected)
        {
            if (currentPos >= currentLineEnd) return false;

            var t = tokens[currentPos];
            if (t.IsError) return false;

            return t.Code == (int)LexicalAnalyzer.TokenType.KEYWORD &&
                   string.Equals(t.Value, expected, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsIdentifier()
        {
            if (currentPos >= currentLineEnd) return false;

            var t = tokens[currentPos];
            if (t.IsError) return false;

            return t.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER;
        }

        private bool IsOperator(string expected)
        {
            if (currentPos >= currentLineEnd) return false;

            var t = tokens[currentPos];
            if (t.IsError) return false;

            return t.Code == (int)LexicalAnalyzer.TokenType.OPERATOR &&
                   t.Value == expected;
        }

        private bool IsNumber()
        {
            if (currentPos >= currentLineEnd) return false;

            var t = tokens[currentPos];
            if (t.IsError) return false;

            return t.Code == (int)LexicalAnalyzer.TokenType.NUMBER;
        }
        private bool IsMinus()
        {
            if (currentPos >= currentLineEnd) return false;

            var t = tokens[currentPos];
            if (t.IsError) return false;

            return t.Code == (int)LexicalAnalyzer.TokenType.OPERATOR &&
                   t.Value == "-";
        }
        private bool IsSemicolon()
        {
            if (currentPos >= currentLineEnd) return false;

            var t = tokens[currentPos];
            if (t.IsError) return false;

            return t.Code == (int)LexicalAnalyzer.TokenType.SEPARATOR &&
                   t.Value == ";";
        }

        private bool IsKeywordAt(LexicalAnalyzer.Token token, string expected)
        {
            if (token == null || token.IsError) return false;

            return token.Code == (int)LexicalAnalyzer.TokenType.KEYWORD &&
                   string.Equals(token.Value, expected, StringComparison.OrdinalIgnoreCase);
        }

        private void SkipUntil(Func<LexicalAnalyzer.Token, bool> stopCondition)
        {
            while (currentPos < currentLineEnd && !stopCondition(tokens[currentPos]))
            {
                currentPos++;
            }
        }

        private LexicalAnalyzer.Token CurrentToken()
        {
            return currentPos < currentLineEnd ? tokens[currentPos] : null;
        }

        private void AddError(string expected, LexicalAnalyzer.Token currentToken)
        {
            string fragment = currentToken != null ? currentToken.Value : "<конец строки>";

            string location = currentToken != null
                ? $"строка {currentToken.Line}, позиция {currentToken.StartPos + 1}"
                : $"строка {currentLineNumber}, конец строки";

            string description = $"Ожидалось {expected}, найдено '{fragment}'";

            errors.Add(new SyntaxError
            {
                Fragment = fragment,
                Location = location,
                Description = description
            });
        }
        private void AddKeywordError(string expected, LexicalAnalyzer.Token currentToken)
        {
            if (currentToken == null)
            {
                AddError($"'{expected}'", currentToken);
                return;
            }

            string actual = currentToken.Value;

            string upperExpected = expected.ToUpperInvariant();
            string upperActual = actual.ToUpperInvariant();

            int prefix = 0;

            while (prefix < upperExpected.Length &&
                   prefix < upperActual.Length &&
                   upperExpected[prefix] == upperActual[prefix])
            {
                prefix++;
            }

            int suffix = 0;

            while (suffix < upperExpected.Length - prefix &&
                   suffix < upperActual.Length - prefix &&
                   upperExpected[upperExpected.Length - 1 - suffix] ==
                   upperActual[upperActual.Length - 1 - suffix])
            {
                suffix++;
            }

            int errorStart = prefix;
            int errorLength = upperActual.Length - prefix - suffix;

            if (errorLength <= 0)
                errorLength = 1;

            if (errorStart >= actual.Length)
                errorStart = actual.Length - 1;

            if (errorStart + errorLength > actual.Length)
                errorLength = actual.Length - errorStart;

            string wrongFragment = actual.Substring(errorStart, errorLength);

            string location =
                $"строка {currentToken.Line}, позиция {currentToken.StartPos + errorStart + 1}";

            string description =
                $"Ожидалось '{expected}', найдено '{actual}'";

            errors.Add(new SyntaxError
            {
                Fragment = wrongFragment,
                Location = location,
                Description = description
            });
        }
    }

    public class SyntaxError
    {
        public string Fragment { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
    }
}