namespace SaltyBetter;

class Settings
{
    public string Email { get; set; }
    public string Password { get; set; }
    public int WaitTime { get; set; }
    public string ChromeDriverPath { get; set; }
    public int BeAmount { get; set; }

    public Settings()
    {
        Email = "";
        Password = "";
        WaitTime = 1000;
        ChromeDriverPath = "";
        BeAmount = 1;
    }
}
