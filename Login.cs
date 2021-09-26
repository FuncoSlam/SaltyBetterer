using OpenQA.Selenium;

namespace SaltyBetter
{
    class LoginInfo
    {
        IWebElement emailField;
        IWebElement passwordField;
        IWebElement loginButton;
        Program program;

        public LoginInfo(Program _program)
        {
            program = _program;
        }

        public void Login(IWebDriver driver)
        {
            emailField = driver.FindElement(By.Id("email"));
            passwordField = driver.FindElement(By.Id("pword"));
            loginButton = driver.FindElement(By.ClassName("submit"));

            emailField.SendKeys(program.settings.email);
            passwordField.SendKeys(program.settings.password);
            loginButton.Click();

            program.ExitIfDriverOffSaltyBet(driver);
        }
    }
}