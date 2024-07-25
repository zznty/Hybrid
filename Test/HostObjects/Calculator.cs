using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Test.HostObjects;

[ComVisible(true)]
[Guid("5343131b-7185-4402-858a-8520f63c4712")]
[SharedHostObject<ICalculator, Calculator>]
public partial class Calculator : SharedHostObject, ICalculator2
{
    public string Version => typeof(Calculator).Assembly.GetName().Version?.ToString() ?? "0.0.0.0";
    
    public int Add(int a, int b)
    {
        return a + b;
    }

    public async void SubtractAsync(int a, int b, Action<int, bool> callback)
    {
        await Task.Delay(2000);
        var result = a - b;
        callback(result, result > 0);
    }

    public int[] Pow(int a, int b)
    {
        var arr = new int[b - 1];
        for (var i = 0; i < arr.Length; i++)
            arr[i] = i == 0 ? a : arr[i - 1] * arr[i - 1];
        
        return arr;
    }

    public int Max(int a, int b) => a > b ? a : b;
}