using System.Runtime.InteropServices;
using Hybrid.Common;

namespace Test.HostObjects;

[ComVisible(true)]
[Guid("d970322b-eb5d-40cf-98e0-0e9a6545c41c")]
[SharedHostObjectDefinition]
public partial interface ICalculator
{
    string Version { get; }
    int Add(int a, int b);
    void SubtractAsync(int a, int b, Action<int, bool> callback);
    int[] Pow(int a, int b);
}

[ComVisible(true)]
[Guid("41c86542-1197-4a5b-9274-988e46a8e35d")]
[SharedHostObjectDefinition]
public partial interface ICalculator2 : ICalculator 
{
    int Max(int a, int b);
    object? TestObject { get; }
}