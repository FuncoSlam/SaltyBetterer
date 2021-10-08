using System;

namespace SaltyBetter;

class Command
{
    public string Name { get; }
    public Delegate Function { get; }
    public string Description = null;

    public Command(string name, Action function)
    {
        Name = name;
        Function = function;
    }

    public Command(string name, Action function, string description)
    {
        Name = name;
        Function = function;
        Description = description;
    }

    public Command(string name, Action<string> function)
    {
        Name = name;
        Function = function;
    }

    public Command(string name, Action<string> function, string description)
    {
        Name = name;
        Function = function;
        Description = description;
    }
}