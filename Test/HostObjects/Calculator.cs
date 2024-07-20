using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Test.HostObjects;

[ComVisible(true)]
[Guid("5343131b-7185-4402-858a-8520f63c4712")]
[SharedHostObject<ICalculator, Calculator>]
public partial class Calculator : SharedHostObject, ICalculator
{
    public string Version => typeof(Calculator).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
    
    public int Add(int a, int b)
    {
        return a + b;
    }

    public async void SubtractAsync(int a, int b, Action<int> callback)
    {
        await Task.Delay(2000);
        callback(a - b);
    }
}