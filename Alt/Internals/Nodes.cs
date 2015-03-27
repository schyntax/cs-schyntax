using System;
using System.Collections.Generic;
using System.Linq;

namespace Alt.Internals
{
    public abstract class Node
    {
        private readonly List<Token> _tokens = new List<Token>();

        public IReadOnlyList<Token> Tokens => _tokens.AsReadOnly();

        public int Index => Tokens[0].Index;

        internal void AddToken(Token token)
        {
            _tokens.Add(token);
        }
    }

    public class ProgramNode : Node
    {
        private readonly List<GroupNode> _groups = new List<GroupNode>(); 
        private readonly List<ExpressionNode> _expressions = new List<ExpressionNode>();

        public IReadOnlyList<GroupNode> Groups => _groups.AsReadOnly();
        public IReadOnlyList<ExpressionNode> Expressions => _expressions.AsReadOnly();

        public void AddGroup(GroupNode group)
        {
            _groups.Add(group);
        }

        public void AddExpression(ExpressionNode exp)
        {
            _expressions.Add(exp);
        }
    }

    public class GroupNode : Node
    {
        private readonly List<ExpressionNode> _expressions = new List<ExpressionNode>();
        public IReadOnlyList<ExpressionNode> Expressions => _expressions.AsReadOnly();

        public void AddExpression(ExpressionNode exp)
        {
            _expressions.Add(exp);
        }
    }

    public enum ExpressionType
    {
        IntervalValue, // used internally by the parser (not a real expression type)
        Seconds,
        Minutes,
        Hours,
        DaysOfWeek,
        DaysOfMonth,
        Dates,
    }

    public class ExpressionNode : Node
    {
        private readonly List<ArgumentNode> _arguments = new List<ArgumentNode>();

        public ExpressionType ExpressionType { get; }
        public IReadOnlyList<ArgumentNode> Arguments => _arguments.AsReadOnly();

        internal ExpressionNode(ExpressionType type)
        {
            ExpressionType = type;
        }

        internal void AddArgument(ArgumentNode arg)
        {
            _arguments.Add(arg);
        }
    }

    public class ArgumentNode : Node
    {
        public bool IsExclusion { get; internal set; }
        public bool HasInterval => Interval != null;
        public IntegerValueNode Interval { get; internal set; }
        public int IntervalValue => Interval.Value;
        public bool IsWildcard { get; internal set; }
        public bool IsRange => Range?.End != null;
        public RangeNode Range { get; internal set; }
        public ValueNode Value => Range?.Start;

        public int IntervalTokenIndex => Tokens.First(t => t.Type == TokenType.Interval).Index;
    }

    public class RangeNode : Node
    {
        public ValueNode Start { get; internal set; }
        public ValueNode End { get; internal set; }
    }

    public abstract class ValueNode : Node
    {
    }

    public class IntegerValueNode : ValueNode
    {
        public int Value { get; internal set; }
    }

    public class DateValueNode : ValueNode
    {
        public int? Year { get; internal set; }
        public int Month { get; internal set; }
        public int Day { get; internal set; }
    }
}
