namespace SaltyBetter;

class Settings
{
    public string Email { get; set; }
    public string Password { get; set; }
    public int WaitTime { get; set; }
    public int BetAmount { get; set; }

    public Settings()
    {
        Email = "";
        Password = "";
        WaitTime = 1000;
        BetAmount = 1;
    }
}
