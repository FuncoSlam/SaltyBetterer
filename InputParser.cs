using System;
using System.Collections;
using System.Collections.Generic;

namespace SaltyBetter;

class InputParser
{
    private readonly Program program;
    private readonly Command[] commands;
    private List<(int salt, DateTime time)> bookmarks;

    public InputParser(Program program)
    {
        this.program = program;

        commands = new Command[] {
            new ("exit", new Action(ExitCommand), "Exits the program"),
            new ("refresh", new Action(RefreshCommand), "Safely refreshes the SaltyBet page"),
            new ("clear", new Action(ClearCommand), "Clears text from the console"),
            new ("bet", new Action<string>(BetCommand), "Changes the bet amount permanently, takes a number"),
            new ("help", new Action(HelpCommand), "Displays all commands with descriptions, this is probably that one"),
            new ("bookmark", new Action(BookmarkCommand), "Creates a bookmark of current salt"),
            new ("clearbookmarks", new Action(ClearBookmarksCommand), "Clears all bookmarks on record"),
            new ("progress", new Action(ProgressCommand), "Displays all bookmarks along with current salt")
        };

        bookmarks = new();
    }

    public void Parse(string[] input)
    {
        Console.Write($"\n");

        foreach (Command command in commands)
        {
            if (input.Length == 0)
            {
                DefaultCommand();
                return;
            }
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
        program.settings.BetAmount = Int32.Parse(bet);
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

    private void HelpCommand()
    {
        foreach (Command command in commands)
        {
            string commandNamePlusSpaces = $"\"{command.Name}\"".PadRight(Command.MaxCommandNameLength);
            Console.Write($"{commandNamePlusSpaces} - {command.Description}\n");
        }
        Console.Write($"\n");
    }

    private void BookmarkCommand()
    {
        int bookmarkedSalt = program.GetCurrentSalt();
        DateTime timeOfBookmark = DateTime.Now;
        bookmarks.Add((bookmarkedSalt, timeOfBookmark));
    }

    private void ProgressCommand()
    {
        int currentSalt = program.GetCurrentSalt();

        foreach ((int salt, DateTime time)bookmark in bookmarks)
        {
            string formattedLine = string.Format("${0,-14} - {1}", bookmark.salt, bookmark.time);
            Console.WriteLine(formattedLine);
        }
        string currentLine = string.Format("${0,-14} - {1}", currentSalt, DateTime.Now);
        Console.WriteLine(currentLine);
    }

    private void ClearBookmarksCommand()
    {
        bookmarks.Clear();
    }
}