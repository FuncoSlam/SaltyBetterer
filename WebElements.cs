using System.Collections.Generic;
using OpenQA.Selenium;

namespace SaltyBetter
{
    class WebElements
    {
        public IWebElement wagerField;
        public IWebElement blueButton;
        public IWebElement redButton;
        public List<IWebElement> buttons;

        public WebElements(IWebDriver driver)
        {
            CollectElements(driver);
        }

        public void CollectElements(IWebDriver driver)
        {
            wagerField = driver.FindElement(By.Id("wager"));
            blueButton = driver.FindElement(By.Id("player1"));
            redButton = driver.FindElement(By.Id("player2"));
            buttons = new List<IWebElement>() { blueButton, redButton };
        }
    }
}