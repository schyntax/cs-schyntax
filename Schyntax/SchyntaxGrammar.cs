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

            // Terminals

            // Punctuation
            var COMMA = new KeyTerm(",", "COMMA");
            var ANY = new KeyTerm("*", "ANY");
            var EXCLUDE = new KeyTerm("!", "EXCLUDE");
            var MODULUS = new KeyTerm("%", "MODULUS");
            var COMMA_OR_SPACE = new RegexBasedTerminal("COMMA_OR_SPACE", "[, ]");

            // Days of the week
            var SUNDAY = new RegexBasedTerminal("SUNDAY", @"\b(su|sun|sunday)\b");
            var MONDAY = new RegexBasedTerminal("MONDAY", @"\b(mo|mon|monday)\b");
            var TUESDAY = new RegexBasedTerminal("TUESDAY", @"\b(tu|tue|tuesday)\b");
            var WEDNESDAY = new RegexBasedTerminal("WEDNESDAY", @"\b(we|wed|wednesday)\b");
            var THURSDAY = new RegexBasedTerminal("THURSDAY", @"\b(th|thu|thursday)\b");
            var FRIDAY = new RegexBasedTerminal("FRIDAY", @"\b(fr|fri|friday|friday)\b");
            var SATURDAY = new RegexBasedTerminal("SATURDAY", @"\b(sa|sat|saturday)\b");
            var WEEKDAY_INTEGER = new RegexBasedTerminal("WEEKDAY_INTEGER", @"\b[1-7]\b");
            
            // Date values
            // Irony really does not like backref'ed regexes
            var DATE_WITH_SLASH_VALUE = new RegexBasedTerminal("DATE_WITH_SLASH_VALUE", @"(\d\d(\d\d)?\/)?\d\d\/\d\d");
            var DATE_WITH_HYPHEN_VALUE = new RegexBasedTerminal("DATE_WITH_HYPHEN_VALUE", @"(\d\d(\d\d)?-)?\d\d-\d\d");

            // Integer Values
            var POSITIVE_INTEGER = new RegexBasedTerminal("POSITIVE_INTEGER", @"[0-9]+");
            var NEGATIVE_INTEGER = new RegexBasedTerminal("NEGATIVE_INTEGER", @"\-[0-9]+");
            
            // Rule types
            var SECONDS = new RegexBasedTerminal("SECONDS", @"\b(s|sec|second|seconds|secondofminute|secondsofminute)\b");
            var MINUTES = new RegexBasedTerminal("MINUTES", @"\b(m|min|minute|minutes|minuteofhour|minutesofhour)\b");
            var HOURS = new RegexBasedTerminal("HOURS", @"\b(h|hour|hours|hourofday|hoursofday)\b");
            var DAYS_OF_MONTH = new RegexBasedTerminal("DAYS_OF_MONTH", @"\b(dom|dayofmonth|daysofmonth)\b");
            var DAYS_OF_WEEK = new RegexBasedTerminal("DAYS_OF_WEEK", @"\b(day|days|dow|dayofweek|daysofweek)\b");
            var DATES = new RegexBasedTerminal("DATES", @"\b(date|dates)\b");

            // Non terminals

            // A Program is a 
            var Program = new NonTerminal("Program");
            var GroupOrRuleList = new NonTerminal("GroupOrRuleList", typeof(SchyntaxAstNodeList));
            var GroupOrRule = new NonTerminal("GroupOrRule");

            var Group = new NonTerminal("Group", typeof(SchyntaxAstNodeList));
            var RuleList = new NonTerminal("RuleList", typeof(SchyntaxAstNodeList));

            var Rule = new NonTerminal("Expression", typeof(RuleNode));

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
            Program.Rule
                = GroupOrRuleList + Eof
                ;

            GroupOrRuleList.Rule
                = GroupOrRuleList + COMMA_OR_SPACE + GroupOrRule
                | GroupOrRule
                ;

            GroupOrRule.Rule
                = Group
                | Rule
                ;

            Group.Rule
                = "(" + RuleList + ")"
                ;

            RuleList.Rule
                = RuleList + COMMA_OR_SPACE + Rule
                | Rule
                ;

            Rule.Rule
                = DATES + "(" + DateRuleArgumentList + ")"
                | DAYS_OF_WEEK + "(" + DayOfWeekRuleArgumentList + ")"
                | IntegerRuleType + "(" + IntegerRuleArgumentList + ")"
                ;

            IntegerRuleType.Rule
                = SECONDS
                | MINUTES
                | HOURS
                | DAYS_OF_MONTH
                ;
            
            /* --- Arguments --- */

            DateRuleArgumentList.Rule
                = DateRuleArgumentList + COMMA + DateRuleArgument
                | DateRuleArgument
                ;

            
            IntegerRuleArgumentList.Rule
                = IntegerRuleArgumentList + COMMA + IntegerRuleArgument
                | IntegerRuleArgument
                ;

            DayOfWeekRuleArgumentList.Rule
                = DayOfWeekRuleArgumentList + COMMA + DayOfWeekRuleArgument
                | DayOfWeekRuleArgument
                ;
            

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

            DateRuleArgument.Rule
                = OptionalExclude + DateRange + ModulusModifier
                | OptionalExclude + DateRange
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

            DateRange.Rule
                = DateValue + ".." + DateValue
                | DateValue
                //| ANY
                ;

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

            IntegerValue.Rule
                = POSITIVE_INTEGER
                | NEGATIVE_INTEGER
                ;

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

            DateValue.Rule
                = DATE_WITH_HYPHEN_VALUE
                | DATE_WITH_SLASH_VALUE
                ;
            
            // Punctuation

            MarkTransient(OptionalExclude, GroupOrRule, Program);

            MarkPunctuation("-", "/", "(", ")", "..");
            MarkPunctuation(COMMA, ANY, EXCLUDE, MODULUS);

            LanguageFlags |= LanguageFlags.CreateAst;

            Root = Program;
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
