using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace comp
{
    public class LexicalAnalyzer
    {
        // Типы лексем с их кодами
        public enum TokenType
        {
            KEYWORD = 1,        // Ключевое слово
            IDENTIFIER = 2,      // Идентификатор
            NUMBER = 3,          // Число
            OPERATOR = 4,        // Оператор
            SEPARATOR = 5,       // Разделитель
            WHITESPACE = 6,      // Пробельный символ
            ERROR = 99           // Ошибка
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

        // Результат анализа
        public class Token
        {
            public int Code { get; set; }              // Условный код
            public string Type { get; set; }            // Тип лексемы
            public string Value { get; set; }           // Лексема
            public int Line { get; set; }               // Номер строки
            public int StartPos { get; set; }           // Начальная позиция
            public int EndPos { get; set; }             // Конечная позиция
            public bool IsError { get; set; }           // Является ли ошибкой
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

                // Пропускаем пробелы (но сохраняем их как отдельные лексемы для навигации)
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

                // Проверяем на операторы
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

                // Проверяем на разделители
                if (separators.Contains(currentChar))
                {
                    tokens.Add(CreateToken(TokenType.SEPARATOR, currentChar.ToString(), lineNumber, position, position));
                    position++;
                    continue;
                }

                // Проверяем на цифры (числа)
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

                // Проверяем на буквы (идентификаторы или ключевые слова)
                if (char.IsLetter(currentChar) || currentChar == '_')
                {
                    string identifier = "";
                    int startPos = position;

                    while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_'))
                    {
                        identifier += text[position];
                        position++;
                    }

                    // Проверяем, является ли идентификатор ключевым словом
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

                // Если символ не распознан - ошибка
                tokens.Add(CreateErrorToken(currentChar.ToString(), lineNumber, position, position));
                position++;
            }

            return tokens;
        }

        /// <summary>
        /// Создание лексемы
        /// </summary>
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

        /// <summary>
        /// Создание лексемы-ошибки
        /// </summary>
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

        /// <summary>
        /// Получение текстового описания типа лексемы
        /// </summary>
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