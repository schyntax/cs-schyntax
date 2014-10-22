using System.Linq.Expressions;
using Irony.Parsing;

namespace Schyntax
{
    public class SchyntaxGrammar : Grammar
    {
        public SchyntaxGrammar() : base(false) // NOT case sensitive
        {
            //terminals
            // ReSharper disable InconsistentNaming

            var SUNDAY = new RegexBasedTerminal("SUNDAY", @"\bsu(n(day)?)?\b");
            var MONDAY = new RegexBasedTerminal("MONDAY", @"\b(mo|mon|monday)\b");
            var TUESDAY = new RegexBasedTerminal("TUESDAY", @"\b(tu|tue|tuesday)\b");
            var WEDNESDAY = new RegexBasedTerminal("WEDNESDAY", @"\b(we|wed|wednesday)\b");
            var THURSDAY = new RegexBasedTerminal("THURSDAY", @"\b(th|thu|thursday)\b");
            var FRIDAY = new RegexBasedTerminal("FRIDAY", @"\b(fr|friday|friday)\b");
            var SATURDAY = new RegexBasedTerminal("SATURDAY", @"\b(sa|sat|saturday)\b");

            //var MIXIN_IDENTIFIER = new RegexBasedTerminal("MIXIN_IDENTIFIER", @"\$[a-z][a-z0-9_]");

            var POSITIVE_INTEGER = new RegexBasedTerminal("POSITIVE_INTEGER", @"[0-9]+");
            var NEGATIVE_INTEGER = new RegexBasedTerminal("NEGATIVE_INTEGER", @"\-[0-9]+");


            var SECONDS = new RegexBasedTerminal("SECONDS", @"\b(s|sec|second|seconds|secondofminute|secondsofminute)\b");
            var MINUTES = new RegexBasedTerminal("MINUTES", @"\b(m|min|minute|minutes|minuteofhour|minutesofhour)\b");
            var HOURS = new RegexBasedTerminal("HOURS", @"\b(h|hour|hours|hourofday|hoursofday)\b");
            var DAYS_OF_WEEK = new RegexBasedTerminal("DAYS_OF_WEEK", @"\b(day|days|dow|dayofweek|daysofweek)\b");
            var DAYS_OF_MONTH = new RegexBasedTerminal("DAYS_OF_MONTH", @"\b(dom|dayofmonth|daysofmonth)\b");
            var DATES = new RegexBasedTerminal("DATES", @"\b(date|dates)\b");

            var GroupOrExpressionList = new NonTerminal("GroupOrExpressionList");
            var GroupOrExpression = new NonTerminal("GroupOrExpression");
            var Group = new NonTerminal("Group");
            var Expression = new NonTerminal("Expression", typeof(ExpressionNode));
            var ExpressionList = new NonTerminal("ExpressionList");

            var IntegerTypeExpression = new NonTerminal("IntegerTypeExpression");

            var IntegerArgumentList = new NonTerminal("IntegerArgumentList");
            var SecondsExpression = new NonTerminal("SecondsExpression", typeof(ExpressionNode));
            var MinutesExpression = new NonTerminal("MinutesExpression");
            var HoursExpression = new NonTerminal("HoursExpression");

            var DaysOfWeekExpression = new NonTerminal("DaysOfWeekExpression");
            var DaysOfMonthExpression = new NonTerminal("DaysOfMonthExpression");
            var DatesExpression = new NonTerminal("DatesExpression");

            var DateArgumentList = new NonTerminal("DateArgumentList");
            var DateArgument = new NonTerminal("DateArgument");
            var IntegerArgument = new NonTerminal("IntegerArgument");
            var DayArgumentList = new NonTerminal("DayArgumentList");
            var DayArgument = new NonTerminal("DayArgument");
            var OptionalExclude = new NonTerminal("OptionalExclude");
            var ModulusLiteral = new NonTerminal("ModulusLiteral");
            var DateRange =new NonTerminal("DateRange");


            var DateLiteral = new NonTerminal("DateLiteral");
            var DayRange = new NonTerminal("DayRange");
            var IntegerRange = new NonTerminal("IntegerRange");
            var IntegerLiteral = new NonTerminal("IntegerLiteral", typeof(IntegerLiteralNode));
            var DayOrIntegerLiteral = new NonTerminal("DayOrIntegerLiteral");
            var DayLiteral = new NonTerminal("DayLiteral");
            var OptionalModulus = new NonTerminal("OptionalModulus");
            // ReSharper restore InconsistentNaming

            //GroupOrExpressionList.Rule
            //    = GroupOrExpression
            //    | GroupOrExpressionList + GroupOrExpression
            //    | GroupOrExpressionList + "," + GroupOrExpression
                //;

            //GroupOrExpression.Rule =
                //= Group
                //Expression
                ;

            //Group.Rule = 
            //    "(" + ExpressionList + ")"
            //    ;


            //ExpressionList.Rule 
            //    = Expression
            //    | ExpressionList + Expression
            //    | ExpressionList + "," + Expression
            //    ;


            Expression.Rule
                = IntegerTypeExpression + "(" + IntegerArgumentList + ")"
                //| DATES + "(" + DateArgumentList + ")"
                //| DAYS_OF_WEEK + "(" + DayArgumentList + ")"
                ;
            //    | MinutesExpression
            //    | HoursExpression
            //    | DaysOfWeekExpression
            //    | DaysOfMonthExpression
            //    | DatesExpression
                ;

            IntegerTypeExpression.Rule
                = SECONDS
                | MINUTES
                | HOURS
                | DAYS_OF_MONTH
                ;
                
            

            //SecondsExpression.Rule
            //    //= SECONDS + "(" + ")"
            //    //| SECONDS + "(" + IntegerArgumentList + ")"
            //    = SECONDS + "(" + IntegerLiteral + ")"
            //    ;

            //MinutesExpression.Rule 
            //    = MINUTES + "(" + ")"
            //    | MINUTES + "(" + IntegerArgumentList + ")"
            //    ;

            //HoursExpression.Rule 
            //    = HOURS + "(" + ")"
            //    | HOURS + "(" + IntegerArgumentList + ")"
            //    ;

            //DaysOfWeekExpression.Rule
            //    = DAYS_OF_WEEK + "(" + ")"
            //    | DAYS_OF_WEEK + "(" + DayArgumentList + ")"
            //    ;

            //DaysOfMonthExpression.Rule
            //    = DAYS_OF_MONTH + "(" + ")"
            //    | DAYS_OF_MONTH + "(" + IntegerArgumentList + ")"
            //    ;

            //DatesExpression.Rule
            //    = DATES + "(" + ")"
            //    | DATES + "(" + DateArgumentList + ")"
            //    ;

            ///* --- Arguments --- */

            //DateArgumentList.Rule
            //    = DateArgument
            //    | DateArgumentList + "," + DateArgument
            //    ;

            IntegerArgumentList.Rule
                //= IntegerArgument
                //| IntegerArgumentList + "," + IntegerArgument
                = IntegerLiteral
                | Empty
                ;
            
            //DayArgumentList.Rule
            //    = DayArgument
            //    | DayArgumentList + "," + DayArgument
            //    ;

            //DateArgument.Rule
            //    = OptionalExclude + ModulusLiteral
            //    | OptionalExclude + DateRange + OptionalModulus
            //    ;

            //IntegerArgument.Rule
                //= OptionalExclude + ModulusLiteral
                //| OptionalExclude + IntegerRange + OptionalModulus
                //;

            //DayArgument.Rule
            //    = OptionalExclude + ModulusLiteral
            //    | OptionalExclude + DayRange + OptionalModulus
            //    ;

            //OptionalExclude.Rule
            //    = "!"
            //    | Empty
            //    ;

            //OptionalModulus.Rule
            //    = ModulusLiteral
            //    | Empty
            //    ;

            /* --- Ranges --- */

            //DateRange.Rule
            //    = DateLiteral
            //    | DateLiteral + ".." + DateLiteral
            //    ;

            //IntegerRange.Rule
            //    = IntegerLiteral
            //    | IntegerLiteral + ".." + IntegerLiteral
            //    ;

            //DayRange.Rule
            //    = DayOrIntegerLiteral
            //    | DayOrIntegerLiteral + ".." + DayOrIntegerLiteral
            //    ;

            /* --- Literals --- */

            //DateLiteral.Rule
            //    = POSITIVE_INTEGER + "/" + POSITIVE_INTEGER
            //    | POSITIVE_INTEGER + "/" + POSITIVE_INTEGER + "/" + POSITIVE_INTEGER
            //    ;

            //DayLiteral.Rule
            //    = SUNDAY
            //    | MONDAY
            //    | TUESDAY
            //    | WEDNESDAY
            //    | THURSDAY
            //    | FRIDAY
            //    | SATURDAY
            //    ;

            IntegerLiteral.Rule
                = POSITIVE_INTEGER
                | NEGATIVE_INTEGER
                ;

            //DayOrIntegerLiteral.Rule
            //    = DayLiteral
            //    | IntegerLiteral
            //    ;

            //ModulusLiteral.Rule
            //    = "%" + POSITIVE_INTEGER
            //    ;

            // Punctuation
            this.MarkTransient(Group, GroupOrExpression, IntegerArgumentList, IntegerTypeExpression);
            this.MarkPunctuation("/", "(", ")", "..");

            this.LanguageFlags |= LanguageFlags.CreateAst;

            this.Root = Expression;
        }
    }
}
