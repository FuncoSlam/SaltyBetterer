using System;
using System.Threading;
using System.IO;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace SaltyBetter
{
    class Program
    {
        static void Main(string[] args)
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
                File.WriteAllText(jsonFilePath, JsonSerializer.Serialize<Settings>(settings));
            }

            // CREATE CHROME DRIVER AND OPEN SALTYBET LOGIN PAGE //

            IWebDriver driver;

            if (File.Exists(settings.chromeDriverPath + "/ChromeDriver.exe"))
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

            IWebElement emailField = driver.FindElement(By.Id("email"));
            IWebElement passwordField = driver.FindElement(By.Id("pword"));
            IWebElement loginButton = driver.FindElement(By.ClassName("submit"));

            emailField.SendKeys(settings.email);
            passwordField.SendKeys(settings.password);
            loginButton.Click();

            exitIfDriverOffSaltyBet();

            // COLLECT NECESARY ELEMENTS //

            IWebElement wagerField = driver.FindElement(By.Id("wager"));
            IWebElement blueButton = driver.FindElement(By.Id("player1"));
            IWebElement redButton = driver.FindElement(By.Id("player2"));
            IWebElement[] buttons = { redButton, blueButton };

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
                Thread.Sleep(settings.waitTime);

                exitIfDriverOffSaltyBet();
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
                Console.Write("\n\nAn error likely related to SaltyBetterSettings.json occured.\nFill it out completely with both twitch login details and the FOLDER in which ChromeDriver.exe is stored.\n\nPress any key to continue...");
                Console.Read();
            }
        }


        class Settings
        {
            public string email { get; set; }
            public string password { get; set; }
            public int waitTime { get; set; }
            public string chromeDriverPath { get; set; }
            public int betAmount { get; set; }

            public Settings()
            {
                email = "";
                password = "";
                waitTime = 1000;
                chromeDriverPath = "";
                betAmount = 1;
            }
        }
    }
}

