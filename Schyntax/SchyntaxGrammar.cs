using System;
using Irony.Ast;
using Irony.Parsing;
using Schyntax.Ast;

namespace Schyntax
{
    public class SchyntaxGrammar : Grammar
    {
        public SchyntaxGrammar() : base(false) // NOT case sensitive
        {
            
            // ReSharper disable InconsistentNaming

            //terminals
            var SUNDAY = new RegexBasedTerminal("SUNDAY", @"\b(su|sun|sunday)\b");
            var MONDAY = new RegexBasedTerminal("MONDAY", @"\b(mo|mon|monday)\b");
            var TUESDAY = new RegexBasedTerminal("TUESDAY", @"\b(tu|tue|tuesday)\b");
            var WEDNESDAY = new RegexBasedTerminal("WEDNESDAY", @"\b(we|wed|wednesday)\b");
            var THURSDAY = new RegexBasedTerminal("THURSDAY", @"\b(th|thu|thursday)\b");
            var FRIDAY = new RegexBasedTerminal("FRIDAY", @"\b(fr|fri|friday|friday)\b");
            var SATURDAY = new RegexBasedTerminal("SATURDAY", @"\b(sa|sat|saturday)\b");
            var WEEKDAY_INTEGER = new RegexBasedTerminal("WEEKDAY_INTEGER", @"\b[1-7]\b");

            var COMMA = new KeyTerm(",", "COMMA");
            var ANY = new KeyTerm("*", "ANY");
            var EXCLUDE = new KeyTerm("!", "EXCLUDE");
            var MODULUS = new KeyTerm("%", "MODULUS");
            
            //var COMMA_OR_SPACE = new RegexBasedTerminal("COMMA_OR_SPACE", @"[ ,]");

            var POSITIVE_INTEGER = new RegexBasedTerminal("POSITIVE_INTEGER", @"[0-9]+");
            var NEGATIVE_INTEGER = new RegexBasedTerminal("NEGATIVE_INTEGER", @"\-[0-9]+");
            var DATE_VALUE = new RegexBasedTerminal("DATE_VALUE", @"\b(\d\d(\d\d)?\/)?\d{1,2}\/\d{1,2}\b");

            var SECONDS = new RegexBasedTerminal("SECONDS", @"\b(s|sec|second|seconds|secondofminute|secondsofminute)\b");
            var MINUTES = new RegexBasedTerminal("MINUTES", @"\b(m|min|minute|minutes|minuteofhour|minutesofhour)\b");
            var HOURS = new RegexBasedTerminal("HOURS", @"\b(h|hour|hours|hourofday|hoursofday)\b");
            var DAYS_OF_MONTH = new RegexBasedTerminal("DAYS_OF_MONTH", @"\b(dom|dayofmonth|daysofmonth)\b");
            var DAYS_OF_WEEK = new RegexBasedTerminal("DAYS_OF_WEEK", @"\b(day|days|dow|dayofweek|daysofweek)\b");
            var DATES = new RegexBasedTerminal("DATES", @"\b(date|dates)\b");

            // Non terminals
            //var Program = new NonTerminal("Program");
            //var GroupOrExpressionList = new NonTerminal("GroupOrExpressionList");
            //var GroupOrExpression = new NonTerminal("GroupOrExpression");
            //var Group = new NonTerminal("Group");
            var Rule = new NonTerminal("Expression", typeof(RuleNode));
            //var ExpressionList = new NonTerminal("ExpressionList");

            var IntegerRuleType = new NonTerminal("IntegerRuleType", typeof(IntegerRuleTypeNode));
            var IntegerRuleArgumentList = new NonTerminal("IntegerRuleArgumentList", typeof(SchyntaxAstNodeList));
            var IntegerRuleArgument = new NonTerminal("IntegerRuleArgument", typeof (IntegerRuleArgumentNode));
            var IntegerRange = new NonTerminal("IntegerRange", typeof(IntegerRangeNode));
            var IntegerValue = new NonTerminal("IntegerValue", typeof(IntegerValueNode));

            var DayOfWeekRuleArgumentList = new NonTerminal("DayArgumentList", typeof(SchyntaxAstNodeList));
            var DayOfWeekRuleArgument = new NonTerminal("DayArgument", typeof(DayOfWeekRuleArgumentNode));
            var DayOfWeekRange = new NonTerminal("DayRange", typeof(DayOfWeekRangeNode));
            var DayOfWeekValue = new NonTerminal("DayOfWeekValue", typeof(DayOfWeekValueNode));

            var DateRuleArgumentList = new NonTerminal("DateRuleArgumentList", typeof(SchyntaxAstNodeList));
            var DateRuleArgument = new NonTerminal("DateRuleArgument", typeof(DateRuleArgumentNode));
            var DateRange = new NonTerminal("DateRange", typeof(DateRangeNode));
            var DateValue = new NonTerminal("DateValue", typeof(DateValueNode));

            var OptionalExclude = new NonTerminal("OptionalExclude");
            var ModulusModifier = new NonTerminal("OptionalModulus", typeof(ModulusValueNode));

            

            // ReSharper restore InconsistentNaming

            //Program.Rule
            //    = GroupOrExpressionList + Eof
            //    ;
            
            //GroupOrExpressionList.Rule
            //    = MakeListRule(GroupOrExpressionList, COMMA_OR_SPACE, GroupOrExpression)
            //    ;

            //GroupOrExpression.Rule
            //    = Group
            //    | Expression
            //    ;

            //Group.Rule = 
            //    "(" + ExpressionList + ")"
            //    ;


            //ExpressionList.Rule 
            //    = MakeListRule(ExpressionList, COMMA_OR_SPACE, Expression)
            //    ;


            Rule.Rule
                = IntegerRuleType + "(" + IntegerRuleArgumentList + ")"
                //| DATES + "(" + DateArgumentList + ")"
                | DAYS_OF_WEEK + "(" + DayOfWeekRuleArgumentList + ")"
                ;

            IntegerRuleType.Rule
                = SECONDS
                | MINUTES
                | HOURS
                | DAYS_OF_MONTH
                ;
            
            /* --- Arguments --- */

            //DateArgumentList.Rule
            //    = MakePlusRule(DateArgumentList, COMMA, DateArgument)
            //    ;

            IntegerRuleArgumentList.Rule
                //= MakeListRule(IntegerArgumentList, COMMA, IntegerOrAnyArgument)
                = IntegerRuleArgumentList + COMMA + IntegerRuleArgument
                | IntegerRuleArgument
                ;

            DayOfWeekRuleArgumentList.Rule
                = DayOfWeekRuleArgumentList + COMMA + DayOfWeekRuleArgument
                | DayOfWeekRuleArgument
                ;

            //DateArgument.Rule
            //    = OptionalExclude + ModulusLiteral
            //    | OptionalExclude + DateRange + OptionalModulus
            //    ;

            IntegerRuleArgument.Rule
                = OptionalExclude + IntegerRange + ModulusModifier
                | OptionalExclude + IntegerRange
                | OptionalExclude + ModulusModifier
                ;

            DayOfWeekRuleArgument.Rule
                = OptionalExclude + DayOfWeekRange + ModulusModifier
                | OptionalExclude + DayOfWeekRange
                | OptionalExclude + ModulusModifier
                ;

            OptionalExclude.Rule
                = EXCLUDE
                | Empty
                ;

            ModulusModifier.Rule
                = MODULUS + POSITIVE_INTEGER
                ;

            /* --- Ranges --- */

            //DateRange.Rule
            //    = DateLiteral
            //    | DateLiteral + ".." + DateLiteral
            //    | ANY
            //    ;

            IntegerRange.Rule
                = IntegerValue + ".." + IntegerValue
                | IntegerValue
                //| ANY
                ;

            DayOfWeekRange.Rule
                = DayOfWeekValue + ".." + DayOfWeekValue
                | DayOfWeekValue
                //| ANY
                ;

            /* --- Literals --- */

            //DateLiteral.Rule
            //    = POSITIVE_INTEGER + "/" + POSITIVE_INTEGER
            //    | POSITIVE_INTEGER + "/" + POSITIVE_INTEGER + "/" + POSITIVE_INTEGER
            //    ;

            DayOfWeekValue.Rule
                = SUNDAY
                | MONDAY
                | TUESDAY
                | WEDNESDAY
                | THURSDAY
                | FRIDAY
                | SATURDAY
                | WEEKDAY_INTEGER
                ;

            IntegerValue.Rule
                = POSITIVE_INTEGER
                | NEGATIVE_INTEGER
                ;

            // Punctuation

            MarkTransient(OptionalExclude);

            MarkPunctuation("/", "(", ")", "..");
            MarkPunctuation(COMMA, ANY, EXCLUDE, MODULUS);

            LanguageFlags |= LanguageFlags.CreateAst;

            Root = Rule;
        }

        public override void SkipWhitespace(ISourceStream source)
        {
            //base.SkipWhitespace(source);
        }

        public override void BuildAst(LanguageData language, ParseTree parseTree)
        {
            if (!LanguageFlags.IsSet(LanguageFlags.CreateAst))
                return;

            var astContext = new AstContext(language);
            astContext.DefaultNodeType = typeof(TokenNode);
            var astBuilder = new AstBuilder(astContext);
            try
            {
                astBuilder.BuildAst(parseTree);
            }
            finally
            {
                astContext.Messages.ForEach(lm => Console.WriteLine("AST MESSAGE: {0}", lm.Message));
            }
        }
    }

}
