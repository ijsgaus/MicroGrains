using System;
using System.Threading.Tasks;
using Orleans;
using PlayInterfacesA;

namespace PlayGrainsA
{
    public class TestGrain : Grain, ITestGrain
    {
        public async Task<int> Add(Int32 a, Int32 b)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            return a + b;
        }
    }
}