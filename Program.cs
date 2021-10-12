using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SaltyBetter;

class Program
{
    public Settings settings = new();

    public IWebDriver driver;

    readonly string jsonFilePath = "./SaltyBetterSettings.json";

    Random random = new();
    bool hasBet = false;
    public bool syncRefresh = false;

    public async Task Run()
    {

        // CREATE OR LOAD SETTINGS FILE //

        string jsonFilePath = "./SaltyBetterSettings.json";
        if (File.Exists(jsonFilePath))
        {
            settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(jsonFilePath));
        }
        else
        {
            UpdateSettingsJson();
        }

        // CREATE CHROME DRIVER AND OPEN SALTYBET LOGIN PAGE //

        if (File.Exists($"{settings.chromeDriverPath}/ChromeDriver.exe"))
        {
            ChromeOptions options = new();
            options.AddArgument("--silent");
            options.AddArgument("--log-level=3");
            driver = new ChromeDriver(settings.chromeDriverPath, options)
            {
                Url = "https://www.saltybet.com/authenticate?signin=1"
            };
        }
        else
        {
            PromptToEditSettingsFile();
            return;
        }

        // LOGIN PROCESS //

        LoginInfo loginInfo = new(settings);
        loginInfo.Login(driver);
        ExitIfDriverOffSaltyBet();
        Console.Clear();

        // COLLECT NECESARY ELEMENTS //

        SaltyWebElements webElements = new(driver);

        // BEGIN ASYNCHRONOUSLY RECIEVING INPUTS //

        Task task = ProcessInputAsync(driver, settings);

        // EVERY 'waitTime' ms, IF BETTING IS AVAILABLE AND HASN'T BEEN DONE ALREADY, BET 'betAmount' SALT ON RANDOM SIDE //

        while (true)
        {
            if (webElements.wagerField.Displayed)
            {
                if (!hasBet)
                {
                    webElements.wagerField.Clear();
                    webElements.wagerField.SendKeys(settings.betAmount.ToString());
                    webElements.buttons[random.Next(webElements.buttons.Count)].Click();
                    hasBet = true;
                }
            }
            else
            {
                hasBet = false;
            }

            await Task.Run(() => Thread.Sleep(settings.waitTime));

            if (syncRefresh)
            {
                Refresh(webElements);
                syncRefresh = !syncRefresh;
            }

            ExitIfDriverOffSaltyBet();
        }
    }

    private void Refresh(SaltyWebElements webElements)
    {
        driver.Navigate().Refresh();
        Thread.Sleep(1000); // Needed to prevent errors collecting elements before the page has refreshed. 1s may be overkill.
        webElements.CollectElements(driver);
    }

    public void ExitIfDriverOffSaltyBet()
    {
        try
        {
            driver.FindElement(By.Id("sbettorswrapper"));
        }
        catch
        {
            driver.Quit();
            Environment.Exit(0);
        }
    }

    static string[] UserPrompt()
    {
        Console.Write(">>> ");
        return Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries);
    }

    public void UpdateSettingsJson()
    {
        JsonSerializerOptions jsonSerializerOptions = new();
        jsonSerializerOptions.WriteIndented = true;
        File.WriteAllText(jsonFilePath, JsonSerializer.Serialize<Settings>(settings, jsonSerializerOptions));
    }

    static void PromptToEditSettingsFile()
    {
        Console.Write(
            "\n\nAn error likely related to SaltyBetterSettings.json occured." +
            "\nFill it out with both your SaltyBet login details and an absolute path to the FOLDER in which ChromeDriver.exe has been installed." +
            "\n\nPress any key to continue...");
        Console.Read();
    }

    async Task ProcessInputAsync(IWebDriver driver, Settings settings)
    {
        InputParser inputParser = new(this);

        while (true)
        {
            string[] input = await Task.Run(() => UserPrompt());

            inputParser.Parse(input);

            Console.WriteLine(LineBreak("-".ToCharArray()[0]));
        }
    }

    static string LineBreak(char lineBreakChar)
    {
        string tabs = new(lineBreakChar, Console.BufferWidth);
        return $"{tabs}\n";
    }
}
