using System.Threading.Tasks;
using Orleans;

namespace PlayInterfacesA
{
    public interface ITestGrain : IGrainWithIntegerKey
    {
        Task<int> Add(int a, int b);
    }
}