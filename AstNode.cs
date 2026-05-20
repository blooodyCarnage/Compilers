using System.Collections.Generic;
using System.Text;

namespace comp
{
    public abstract class AstNode
    {
        public string NodeType { get; protected set; }
        public List<AstNode> Children { get; } = new List<AstNode>();

        public abstract string ToTreeString(string indent = "", bool isLast = true);
    }

    public class ConstDeclNode : AstNode
    {
        public string Name { get; set; }
        public string Modifier { get; set; }
        public AstNode TypeNode { get; set; }
        public AstNode ValueNode { get; set; }

        public int Line { get; set; }
        public int Position { get; set; }

        public ConstDeclNode()
        {
            NodeType = "ConstDeclNode";
        }

        public override string ToTreeString(string indent = "", bool isLast = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}{NodeType}");

            string childIndent = indent + (isLast ? "    " : "│   ");

            sb.AppendLine($"{childIndent}├── name: \"{Name}\"");
            sb.AppendLine($"{childIndent}├── modifiers: [\"{Modifier}\"]");

            sb.Append(TypeNode.ToTreeString(childIndent, false));
            sb.Append(ValueNode.ToTreeString(childIndent, true));

            return sb.ToString();
        }
    }

    public class IntNode : AstNode
    {
        public string Name { get; set; } = "INTEGER";

        public IntNode()
        {
            NodeType = "IntNode";
        }

        public override string ToTreeString(string indent = "", bool isLast = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}type: {NodeType}");

            string childIndent = indent + (isLast ? "    " : "│   ");
            sb.AppendLine($"{childIndent}└── name: \"{Name}\"");

            return sb.ToString();
        }
    }

    public class IntLiteralNode : AstNode
    {
        public int Value { get; set; }

        public int Line { get; set; }
        public int Position { get; set; }

        public IntLiteralNode()
        {
            NodeType = "IntLiteralNode";
        }

        public override string ToTreeString(string indent = "", bool isLast = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}value: {NodeType}");

            string childIndent = indent + (isLast ? "    " : "│   ");
            sb.AppendLine($"{childIndent}└── value: {Value}");

            return sb.ToString();
        }
    }

    public class IdentifierNode : AstNode
    {
        public string Name { get; set; }

        public int Line { get; set; }
        public int Position { get; set; }

        public IdentifierNode()
        {
            NodeType = "IdentifierNode";
        }

        public override string ToTreeString(string indent = "", bool isLast = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"{indent}{(isLast ? "└── " : "├── ")}value: {NodeType}");

            string childIndent = indent + (isLast ? "    " : "│   ");
            sb.AppendLine($"{childIndent}└── name: \"{Name}\"");

            return sb.ToString();
        }
    }
}