using System;
using System.Text.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace SaltyBetter;

class InputParser
{
    private readonly Program program;
    private readonly Command[] commands;
    private List<Bookmark> bookmarks;
    private string bookmarkFilePath;
    private JsonSerializerOptions jsonSerializerOptions;

    public InputParser(Program program)
    {
        this.program = program;

        commands = new Command[] {
            new Command("exit", new Action(ExitCommand), "Exits the program"),
            new Command("refresh", new Action(RefreshCommand), "Safely refreshes the SaltyBet page"),
            new Command("clear", new Action(ClearCommand), "Clears text from the console"),
            new Command("bet", new Action<string>(BetCommand), "Changes the bet amount permanently, takes a number"),
            new Command("help", new Action(HelpCommand), "Displays all commands with descriptions, this is probably that one"),
            new Command("bookmark", new Action(BookmarkCommand), "Creates a bookmark of current salt"),
            new Command("clearmarks", new Action(ClearBookmarksCommand), "Clears all bookmarks on record"),
            new Command("savemarks", new Action(SaveBookmarksCommand), "Saves current bookmarks to a file"),
            new Command("loadmarks", new Action(LoadBookmarksCommand), "Overwrites current bookmarks with bookmarks file"),
            new Command("viewmarks", new Action(ViewmarkCommand), "Displays all bookmarks along with current salt")
        };

        bookmarkFilePath = "./bookmarks.json";
        bookmarks = new();

        jsonSerializerOptions = new()
        {
            WriteIndented = true
        };
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
        bookmarks.Add(new(bookmarkedSalt, timeOfBookmark));
    }

    private void ViewmarkCommand()
    {
        int currentSalt = program.GetCurrentSalt();

        foreach (Bookmark bookmark in bookmarks)
        {
            string formattedLine = string.Format("${0,-14} - {1}", bookmark.Salt, bookmark.Time);
            Console.WriteLine(formattedLine);
        }
        string currentLine = string.Format("${0,-14} - {1}", currentSalt, "NOW");
        Console.WriteLine(currentLine);
    }

    private void ClearBookmarksCommand()
    {
        bookmarks.Clear();
    }

    private void LoadBookmarksCommand()
    {
        bookmarks.Clear();

        string text = File.ReadAllText(bookmarkFilePath);

        bookmarks = JsonSerializer.Deserialize<List<Bookmark>>(text, jsonSerializerOptions);
    }

    private void SaveBookmarksCommand()
    {
        string text = JsonSerializer.Serialize(bookmarks, jsonSerializerOptions);

        File.WriteAllText(bookmarkFilePath, text);
    }

    private struct Bookmark
    {
        public int Salt { get; init; }
        public DateTime Time { get; init; }
        public string Comment { get; init; }

        public Bookmark(int salt, DateTime time)
        {
            Salt = salt;
            Time = time;
            Comment = string.Empty;
        }

        public Bookmark(int salt, DateTime time, string comment)
        {
            Salt = salt;
            Time = time;
            Comment = comment;
        }
    }
}