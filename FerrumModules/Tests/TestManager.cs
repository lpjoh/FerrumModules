using System;

using Crossfrog.Ferrum.Engine;

namespace Crossfrog.Ferrum.Tests
{
    public class TestManager : Manager
    {
        private int counter = 0;

        public override void Init()
        {
            Console.Write("Manager Initalized! Yay!!!!");
        }

        public override void Update(float delta)
        {
            counter++;
            Console.WriteLine(counter);
        }
    }
}
