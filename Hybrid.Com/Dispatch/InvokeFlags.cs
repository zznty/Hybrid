namespace Hybrid.Com.Dispatch;

[Flags]
public enum InvokeFlags : short
{
    DispatchMethod = 1,
    DispatchPropertyGet = 2,
    DispatchPropertyPut = 4,
    DispatchPropertyPutRef = 8
}