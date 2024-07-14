using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Test.HostObjects;

[ComVisible(true)]
[Guid("699f36d9-f516-46e3-a040-baa2436ed40a")]
[SharedHostObject<ICalculator, DuplicateCalculator>]
public partial class DuplicateCalculator : SharedHostObject, ICalculator
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