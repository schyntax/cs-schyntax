using System;
using System.Collections.Generic;
using System.Linq;

namespace Schyntax
{
    public class Schedule
    {
        private readonly string _originalString;
        private readonly List<RuleGroup> _ruleGroups;

        public Schedule(string text)
        {
            _originalString = text;
            _ruleGroups = ScheduleBuilder.ParseAndCompile(text);
        }
        
        public string OriginalString
        {
            get { return _originalString; }
        }
    
        public DateTime? Next(DateTime after)
        {
            return _ruleGroups.Select(g => g.GetEvent(after, true)).Min();
        }

        public DateTime? Previous(DateTime atOrBefore)
        {
            return _ruleGroups.Select(g => g.GetEvent(atOrBefore, false)).Max();
        }

    }
}
