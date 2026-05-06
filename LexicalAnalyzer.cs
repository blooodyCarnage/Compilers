using System;
using System.Collections.Generic;

namespace comp
{
    public class LexicalAnalyzer
    {
        public enum TokenType
        {
            KEYWORD = 1,
            IDENTIFIER = 2,
            NUMBER = 3,
            OPERATOR = 4,
            SEPARATOR = 5,
            ERROR = 99
        }

        private readonly HashSet<string> keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DECLARE", "CONSTANT", "INTEGER"
        };

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
            int pos = 0;

            while (pos < text.Length)
            {
                char ch = text[pos];

                if (char.IsWhiteSpace(ch))
                {
                    if (ch == '\n') line++;
                    pos++;
                    continue;
                }

                if (ch == ':' && pos + 1 < text.Length && text[pos + 1] == '=')
                {
                    tokens.Add(CreateToken(TokenType.OPERATOR, ":=", line, pos, pos + 1));
                    pos += 2;
                    continue;
                }

                if (ch == ':')
                {
                    int start = pos;
                    string err = "";
                    while (pos < text.Length && !char.IsWhiteSpace(text[pos]) && text[pos] != ';')
                    {
                        err += text[pos];
                        pos++;
                    }
                    tokens.Add(CreateErrorToken(err, line, start, pos - 1));
                    continue;
                }

                if (ch == ';')
                {
                    tokens.Add(CreateToken(TokenType.SEPARATOR, ";", line, pos, pos));
                    pos++;
                    continue;
                }
                if (ch == '=' || ch == '+' || ch == '-')
                {
                    tokens.Add(CreateToken(TokenType.OPERATOR, ch.ToString(), line, pos, pos));
                    pos++;
                    continue;
                }

                if (char.IsDigit(ch))
                {
                    int start = pos;
                    while (pos < text.Length && char.IsDigit(text[pos])) pos++;
                    string val = text.Substring(start, pos - start);
                    tokens.Add(CreateToken(TokenType.NUMBER, val, line, start, pos - 1));
                    continue;
                }

                if (char.IsLetter(ch) || ch == '_')
                {
                    int start = pos;
                    string word = "";
                    while (pos < text.Length && (char.IsLetterOrDigit(text[pos]) || text[pos] == '_'))
                    {
                        word += text[pos];
                        pos++;
                    }
                    if (pos < text.Length)
                    {
                        char next = text[pos];
                        if (!char.IsWhiteSpace(next) && next != ';' && next != ':' && next != '=' && next != '+' && next != '-')
                        {
                            while (pos < text.Length && !char.IsWhiteSpace(text[pos]) && text[pos] != ';' && text[pos] != ':' && text[pos] != '=' && text[pos] != '+' && text[pos] != '-')
                            {
                                word += text[pos];
                                pos++;
                            }
                            tokens.Add(CreateErrorToken(word, line, start, pos - 1));
                            continue;
                        }
                    }
                    if (keywords.Contains(word))
                        tokens.Add(CreateToken(TokenType.KEYWORD, word, line, start, pos - 1));
                    else
                        tokens.Add(CreateToken(TokenType.IDENTIFIER, word, line, start, pos - 1));
                    continue;
                }

                tokens.Add(CreateErrorToken(ch.ToString(), line, pos, pos));
                pos++;
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

        private Token CreateErrorToken(string value, int line, int start, int end)
        {
            return new Token
            {
                Code = (int)TokenType.ERROR,
                Type = "Ошибка",
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
                case TokenType.KEYWORD: return "Ключевое слово";
                case TokenType.IDENTIFIER: return "Идентификатор";
                case TokenType.NUMBER: return "Число";
                case TokenType.OPERATOR: return "Оператор";
                case TokenType.SEPARATOR: return "Разделитель";
                default: return "Неизвестный тип";
            }
        }
    }
}