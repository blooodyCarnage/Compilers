using System;
using System.Collections.Generic;

namespace comp
{
    public class SyntaxAnalyzer
    {
        private List<LexicalAnalyzer.Token> tokens;
        private int currentPos;
        private List<SyntaxError> errors;

        public SyntaxAnalyzer(List<LexicalAnalyzer.Token> tokens)
        {
            this.tokens = tokens;
            currentPos = 0;
            errors = new List<SyntaxError>();
        }

        public List<SyntaxError> Parse()
        {
            ExpectKeyword("DECLARE");
            ExpectIdentifier();
            ExpectKeyword("CONSTANT");
            ExpectKeyword("INTEGER");
            bool operatorOk = ExpectOperator(":=");
            if (operatorOk)
            {
                ExpectNumber();
            }
            else
            {
                SkipToSemicolonWithNumberError();
            }
            ExpectSemicolon();

            if (currentPos < tokens.Count)
            {
                var extraToken = tokens[currentPos];
                errors.Add(new SyntaxError
                {
                    Fragment = extraToken.Value,
                    Location = $"строка {extraToken.Line}, позиция {extraToken.StartPos + 1}",
                    Description = "Лишний текст после объявления"
                });
            }
            return errors;
        }

        private bool ExpectKeyword(string expectedValue)
        {
            if (currentPos >= tokens.Count)
            {
                AddError($"'{expectedValue}'", null);
                return false;
            }
            var token = tokens[currentPos];
            if (token.Code == (int)LexicalAnalyzer.TokenType.KEYWORD &&
                string.Equals(token.Value, expectedValue, StringComparison.OrdinalIgnoreCase))
            {
                currentPos++;
                return true;
            }
            else
            {
                AddError($"'{expectedValue}'", token);
                currentPos++;
                return false;
            }
        }

        private bool ExpectIdentifier()
        {
            if (currentPos >= tokens.Count)
            {
                AddError("идентификатор", null);
                return false;
            }
            var token = tokens[currentPos];
            if (token.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER)
            {
                currentPos++;
                return true;
            }
            else
            {
                AddError("идентификатор", token);
                currentPos++;
                return false;
            }
        }

        private bool ExpectOperator(string expectedValue)
        {
            if (currentPos >= tokens.Count)
            {
                AddError($"'{expectedValue}'", null);
                return false;
            }
            var token = tokens[currentPos];
            if (token.Code == (int)LexicalAnalyzer.TokenType.OPERATOR &&
                string.Equals(token.Value, expectedValue, StringComparison.Ordinal))
            {
                currentPos++;
                return true;
            }
            else
            {
                AddError($"'{expectedValue}'", token);
                currentPos++;
                return false;
            }
        }

        private bool ExpectNumber()
        {
            if (currentPos >= tokens.Count)
            {
                AddError("число", null);
                return false;
            }
            var token = tokens[currentPos];
            if (token.Code == (int)LexicalAnalyzer.TokenType.NUMBER)
            {
                currentPos++;
                return true;
            }
            else
            {
                AddError("число", token);
                currentPos++;
                return false;
            }
        }

        private void ExpectSemicolon()
        {
            if (currentPos >= tokens.Count)
            {
                AddError("';'", null);
                return;
            }
            var token = tokens[currentPos];
            if (token.Code == (int)LexicalAnalyzer.TokenType.SEPARATOR && token.Value == ";")
            {
                currentPos++;
            }
            else
            {
                AddError("';'", token);
                if (token.Value != ";")
                    currentPos++;
            }
        }
        private void SkipToSemicolonWithNumberError()
        {
            while (currentPos < tokens.Count)
            {
                var token = tokens[currentPos];
                if (token.Value == ";")
                    break;
                if (token.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER && ContainsDigit(token.Value))
                {
                    AddError("число", token);
                }
                currentPos++;
            }
        }

        private bool ContainsDigit(string s)
        {
            foreach (char c in s)
                if (char.IsDigit(c)) return true;
            return false;
        }

        private void AddError(string expected, LexicalAnalyzer.Token currentToken)
        {
            string fragment = currentToken != null ? currentToken.Value : "<конец строки>";
            string location = currentToken != null ? $"строка {currentToken.Line}, позиция {currentToken.StartPos + 1}" : "конец файла";
            string description = $"Ожидалось {expected}, найдено '{fragment}'";
            errors.Add(new SyntaxError
            {
                Fragment = fragment,
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