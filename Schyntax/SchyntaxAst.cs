using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Irony.Ast;
using Irony.Parsing;

// ReSharper disable once CheckNamespace
namespace Schyntax.Ast
{
    public interface ISchyntaxAstNode
    {
    }

    public abstract class SchyntaxAstNode : ISchyntaxAstNode, IAstNodeInit
    {
        public abstract void Init(AstContext context, ParseTreeNode parseNode);
    }

    public class SchyntaxAstNodeList : List<ISchyntaxAstNode>, ISchyntaxAstNode, IAstNodeInit
    {
        public void Init(AstContext context, ParseTreeNode parseNode)
        {
            Add((ISchyntaxAstNode)parseNode.ChildNodes[0].AstNode);
        }
    }

    public class RuleNode : SchyntaxAstNode
    {
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            Type = (SchyntaxAstNode)parseNode.ChildNodes[0].AstNode;
            Arguments = (SchyntaxAstNodeList)parseNode.ChildNodes[1].AstNode;
        } 

        public SchyntaxAstNode Type { get; set; }
        public SchyntaxAstNodeList Arguments { get; set; }
    }

    public class IntegerRuleTypeNode : SchyntaxAstNode
    {
        private static readonly Dictionary<Regex, string> TypeExpressions = new Dictionary<Regex, string>
        {
            { new Regex(@"^(s|sec|second|seconds|secondofminute|secondsofminute)$", RegexOptions.Compiled), "seconds" },
            { new Regex(@"^(m|min|minute|minutes|minuteofhour|minutesofhour)$", RegexOptions.Compiled), "minutes" },
            { new Regex(@"^(h|hour|hours|hourofday|hoursofday)$", RegexOptions.Compiled), "hours" },
            { new Regex(@"^(dom|dayofmonth|daysofmonth)$", RegexOptions.Compiled), "dayofmonth" },
        };

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            var text = parseNode.FindTokenAndGetText();
            foreach (var kv in TypeExpressions.Where(kv => kv.Key.IsMatch(text)))
            {
                Type = kv.Value;
                return;
            }

            throw new InvalidOperationException("Unknown Integer Rule Type: " + text);
        }

        public string Type { get; set; }
    }

    public class RuleArgumentNode<TNode> : SchyntaxAstNode
        where TNode : SchyntaxAstNode
    {
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            if (parseNode.ChildNodes[0].FindTokenAndGetText() == "!") Exclude = true;
            Range = parseNode.ChildNodes.Select(n => n.AstNode).OfType<TNode>().SingleOrDefault();
            Mod = parseNode.ChildNodes.Select(n => n.AstNode).OfType<ModulusValueNode>().Select(mv => (int?)mv.Value).SingleOrDefault();
        }

        public TNode Range { get; set; }
        public bool Exclude { get; set; }
        public int? Mod { get; set; }
        public bool IsAny { get { return Range == null; } }
    }

    public class DayOfWeekRuleArgumentNode : RuleArgumentNode<DayOfWeekValueNode> {}
    public class IntegerRuleArgumentNode : RuleArgumentNode<IntegerValueNode> {}
    public class DateRuleArgumentNode : RuleArgumentNode<DateValueNode> {}

    public class ValueNode<T> : SchyntaxAstNode
    {
        private readonly Func<string, T> _converter;

        public ValueNode(Func<string, T> converter)
        {
            _converter = converter;
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            var text = parseNode.FindTokenAndGetText();
            Value = _converter(text);
        }

        public T Value { get; set; }
    }

    public class IntegerValueNode : ValueNode<int>
    {
        public IntegerValueNode() : base(int.Parse) {}
    }

    public class DayOfWeekValueNode : ValueNode<DayOfWeek>
    {
        public DayOfWeekValueNode() : base(TextToDayOfWeek) {}

        private static DayOfWeek TextToDayOfWeek(string text)
        {
            if (char.IsNumber(text[0]))
                return (DayOfWeek)(int.Parse(text) - 1);

            switch (text.Substring(0, 2))
            {
                case "su": return DayOfWeek.Sunday;
                case "mo": return DayOfWeek.Monday;
                case "tu": return DayOfWeek.Tuesday;
                case "we": return DayOfWeek.Wednesday;
                case "th": return DayOfWeek.Thursday;
                case "fr": return DayOfWeek.Friday;
                case "sa": return DayOfWeek.Saturday;
                default: throw new ArgumentException("Unknown day of week: " + text);
            }
        }
    }

    public class DateValueNode : ValueNode<DateTime>
    {
        public DateValueNode() : base(DateTime.Parse) {}
    }

    public class ModulusValueNode : IntegerValueNode { }
    
    [DebuggerDisplay("[{Low}..{High}]")]
    public class RangeNode<T, TValueNode> : SchyntaxAstNode
        where TValueNode : ValueNode<T>
    {
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            Low = High = ((TValueNode)parseNode.ChildNodes[0].AstNode).Value;
            if (parseNode.ChildNodes.Count > 1)
                High = ((TValueNode)parseNode.ChildNodes[1].AstNode).Value;
        }

        public T Low { get; set; }
        public T High { get; set; }
    }

    public class IntegerRangeNode : RangeNode<int, IntegerValueNode> {}
    public class DayOfWeekRangeNode : RangeNode<DayOfWeek, DayOfWeekValueNode> {}
    public class DateRangeNode : RangeNode<DateTime, DateValueNode> {}


    public class TokenNode : SchyntaxAstNode
    {
        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            Text = parseNode.FindTokenAndGetText();
        }

        public string Text { get; set; }
    }
}
