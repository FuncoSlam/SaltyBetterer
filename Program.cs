using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SaltyBetter
{
    class Program
    {
        Settings settings = new Settings();

        IWebDriver driver;

        string jsonFilePath = "./SaltyBetterSettings.json";

        Random random = new Random();
        bool hasBet = false;
        bool syncRefresh = false;

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
                ChromeOptions options = new ChromeOptions();
                options.AddArgument("--silent");
                options.AddArgument("--log-level=3");
                driver = new ChromeDriver(settings.chromeDriverPath, options);
                driver.Url = "https://www.saltybet.com/authenticate?signin=1";
            }
            else
            {
                PromptToEditSettingsFile();
                return;
            }

            // LOGIN PROCESS //

            LoginInfo loginInfo = new LoginInfo(settings);
            loginInfo.Login(driver);
            ExitIfDriverOffSaltyBet();

            // COLLECT NECESARY ELEMENTS //

            SaltyWebElements webElements = new SaltyWebElements(driver);

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

        string[] UserPrompt()
        {
            Console.Write(">>> ");
            return Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries);
        }

        void UpdateSettingsJson()
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.WriteIndented = true;
            File.WriteAllText(jsonFilePath, JsonSerializer.Serialize<Settings>(settings, jsonSerializerOptions));
        }

        void PromptToEditSettingsFile()
        {
            Console.Write(
                "\n\nAn error likely related to SaltyBetterSettings.json occured." +
                "\nFill it out completely with both twitch login details and the FOLDER in which ChromeDriver.exe has been installed." +
                "\n\nPress any key to continue...");
            Console.Read();
        }

        async Task ProcessInputAsync(IWebDriver driver, Settings settings)
        {
            while (true)
            {
                string[] input = await Task.Run(() => UserPrompt());

                switch (input[0])
                {
                    case "exit":
                        driver.Quit();
                        Environment.Exit(0);
                        break;

                    case "bet":
                        settings.betAmount = Int32.Parse(input[1]);
                        UpdateSettingsJson();
                        break;

                    case "refresh":
                        syncRefresh = true;
                        break;

                    default:
                        Console.WriteLine("\nInvalid input");
                        break;
                }
                Console.WriteLine(LineBreak("-".ToCharArray()[0]));
            }
        }

        string LineBreak(char lineBreakChar)
        {
            string tabs = new string(lineBreakChar, Console.BufferWidth);
            return $"{tabs}\n";
        }
    }
}