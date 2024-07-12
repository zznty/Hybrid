using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;
using Hybrid.Com.TypeLib;
using Hybrid.Common;

namespace Hybrid.Com;

[GeneratedComClass]
[Guid("75B96AEC-D0DD-42CB-9E98-B91D06CCDEA0")]
[ComVisible(true)]
public partial class SharedHostObject : IDispatch, ISharedHostObject
{
    private static Guid? _iid;
    private Guid Iid => _iid ??= GetIid(this);

    private static unsafe Guid GetIid(SharedHostObject sharedHostObject)
    {
        var interfaceEntriesPtr = StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy.GetComExposedTypeDetails(sharedHostObject.GetType()
            .TypeHandle)!.GetComInterfaceEntries(out var count);

        var entries = new ReadOnlySpan<ComWrappers.ComInterfaceEntry>(interfaceEntriesPtr, count);

        return entries[^1].IID;
    }

    public int GetTypeInfoCount() => GetType() != typeof(SharedHostObject) ? 1 : 0;

    public ITypeInfo GetTypeInfo(int iTInfo, int lcid)
    {
        var guid = Iid;
        return HybridTypeLib.Global.GetTypeInfoOfGuid(ref guid);
    }

    public void GetIDsOfNames(in Guid riid, string[] rgszNames, int cNames, int lcid, int[] rgdispid)
    {
        var typeInfo = GetTypeInfo(0, lcid);
        
        typeInfo.GetIDsOfNames(rgszNames, cNames, rgdispid);
    }

    public int Invoke(int dispIdMember, in Guid riid, int lcid, InvokeFlags wFlags, ref DISPPARAMS pDispParams, nint pVarResult,
        nint pExcepInfo, nint puArgErr)
    {
        var typeInfo = GetTypeInfo(0, lcid);

        if (!ComWrappers.TryGetComInstance(this, out var instance))
            instance = ComMarshalSupport.Wrapper.GetOrCreateComInterfaceForObject(this, CreateComInterfaceFlags.None);
        
        var guid = Iid;
        Marshal.QueryInterface(instance, ref guid, out var implInstance);
        
        return typeInfo.Invoke(implInstance, dispIdMember, wFlags, ref pDispParams, pVarResult, pExcepInfo, puArgErr);
    }
    
}