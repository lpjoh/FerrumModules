using System;

namespace Crossfrog.Ferrum.Tests
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new TestGame();
            game.Run();
        }
    }
}
