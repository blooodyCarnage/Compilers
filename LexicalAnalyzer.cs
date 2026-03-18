using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            WHITESPACE = 6,      
            ERROR = 99           
        }

        // Ключевые слова языка
        private readonly HashSet<string> keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DECLARE", "CONSTANT", "INTEGER"
        };

        // Операторы
        private readonly HashSet<char> operators = new HashSet<char>
        {
            '=', '+', '-' 
        };

        // Разделители
        private readonly HashSet<char> separators = new HashSet<char>
        {
            ':', ';'
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

        /// <summary>
        /// Основной метод анализа текста
        /// </summary>
        /// <param name="text">Входной текст</param>
        /// <returns>Список лексем</returns>
        public List<Token> Analyze(string text)
        {
            var tokens = new List<Token>();
            int lineNumber = 1;
            int position = 0;
            int lineStartPos = 0;

            while (position < text.Length)
            {
                char currentChar = text[position];

                if (char.IsWhiteSpace(currentChar))
                {
                    if (currentChar == '\n')
                    {
                        lineNumber++;
                        lineStartPos = position + 1;
                    }

                    position++;
                    continue;
                }

                if (operators.Contains(currentChar))
                {
                    // Проверяем на составной оператор ":="
                    if (currentChar == ':' && position + 1 < text.Length && text[position + 1] == '=')
                    {
                        tokens.Add(CreateToken(TokenType.OPERATOR, ":=", lineNumber, position, position + 1));
                        position += 2;
                    }
                    else
                    {
                        tokens.Add(CreateToken(TokenType.OPERATOR, currentChar.ToString(), lineNumber, position, position));
                        position++;
                    }
                    continue;
                }

                if (separators.Contains(currentChar))
                {
                    tokens.Add(CreateToken(TokenType.SEPARATOR, currentChar.ToString(), lineNumber, position, position));
                    position++;
                    continue;
                }

                if (char.IsDigit(currentChar))
                {
                    string number = "";
                    int startPos = position;

                    while (position < text.Length && char.IsDigit(text[position]))
                    {
                        number += text[position];
                        position++;
                    }

                    tokens.Add(CreateToken(TokenType.NUMBER, number, lineNumber, startPos, position - 1));
                    continue;
                }

                if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    string identifier = "";
                    int startPos = position;

                    while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_'))
                    {
                        identifier += text[position];
                        position++;
                    }

                    if (keywords.Contains(identifier))
                    {
                        tokens.Add(CreateToken(TokenType.KEYWORD, identifier, lineNumber, startPos, position - 1));
                    }
                    else
                    {
                        tokens.Add(CreateToken(TokenType.IDENTIFIER, identifier, lineNumber, startPos, position - 1));
                    }
                    continue;
                }

                tokens.Add(CreateErrorToken(currentChar.ToString(), lineNumber, position, position));
                position++;
            }

            return tokens;
        }

        private Token CreateToken(TokenType type, string value, int line, int start, int end)
        {
            string typeDescription = GetTypeDescription(type);

            return new Token
            {
                Code = (int)type,
                Type = typeDescription,
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
                Type = "Ошибка: недопустимый символ",
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
                case TokenType.KEYWORD:
                    return "Ключевое слово";
                case TokenType.IDENTIFIER:
                    return "Идентификатор";
                case TokenType.NUMBER:
                    return "Число";
                case TokenType.OPERATOR:
                    return "Оператор";
                case TokenType.SEPARATOR:
                    return "Разделитель";
                case TokenType.WHITESPACE:
                    return "Пробел";
                default:
                    return "Неизвестный тип";
            }
        }
    }
}