using System;
using System.Collections.Generic;

namespace comp
{
    public class LexicalAnalyzer
    {
        public enum TokenType
        {
            IDENTIFIER = 1,
            NUMBER = 2,
            OPERATOR = 3,
            LEFT_PAREN = 4,
            RIGHT_PAREN = 5,
            SEPARATOR = 6,
            ERROR = 99
        }

        public class Token
        {
            public int Code { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
            public int Line { get; set; }
            public int StartPos { get; set; }
            public int EndPos { get; set; }
            public bool IsError { get; set; }
        }

        public List<Token> Analyze(string text)
        {
            var tokens = new List<Token>();
            int line = 1;
            int column = 0;
            int pos = 0;

            while (pos < text.Length)
            {
                char ch = text[pos];

                if (char.IsWhiteSpace(ch))
                {
                    if (ch == '\n')
                    {
                        line++;
                        column = 0;
                    }
                    else if (ch == '\r')
                    {
                        column = 0;
                    }
                    else
                    {
                        column++;
                    }

                    pos++;
                    continue;
                }

                if (char.IsLetter(ch) || ch == '_')
                {
                    int start = pos;
                    string value = "";

                    while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
                    {
                        value += text[pos];
                        pos++;
                        column++;
                    }

                    tokens.Add(CreateToken(TokenType.IDENTIFIER, value, line, start, pos - 1));
                    continue;
                }

                if (char.IsDigit(ch))
                {
                    int start = pos;
                    string value = "";

                    while (pos < text.Length && char.IsDigit(text[pos]))
                    {
                        value += text[pos];
                        pos++;
                        column++;
                    }

                    if (pos < text.Length && (char.IsLetter(text[pos]) || text[pos] == '_'))
                    {
                        while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
                        {
                            value += text[pos];
                            pos++;
                            column++;
                        }

                        tokens.Add(CreateErrorToken(value, line, start, pos - 1,
                            "Число не может сразу переходить в идентификатор"));
                        continue;
                    }

                    tokens.Add(CreateToken(TokenType.NUMBER, value, line, start, pos - 1));
                    continue;
                }

                if (ch == '+' || ch == '-' || ch == '*' || ch == '/')
                {
                    tokens.Add(CreateToken(TokenType.OPERATOR, ch.ToString(), line, pos, pos));
                    pos++;
                    column++;
                    continue;
                }

                if (ch == '(')
                {
                    tokens.Add(CreateToken(TokenType.LEFT_PAREN, ch.ToString(), line, pos, pos));
                    pos++;
                    column++;
                    continue;
                }

                if (ch == ')')
                {
                    tokens.Add(CreateToken(TokenType.RIGHT_PAREN, ch.ToString(), line, pos, pos));
                    pos++;
                    column++;
                    continue;
                }

                if (ch == ';')
                {
                    tokens.Add(CreateToken(TokenType.SEPARATOR, ch.ToString(), line, pos, pos));
                    pos++;
                    column++;
                    continue;
                }

                tokens.Add(CreateErrorToken(ch.ToString(), line, pos, pos,
                    "Недопустимый символ"));
                pos++;
                column++;
            }

            return tokens;
        }

        private Token CreateToken(TokenType type, string value, int line, int start, int end)
        {
            return new Token
            {
                Code = (int)type,
                Type = GetTypeDescription(type),
                Value = value,
                Line = line,
                StartPos = start,
                EndPos = end,
                IsError = false
            };
        }

        private Token CreateErrorToken(string value, int line, int start, int end, string description)
        {
            return new Token
            {
                Code = (int)TokenType.ERROR,
                Type = "Ошибка: " + description,
                Value = value,
                Line = line,
                StartPos = start,
                EndPos = end,
                IsError = true
            };
        }

        private string GetTypeDescription(TokenType type)
        {
            switch (type)
            {
                case TokenType.IDENTIFIER: return "Идентификатор";
                case TokenType.NUMBER: return "Число";
                case TokenType.OPERATOR: return "Оператор";
                case TokenType.LEFT_PAREN: return "Открывающая скобка";
                case TokenType.RIGHT_PAREN: return "Закрывающая скобка";
                case TokenType.SEPARATOR: return "Разделитель";
                default: return "Неизвестный тип";
            }
        }
    }
}
