using OpenQA.Selenium;

namespace SaltyBetter;

class LoginInfo
{
    IWebElement emailField;
    IWebElement passwordField;
    IWebElement loginButton;
    Settings settings;

    public LoginInfo(Settings _settings)
    {
        settings = _settings;
    }

    public void Login(IWebDriver driver)
    {
        emailField = driver.FindElement(By.Id("email"));
        passwordField = driver.FindElement(By.Id("pword"));
        loginButton = driver.FindElement(By.ClassName("submit"));

        emailField.SendKeys(settings.email);
        passwordField.SendKeys(settings.password);
        loginButton.Click();
    }
}
