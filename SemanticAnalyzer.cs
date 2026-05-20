using System;
using System.Collections.Generic;
using System.Text;

namespace comp
{
    public class SemanticAnalyzer
    {
        private readonly List<LexicalAnalyzer.Token> tokens;
        private readonly SymbolTable symbolTable;
        private readonly List<SemanticError> errors;
        private readonly List<AstNode> astRoots;

        private int currentPos;

        public SemanticAnalyzer(List<LexicalAnalyzer.Token> tokens)
        {
            this.tokens = tokens;
            symbolTable = new SymbolTable();
            errors = new List<SemanticError>();
            astRoots = new List<AstNode>();
            currentPos = 0;
        }

        public List<SemanticError> Errors => errors;
        public List<AstNode> AstRoots => astRoots;

        public void Analyze()
        {
            while (currentPos < tokens.Count)
            {
                AstNode node = ParseConstDeclaration();

                if (node != null)
                    astRoots.Add(node);
                else
                    SkipToNextSemicolon();
            }
        }

        private AstNode ParseConstDeclaration()
        {
            LexicalAnalyzer.Token declareToken = MatchValue("DECLARE");
            if (declareToken == null)
                return null;

            LexicalAnalyzer.Token nameToken = MatchType(LexicalAnalyzer.TokenType.IDENTIFIER);
            if (nameToken == null)
                return null;

            LexicalAnalyzer.Token constantToken = MatchValue("CONSTANT");
            if (constantToken == null)
                return null;

            LexicalAnalyzer.Token typeToken = MatchValue("INTEGER");
            if (typeToken == null)
                return null;

            LexicalAnalyzer.Token assignToken = MatchValue(":=");
            if (assignToken == null)
                return null;

            bool hasSemanticError = false;

            AstNode valueNode = ParseValueExpression(typeToken.Value, ref hasSemanticError);

            MatchValue(";");

            if (symbolTable.CheckDuplicate(nameToken.Value))
            {
                SymbolInfo previous = symbolTable.Lookup(nameToken.Value);

                AddError(
                    $"Ошибка: идентификатор \"{nameToken.Value}\" уже объявлен ранее (строка {previous.Line})",
                    nameToken
                );

                hasSemanticError = true;
            }

            if (valueNode == null)
                hasSemanticError = true;

            if (hasSemanticError)
                return null;

            string valueText = "";

            if (valueNode is IntLiteralNode intLiteral)
                valueText = intLiteral.Value.ToString();
            else if (valueNode is IdentifierNode idNode)
                valueText = idNode.Name;

            symbolTable.Declare(new SymbolInfo
            {
                Name = nameToken.Value,
                Type = typeToken.Value.ToUpper(),
                Value = valueText,
                Line = nameToken.Line,
                Position = nameToken.StartPos + 1
            });

            return new ConstDeclNode
            {
                Name = nameToken.Value,
                Modifier = constantToken.Value,
                TypeNode = new IntNode
                {
                    Name = typeToken.Value
                },
                ValueNode = valueNode,
                Line = nameToken.Line,
                Position = nameToken.StartPos + 1
            };
        }

        private AstNode ParseValueExpression(string declaredType, ref bool hasSemanticError)
        {
            LexicalAnalyzer.Token token = CurrentToken();

            if (token == null)
                return null;

            if (token.Code == (int)LexicalAnalyzer.TokenType.NUMBER)
            {
                currentPos++;
                return CreateIntegerLiteralNode(token.Value, token, ref hasSemanticError);
            }

            if (token.Code == (int)LexicalAnalyzer.TokenType.OPERATOR &&
                (token.Value == "-" || token.Value == "+"))
            {
                LexicalAnalyzer.Token signToken = token;
                currentPos++;

                LexicalAnalyzer.Token numberToken = CurrentToken();

                if (numberToken == null ||
                    numberToken.Code != (int)LexicalAnalyzer.TokenType.NUMBER)
                {
                    AddError(
                        $"Ошибка: после оператора \"{signToken.Value}\" ожидалось число",
                        signToken
                    );

                    hasSemanticError = true;
                    return null;
                }

                currentPos++;

                string numberText = signToken.Value + numberToken.Value;

                return CreateIntegerLiteralNode(numberText, signToken, ref hasSemanticError);
            }

            if (token.Code == (int)LexicalAnalyzer.TokenType.IDENTIFIER)
            {
                currentPos++;

                SymbolInfo symbol = symbolTable.Lookup(token.Value);

                if (symbol == null)
                {
                    AddError(
                        $"Ошибка: идентификатор \"{token.Value}\" не был объявлен ранее",
                        token
                    );

                    hasSemanticError = true;
                }
                else if (symbol.Type.ToUpper() != declaredType.ToUpper())
                {
                    AddError(
                        $"Ошибка: тип идентификатора \"{token.Value}\" не соответствует типу {declaredType}",
                        token
                    );

                    hasSemanticError = true;
                }

                return new IdentifierNode
                {
                    Name = token.Value,
                    Line = token.Line,
                    Position = token.StartPos + 1
                };
            }

            AddError(
                $"Ошибка: значение \"{token.Value}\" не соответствует типу {declaredType}",
                token
            );

            currentPos++;
            hasSemanticError = true;

            return null;
        }

        private AstNode CreateIntegerLiteralNode(
            string valueText,
            LexicalAnalyzer.Token token,
            ref bool hasSemanticError)
        {
            if (!int.TryParse(valueText, out int value))
            {
                AddError(
                    $"Ошибка: значение \"{valueText}\" выходит за пределы типа INTEGER",
                    token
                );

                hasSemanticError = true;
                return null;
            }

            return new IntLiteralNode
            {
                Value = value,
                Line = token.Line,
                Position = token.StartPos + 1
            };
        }

        private LexicalAnalyzer.Token MatchValue(string expected)
        {
            LexicalAnalyzer.Token token = CurrentToken();

            if (token != null &&
                string.Equals(token.Value, expected, StringComparison.OrdinalIgnoreCase))
            {
                currentPos++;
                return token;
            }

            return null;
        }

        private LexicalAnalyzer.Token MatchType(LexicalAnalyzer.TokenType expectedType)
        {
            LexicalAnalyzer.Token token = CurrentToken();

            if (token != null && token.Code == (int)expectedType)
            {
                currentPos++;
                return token;
            }

            return null;
        }

        private LexicalAnalyzer.Token CurrentToken()
        {
            if (currentPos >= tokens.Count)
                return null;

            return tokens[currentPos];
        }

        private void SkipToNextSemicolon()
        {
            while (currentPos < tokens.Count && tokens[currentPos].Value != ";")
                currentPos++;

            if (currentPos < tokens.Count && tokens[currentPos].Value == ";")
                currentPos++;
        }

        private void AddError(string message, LexicalAnalyzer.Token token)
        {
            errors.Add(new SemanticError
            {
                Message = message,
                Location = $"строка {token.Line}, символ {token.StartPos + 1}",
                Fragment = token.Value
            });
        }

        public string GetAstText()
        {
            if (astRoots.Count == 0)
                return "AST не построено.";

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < astRoots.Count; i++)
            {
                sb.Append(astRoots[i].ToTreeString());

                if (i < astRoots.Count - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}