# C# Schyntax

[![NuGet version](https://badge.fury.io/nu/Schyntax.svg)](http://badge.fury.io/nu/Schyntax)
[![Build status](https://ci.appveyor.com/api/projects/status/y1ij5ty5hv2gx1qd/branch/master?svg=true)](https://ci.appveyor.com/project/bretcope/cs-schyntax/branch/master)

[Schyntax](https://github.com/schyntax/schyntax) is a domain-specific language for defining event schedules in a terse, but readable, format. For example, if you want something to run every five minutes during work hours on weekdays, you could write `days(mon..fri) hours(9..17) min(*%5)`. This project is the reference implementation of Schyntax.

## Usage

The easiest way to setup scheduled tasks is with the `Schtick` class. Best practice is to make a singleton instance, though there's nothing harmful about creating more than one.

```csharp
var schtick = new Schtick();

// setup an exception handler so we know when tasks blow up
schtick.OnTaskException += (task, exception) => LogException(ex);

// add a task which will call DoSomeTask every hour at 15 minutes past the hour
schtick.AddTask("unique-task-name", "hour(*) min(15)", (task, timeIntendedToRun) => DoSomeTask());
```

> For complete documentation of schedule format language itself, see the [Schyntax](https://github.com/schyntax/schyntax) project.

`AddTask` has several optional arguments which are documented in [Schtick.cs](https://github.com/schyntax/cs-schyntax/blob/master/Schyntax/Schtick.cs) and will show up in intellisense.

If you don't need an actual task runner, you can also use the `Schedule` class directly.

```csharp
var sch = new Schedule("hours(16) days(mon..fri)");

// get the next two times applicable to the schedule
var next1 = sch.Next();
var next2 = sch.Next(next1);

// get the previous two times applicable to the schedule
var prev1 = sch.Previous();
var prev2 = sch.Previous(prev1);
```
