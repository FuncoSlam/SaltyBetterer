using System;

namespace SaltyBetter;

class InputParser
{
    private Program program;
    private Command[] commands;

    public InputParser(Program _program)
    {
        program = _program;

        commands = new Command[] {
            new ("exit", new Action(ExitCommand)),
            new ("refresh", new Action(RefreshCommand)),
            new ("clear", new Action(ClearCommand)),
            new ("bet", new Action<string>(BetCommand))
        };
    }

    public void Parse(string[] input)
    {
        foreach (Command command in commands)
        {
            if (input[0] == command.Name)
            {
                if (input.Length > 1)
                {
                    try
                    {
                        command.Function.DynamicInvoke(input[1]);
                        return;
                    }
                    catch
                    {
                        DefaultCommand();
                        return;
                    }
                }
                else
                {
                    try
                    {
                        command.Function.DynamicInvoke();
                        return;
                    }
                    catch
                    {
                        DefaultCommand();
                        return;
                    }
                }

            }
        }
        DefaultCommand();
        return;
    }

    private void BetCommand(string bet)
    {
        program.settings.betAmount = Int32.Parse(bet);
        program.UpdateSettingsJson();
    }

    private void ExitCommand()
    {
        program.driver.Quit();
        Environment.Exit(0);
    }

    private void RefreshCommand()
    {
        program.syncRefresh = true;
    }

    private void ClearCommand()
    {
        Console.Clear();
    }

    private void DefaultCommand()
    {
        Console.WriteLine("\nInvalid input");
    }
}