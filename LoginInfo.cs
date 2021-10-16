using OpenQA.Selenium;

namespace SaltyBetter;

class LoginInfo
{
    private IWebElement emailField;
    private IWebElement passwordField;
    private IWebElement loginButton;

    readonly Settings Settings;

    public LoginInfo(Settings Settings)
    {
        this.Settings = Settings;
    }

    public void Login(IWebDriver driver)
    {
        emailField = driver.FindElement(By.Id("email"));
        passwordField = driver.FindElement(By.Id("pword"));
        loginButton = driver.FindElement(By.ClassName("submit"));

        emailField.SendKeys(Settings.Email);
        passwordField.SendKeys(Settings.Password);
        loginButton.Click();
    }
}
