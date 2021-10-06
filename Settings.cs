namespace SaltyBetter;

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
