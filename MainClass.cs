using System.Threading.Tasks;

namespace SaltyBetter;

class MainClass
{
    static async Task Main(string[] args)
    {
        Program program = new Program();
        await program.Run();
    }
}

