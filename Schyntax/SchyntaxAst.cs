using System;
using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace Schyntax
{
    public class ExpressionNode : AstNode
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            NodeType = treeNode.ChildNodes[0].FindTokenAndGetText().ToLowerInvariant();
        }

        public string NodeType { get; set; }
    }

    public class IntegerLiteralNode : AstNode
    {
        public int Value { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var str = treeNode.FindTokenAndGetText();
            Value = int.Parse(str);
        }
    }
}
