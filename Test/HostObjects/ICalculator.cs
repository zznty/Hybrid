using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Test.HostObjects;

[ComVisible(true)]
[Guid("d970322b-eb5d-40cf-98e0-0e9a6545c41c")]
public partial interface ICalculator
{
    string Version { get; }
    int Add(int a, int b);
    void SubtractAsync(int a, int b, Action<int> callback);
}

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

    public void SubtractAsync(int a, int b, Action<int> callback)
    {
        callback(a - b);
    }
}