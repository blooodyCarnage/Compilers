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
            ParseDeclaration();
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

        private void ParseDeclaration()
        {
            if (!Match(LexicalAnalyzer.TokenType.KEYWORD, "DECLARE"))
            {
                AddError("DECLARE", CurrentToken());
                SkipToken();
            }

            if (!Match(LexicalAnalyzer.TokenType.IDENTIFIER))
            {
                AddError("идентификатор", CurrentToken());
                SkipToken();
            }

            while (true)
            {
                if (Match(LexicalAnalyzer.TokenType.KEYWORD, "CONSTANT"))
                    break;
                if (currentPos >= tokens.Count || IsSemicolon())
                    return;
                var token = tokens[currentPos];
                if (token.Value == "INTEGER" || token.Value == ":=" || token.Code == (int)LexicalAnalyzer.TokenType.NUMBER)
                    break;
                AddError("CONSTANT", token);
                SkipToken();
            }

            while (true)
            {
                if (Match(LexicalAnalyzer.TokenType.KEYWORD, "INTEGER"))
                    break;
                if (currentPos >= tokens.Count || IsSemicolon())
                    return;
                var token = tokens[currentPos];
                if (token.Value == ":=" || token.Code == (int)LexicalAnalyzer.TokenType.NUMBER)
                    break;
                AddError("INTEGER", token);
                SkipToken();
            }

            while (true)
            {
                if (Match(LexicalAnalyzer.TokenType.OPERATOR, ":="))
                    break;
                if (currentPos >= tokens.Count || IsSemicolon())
                    return;
                var token = tokens[currentPos];
                if (token.Code == (int)LexicalAnalyzer.TokenType.NUMBER)
                    break;
                AddError(":=", token);
                SkipToken();
            }

            while (true)
            {
                if (Match(LexicalAnalyzer.TokenType.NUMBER))
                    break;
                if (currentPos >= tokens.Count || IsSemicolon())
                    return;
                AddError("число", CurrentToken());
                SkipToken();
            }

            // 7. ;
            if (!Match(LexicalAnalyzer.TokenType.SEPARATOR, ";"))
            {
                if (!IsSemicolon())
                    AddError(";", CurrentToken());
            }
        }

        private bool Match(LexicalAnalyzer.TokenType expectedType, string expectedValue = null)
        {
            if (currentPos >= tokens.Count) return false;
            var token = tokens[currentPos];
            if (token.Code == (int)expectedType)
            {
                if (expectedValue == null || string.Equals(token.Value, expectedValue, StringComparison.Ordinal))
                {
                    currentPos++;
                    return true;
                }
            }
            return false;
        }

        private void SkipToken()
        {
            if (currentPos < tokens.Count) currentPos++;
        }

        private bool IsSemicolon()
        {
            return currentPos < tokens.Count && tokens[currentPos].Value == ";";
        }

        private LexicalAnalyzer.Token CurrentToken()
        {
            return currentPos < tokens.Count ? tokens[currentPos] : null;
        }

        private void AddError(string expected, LexicalAnalyzer.Token currentToken)
        {
            string fragment = currentToken != null ? currentToken.Value : "<конец строки>";
            string location = currentToken != null ? $"строка {currentToken.Line}, позиция {currentToken.StartPos + 1}" : "конец файла";
            string description = $"Ожидалось '{expected}', найдено '{fragment}'";
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