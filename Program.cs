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
        static async Task Main(string[] args)
        {

            // CREATE OR LOAD SETTINGS FILE //

            Settings settings = new Settings();

            string jsonFilePath = "./SaltyBetterSettings.json";
            if (File.Exists(jsonFilePath))
            {
                settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(jsonFilePath));
            }
            else
            {
                updateSettingsJson();
            }

            // CREATE CHROME DRIVER AND OPEN SALTYBET LOGIN PAGE //

            IWebDriver driver;

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
                promptToEditSettingsFile();
                return;
            }

            // LOGIN PROCESS //

            login();

            // COLLECT NECESARY ELEMENTS //

            collectElements();

            // BEGIN ASYNCHRONOUSLY RECIEVING INPUTS //

            Task task = processInputAsync();

            // EVERY 'waitTime' ms, IF BETTING IS AVAILABLE AND HASN'T BEEN DONE ALREADY, BET 'betAmount' SALT ON RANDOM SIDE //

            Random random = new Random();
            bool hasBet = false;

            while (true)
            {
                if (wagerField.Displayed)
                {
                    if (!hasBet)
                    {
                        wagerField.Clear();
                        wagerField.SendKeys(settings.betAmount.ToString());
                        buttons[random.Next(buttons.Length)].Click();
                        hasBet = true;
                    }
                }
                else
                {
                    hasBet = false;
                }
                await Task.Run(() => Thread.Sleep(settings.waitTime));

                exitIfDriverOffSaltyBet();
            }

            void updateSettingsJson()
            {
                JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                jsonSerializerOptions.WriteIndented = true;
                File.WriteAllText(jsonFilePath, JsonSerializer.Serialize<Settings>(settings, jsonSerializerOptions));
            }

            void exitIfDriverOffSaltyBet()
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

            void promptToEditSettingsFile()
            {
                Console.Write(
                    "\n\nAn error likely related to SaltyBetterSettings.json occured." +
                    "\nFill it out completely with both twitch login details and the FOLDER in which ChromeDriver.exe is stored." +
                    "\n\nPress any key to continue...");
                Console.Read();
            }

            async Task processInputAsync()
            {
                while (true)
                {
                    string[] input = await Task.Run(() => Console.ReadLine().Split(" ", StringSplitOptions.RemoveEmptyEntries));

                    switch (input[0])
                    {
                        case "exit":
                            driver.Quit();
                            Environment.Exit(0);
                            break;

                        case "bet":
                            settings.betAmount = Int32.Parse(input[1]);
                            updateSettingsJson();
                            break;

                        case "refresh":
                            // REFRESH THE PAGE WITHOUT BREAKING THINGS
                            break;

                        default:
                            Console.WriteLine("\nInvalid input");
                            break;
                    }
                }
            }

            void collectElements()
            {
            IWebElement wagerField = driver.FindElement(By.Id("wager"));
            IWebElement blueButton = driver.FindElement(By.Id("player1"));
            IWebElement redButton = driver.FindElement(By.Id("player2"));
            IWebElement[] buttons = { redButton, blueButton };
            }

            void login()
            {
            IWebElement emailField = driver.FindElement(By.Id("email"));
            IWebElement passwordField = driver.FindElement(By.Id("pword"));
            IWebElement loginButton = driver.FindElement(By.ClassName("submit"));

            emailField.SendKeys(settings.email);
            passwordField.SendKeys(settings.password);
            loginButton.Click();

            exitIfDriverOffSaltyBet();
            }
        }
    }
}
