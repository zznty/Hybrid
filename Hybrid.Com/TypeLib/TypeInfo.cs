using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;
using DISPPARAMS = Hybrid.Com.Dispatch.DISPPARAMS;
using EXCEPINFO = Hybrid.Com.Dispatch.EXCEPINFO;
using FUNCDESC = Hybrid.Com.Dispatch.FUNCDESC;
using FUNCKIND = Hybrid.Com.Dispatch.FUNCKIND;
using ITypeComp = Hybrid.Com.Dispatch.ITypeComp;
using ITypeInfo = Hybrid.Com.Dispatch.ITypeInfo;
using ITypeLib = Hybrid.Com.Dispatch.ITypeLib;
using TYPEATTR = Hybrid.Com.Dispatch.TYPEATTR;
using TYPEFLAGS = Hybrid.Com.Dispatch.TYPEFLAGS;
using TYPEKIND = Hybrid.Com.Dispatch.TYPEKIND;
using VARDESC = Hybrid.Com.Dispatch.VARDESC;

namespace Hybrid.Com.TypeLib;

[GeneratedComClass]
public partial class TypeInfo(Guid iid, int typeLibIndex, TypeInfo? baseTypeInfo, Func<object>? instanceFactory) : ITypeInfo
{
    private ushort TypeOffset => (ushort)(1 + baseTypeInfo?.TypeOffset ?? 0); 
    public unsafe TYPEATTR* GetTypeAttr()
    {
        var attr = (TYPEATTR*)Marshal.AllocCoTaskMem(sizeof(TYPEATTR));
        
        var typeHandle = HybridTypeLib.Global.ResolveTypeHandleFromIid(iid);
        IComInterfaceDispatchDetails.GetComInterfaceDispatchDetailsFromTypeHandle(typeHandle, out var entriesCount);
        
        attr->guid = iid;
        attr->lcid = 1024;
        attr->cbSizeInstance = sizeof(nint);
        attr->typekind = TYPEKIND.TKIND_DISPATCH;
        attr->cImplTypes = (short)TypeOffset;
        attr->cFuncs = (short)entriesCount;
        attr->wTypeFlags = TYPEFLAGS.TYPEFLAG_FNONEXTENSIBLE | TYPEFLAGS.TYPEFLAG_FDISPATCHABLE;
        
        if (instanceFactory is not null) 
            attr->wTypeFlags |= TYPEFLAGS.TYPEFLAG_FCANCREATE;
        
        return attr;
    }

    public ITypeComp GetTypeComp()
    {
        throw new NotImplementedException();
    }

    public unsafe FUNCDESC* GetFuncDesc(int index)
    {
        var desc = (FUNCDESC*)Marshal.AllocCoTaskMem(sizeof(FUNCDESC));
        
        var typeHandle = HybridTypeLib.Global.ResolveTypeHandleFromIid(iid);
        var entries = IComInterfaceDispatchDetails.GetComInterfaceDispatchDetailsFromTypeHandle(typeHandle, out var entriesCount);
        if (index >= entriesCount) throw new IndexOutOfRangeException();

        var entry = entries[index];

        desc->memid = ComputeMemId(index);
        desc->cParams = (short)(entries->ParameterCount - 1); // minus return type (passed as first parameter)
        desc->funckind = FUNCKIND.FUNC_DISPATCH;
        desc->invkind = entry.Flags;
        
        return desc;
    }

    public unsafe VARDESC* GetVarDesc(int index)
    {
        throw new NotImplementedException();
    }

    public unsafe void GetNames(int memid, string[] rgBstrNames, int cMaxNames, out int pcNames)
    {
        if (memid >> 16 < TypeOffset)
        {
            if (baseTypeInfo is null)
                throw new ArgumentException(null, nameof(memid));

            baseTypeInfo.GetNames(memid, rgBstrNames, cMaxNames, out pcNames);
            return;
        }
        
        var typeHandle = HybridTypeLib.Global.ResolveTypeHandleFromIid(iid);
        var entries = IComInterfaceDispatchDetails.GetComInterfaceDispatchDetailsFromTypeHandle(typeHandle, out var entriesCount);
        
        var index = (memid & 0xffff) - 1;
        if (index >= entriesCount) throw new ArgumentException(null, nameof(memid));
        
        if (cMaxNames < 1) throw new ArgumentException(null, nameof(cMaxNames));
        
        rgBstrNames[0] = Utf16StringMarshaller.ConvertToManaged(entries[index].MemberName)!;
        pcNames = 1;
    }

    public void GetRefTypeOfImplType(int index, out nint href)
    {
        throw new NotImplementedException();
    }

    public IMPLTYPEFLAGS GetImplTypeFlags(int index)
    {
        throw new NotImplementedException();
    }

    public unsafe void GetIDsOfNames(string[] rgszNames, int cNames, int[] pMemId)
    {
        var typeHandle = HybridTypeLib.Global.ResolveTypeHandleFromIid(iid);
        var entriesPtr = IComInterfaceDispatchDetails.GetComInterfaceDispatchDetailsFromTypeHandle(typeHandle, out var entriesCount);

        for (var namesIndex = 0; namesIndex < rgszNames.Length; namesIndex++)
        {
            var namePtr = Utf16StringMarshaller.ConvertToUnmanaged(rgszNames[namesIndex]);
            var success = false;
            try
            {
                for (var i = 0; i < entriesCount; i++)
                {
                    if (Utf16StringEquals((char*)namePtr, (char*)entriesPtr[i].MemberName))
                    {
                        pMemId[namesIndex] = ComputeMemId(i);
                        success = true;
                        break;
                    }
                }
            }
            finally
            {
                Utf16StringMarshaller.Free(namePtr);
            }
            
            if (success) return;
            
            if (baseTypeInfo == null)
                throw new KeyNotFoundException($"Name {rgszNames[namesIndex]} not found in type {Type.GetTypeFromHandle(typeHandle)}.");
            
            baseTypeInfo.GetIDsOfNames(rgszNames, cNames, pMemId);
        }
    }

    private int ComputeMemId(int index)
    {
        return (ushort)index + 1 | TypeOffset << 16;
    }

    private static unsafe bool Utf16StringEquals(char* aPtr, char* bPtr)
    {
        if (aPtr == bPtr)
            return true;
        if (aPtr == null || bPtr == null)
            return false;

        while (*aPtr++ != 0)
        {
            if (*bPtr++ == 0)
                return false;
            
            if (char.ToLowerInvariant(*aPtr) != char.ToLowerInvariant(*bPtr))
                return false;
        }
        
        return true;
    }

    public unsafe int Invoke(nint pvInstance, int memid, InvokeFlags wFlags, ref DISPPARAMS pDispParams, nint pVarResult,
        nint pExcepInfo, nint puArgErr)
    {
        var typeHandle = HybridTypeLib.Global.ResolveTypeHandleFromIid(iid);
        var entriesPtr = IComInterfaceDispatchDetails.GetComInterfaceDispatchDetailsFromTypeHandle(typeHandle, out var entriesCount);

        if (memid >> 16 < TypeOffset)
            return baseTypeInfo?.Invoke(pvInstance, memid, wFlags, ref pDispParams, pVarResult, pExcepInfo, puArgErr) ??
                   throw new ArgumentOutOfRangeException(nameof(memid));
        
        var index = (memid & 0xffff) - 1;
        
        entriesPtr = &entriesPtr[index];

        if (entriesPtr->Flags != wFlags)
            throw new TargetParameterCountException();

        var funcPtr = (nint)StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy.GetIUnknownDerivedDetails(typeHandle)!
                .ManagedVirtualMethodTable[index];

        ref var varResult = ref *(PropVariant*)pVarResult;

        int hr;
        try
        {
            hr = DynamicDispatcher.Dispatch(pvInstance, funcPtr,
                new ReadOnlySpan<DispatchParameterDetails>(entriesPtr->Parameters, entriesPtr->ParameterCount),
                ref pDispParams, ref varResult);
        }
        catch (Exception e)
        {
            ComMarshalSupport.LastException.Value = e;
            hr = e.HResult;
        }

        if (hr == 0 || ComMarshalSupport.LastException.Value is not { } lastException) return hr;
        
        ComMarshalSupport.LastException.Value = null;
            
        ref var exceptionInfo = ref *(EXCEPINFO*)pExcepInfo;
        exceptionInfo.bstrSource = Marshal.StringToBSTR(lastException.Source);
        exceptionInfo.bstrDescription = Marshal.StringToBSTR(lastException.Message);
        exceptionInfo.bstrHelpFile = Marshal.StringToBSTR(lastException.HelpLink);
        exceptionInfo.scode = lastException.HResult;

        return hr;
    }

    public void GetDocumentation(int index, out string strName, out string strDocString, out int dwHelpContext,
        out string strHelpFile)
    {
        throw new NotImplementedException();
    }

    public void GetDllEntry(int memid, InvokeFlags invKind, out string pBstrDllName, out string pBstrName, out short pwOrdinal)
    {
        throw new NotImplementedException();
    }

    public ITypeInfo GetRefTypeInfo(int hRef)
    {
        throw new NotImplementedException();
    }

    public nint AddressOfMember(int memid, InvokeFlags invKind)
    {
        throw new NotImplementedException();
    }

    public unsafe nint CreateInstance(nint pUnkOuter, in Guid riid)
    {
        if (instanceFactory is null) 
            throw new NotSupportedException("This Type does not support CreateInstance.");
        
        var instance = instanceFactory();
        var instancePtr = (nint)ComInterfaceMarshaller<object>.ConvertToUnmanaged(instance);

        var riidRef = riid;
        var hr = Marshal.QueryInterface(instancePtr, ref riidRef, out var result);
        Marshal.ThrowExceptionForHR(hr);

        return result;
    }

    public string? GetMops(int memid)
    {
        throw new NotImplementedException();
    }

    public void GetContainingTypeLib(out ITypeLib ppTLB, out int pIndex)
    {
        ppTLB = HybridTypeLib.Global;
        pIndex = typeLibIndex;
    }

    public unsafe void ReleaseTypeAttr(TYPEATTR* pTypeAttr)
    {
        Marshal.FreeCoTaskMem((nint)pTypeAttr);
    }

    public unsafe void ReleaseFuncDesc(FUNCDESC* pFuncDesc)
    {
        Marshal.FreeCoTaskMem((nint)pFuncDesc);
    }

    public unsafe void ReleaseVarDesc(VARDESC* pVarDesc)
    {
        Marshal.FreeCoTaskMem((nint)pVarDesc);
    }
}