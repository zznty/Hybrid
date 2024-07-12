using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;

namespace Hybrid.Com.TypeLib;

[GeneratedComClass]
public partial class HybridTypeLib : ITypeLib
{
    private static readonly Guid TypeLibIid = new("687cce8c-7c74-472a-a75c-25fa45e5fa8d");
    private readonly List<Guid> _indexIidMap = [];
    private readonly Dictionary<Guid, (ITypeInfo Info, TYPEKIND Kind, RuntimeTypeHandle Handle)> _typeMap = [];
    private readonly HashSet<string> _typeNames = new(StringComparer.OrdinalIgnoreCase);

    public static HybridTypeLib Global { get; } = new();

    private HybridTypeLib()
    {
    }

    public RuntimeTypeHandle ResolveTypeHandleFromIid(Guid iid) => _typeMap[iid].Handle;
    
    public unsafe void RegisterComInterface<TInterface, TCoClass>() where TCoClass : class, TInterface
    {
        if (StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy.GetIUnknownDerivedDetails(typeof(TInterface).TypeHandle) is not { } typeDetails)
            throw new NotSupportedException($"Cannot resolve COM type details for {typeof(TInterface)}");
        
        if (StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy.GetComExposedTypeDetails(typeof(TCoClass).TypeHandle) is not { } classTypeDetails)
            throw new NotSupportedException($"Cannot resolve COM type details for {typeof(TCoClass)}");

        if (_typeMap.ContainsKey(typeDetails.Iid)) return;
        
        var entriesPtr = classTypeDetails.GetComInterfaceEntries(out var entryCount);
        var entries = new ReadOnlySpan<ComWrappers.ComInterfaceEntry>(entriesPtr, entryCount);

        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            
            if (entry.IID != typeDetails.Iid) continue;
            
            _indexIidMap.Add(entry.IID);
            _typeMap.Add(entry.IID,
                (new TypeInfo(entry.IID, _indexIidMap.Count, i == 0 ? null : _typeMap[entries[i - 1].IID].Info), TYPEKIND.TKIND_INTERFACE, typeof(TInterface).TypeHandle));
        }
    }
    
    public int GetTypeInfoCount() => _typeMap.Count;

    public ITypeInfo GetTypeInfo(int index) => _typeMap[_indexIidMap[index]].Info;

    public TYPEKIND GetTypeInfoType(int index) => _typeMap[_indexIidMap[index]].Kind;

    public ITypeInfo GetTypeInfoOfGuid(ref Guid guid) => _typeMap[guid].Info;

    public unsafe TYPELIBATTR* GetLibAttr()
    {
        ref var attr = ref *(TYPELIBATTR*)Marshal.AllocCoTaskMem(sizeof(TYPELIBATTR));
        
        attr.guid = TypeLibIid;
        attr.lcid = 1024;
        attr.syskind = SYSKIND.SYS_WIN64;
        attr.wMajorVerNum = 1;
        
        return (TYPELIBATTR*)Unsafe.AsPointer(ref attr);
    }

    public ITypeComp GetTypeComp()
    {
        throw new NotImplementedException();
    }

    public void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext,
        out string strHelpFile)
    {
        throw new NotImplementedException();
    }

    public bool IsName(ref string szNameBuf, int lHashVal)
    {
        if (!_typeNames.TryGetValue(szNameBuf, out var value)) return false;
        
        szNameBuf = value;
        return true;
    }

    public void FindName(string szNameBuf, int lHashVal, ITypeInfo[] ppTInfo, int[] rgMemId, ref short pcFound)
    {
        throw new NotImplementedException();
    }

    public unsafe void ReleaseTLibAttr(TYPELIBATTR* pTLibAttr)
    {
        Marshal.FreeCoTaskMem((nint)pTLibAttr);
    }
}