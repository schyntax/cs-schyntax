# C# Schyntax

[![NuGet version](https://badge.fury.io/nu/Schyntax.svg)](http://badge.fury.io/nu/Schyntax)
[![Build status](https://ci.appveyor.com/api/projects/status/y1ij5ty5hv2gx1qd/branch/master?svg=true)](https://ci.appveyor.com/project/bretcope/cs-schyntax/branch/master)

[Schyntax](https://github.com/schyntax/schyntax) is a domain-specific language for defining event schedules in a terse, but readable, format. For example, if you want something to run every five minutes during work hours on weekdays, you could write `days(mon..fri) hours(9..<17) min(*%5)`. This project is the reference implementation of Schyntax.

## Usage

> This library is __NOT__ a scheduled task runner. Most likely, you'll want to use [Schtick](https://github.com/schyntax/cs-schtick), which is a scheduled task runner built on top of Schyntax, instead of using this library directly.

Schyntax exposes the `Schedule` class.

```csharp
using Schyntax;

var sch = new Schedule("min(*%2)");
```

### Schedule#Next

Accepts an optional `after` argument in the form of a `DateTime`. If no argument is provided, the current time is used.

Returns a `DateTime` object representing the next timestamp which matches the scheduling criteria. The date will always be greater than, never equal to, `after`. If no timestamp could be found which matches the scheduling criteria, a `ValidTimeNotFoundException` error is thrown, which generally indicates conflicting scheduling criteria (explicitly including and excluding the same day or time).

```csharp
var nextEventTime = sch.Next();
```

### Schedule#Previous

Same as `Previous()` accept that its return value will be less than or equal to the current time or optional `atOrBefore` argument. This means that if you want to find the last n-previous events, you should subtract at least a millisecond from the result before passing it back to the function.

```csharp
var prevEventTime = sch.Previous(); 
```
