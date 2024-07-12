using System.Runtime.InteropServices;
using Hybrid.Com;
using Hybrid.Common;

namespace Test.HostObjects;

[ComVisible(true)]
[Guid("d970322b-eb5d-40cf-98e0-0e9a6545c41c")]
public partial interface ICalculator
{
    int Add(int a, int b);
}

[ComVisible(true)]
[Guid("5343131b-7185-4402-858a-8520f63c4712")]
[SharedHostObject<ICalculator, Calculator>]
public partial class Calculator : SharedHostObject, ICalculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}