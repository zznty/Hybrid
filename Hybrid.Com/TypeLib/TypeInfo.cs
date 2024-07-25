using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;
using Hybrid.Com.Dispatch;
using DISPPARAMS = Hybrid.Com.Dispatch.DISPPARAMS;
using EXCEPINFO = Hybrid.Com.Dispatch.EXCEPINFO;
using FUNCDESC = Hybrid.Com.Dispatch.FUNCDESC;
using ITypeComp = Hybrid.Com.Dispatch.ITypeComp;
using ITypeInfo = Hybrid.Com.Dispatch.ITypeInfo;
using ITypeLib = Hybrid.Com.Dispatch.ITypeLib;
using TYPEATTR = Hybrid.Com.Dispatch.TYPEATTR;
using VARDESC = Hybrid.Com.Dispatch.VARDESC;

namespace Hybrid.Com.TypeLib;

[GeneratedComClass]
public partial class TypeInfo(Guid iid, int typeLibIndex, TypeInfo? baseTypeInfo) : ITypeInfo
{
    private ushort TypeOffset => (ushort)(1 + baseTypeInfo?.TypeOffset ?? 0); 
    public unsafe TYPEATTR* GetTypeAttr()
    {
        throw new NotImplementedException();
    }

    public ITypeComp GetTypeComp()
    {
        throw new NotImplementedException();
    }

    public unsafe FUNCDESC* GetFuncDesc(int index)
    {
        throw new NotImplementedException();
    }

    public unsafe VARDESC* GetVarDesc(int index)
    {
        throw new NotImplementedException();
    }

    public void GetNames(int memid, string[] rgBstrNames, int cMaxNames, out int pcNames)
    {
        throw new NotImplementedException();
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
                    if (Utf16StringEquals(namePtr, entriesPtr[i].MemberName))
                    {
                        pMemId[namesIndex] = (ushort)i + 1 | TypeOffset << 16;
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

    private static unsafe bool Utf16StringEquals(ushort* aPtr, ushort* bPtr)
    {
        if (aPtr == bPtr)
            return true;
        if (aPtr == null || bPtr == null)
            return false;

        while (*aPtr++ != 0)
        {
            if (*bPtr++ == 0)
                return false;
            if (*aPtr != *bPtr)
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

        var funcPtr = StrategyBasedComWrappers.DefaultIUnknownInterfaceDetailsStrategy.GetIUnknownDerivedDetails(typeHandle)!
                .ManagedVirtualMethodTable[index];

        var hr = DispatchMember(pvInstance, funcPtr, entriesPtr->Parameters, entriesPtr->ParameterCount, ref pDispParams, pVarResult);

        if (hr < 0 && ComMarshalSupport.LastException.Value is { } lastException)
        {
            ComMarshalSupport.LastException.Value = null;
            
            ref var exceptionInfo = ref *(EXCEPINFO*)pExcepInfo;
            exceptionInfo.bstrSource = Marshal.StringToBSTR(lastException.Source);
            exceptionInfo.bstrDescription = Marshal.StringToBSTR(lastException.Message);
            exceptionInfo.bstrHelpFile = Marshal.StringToBSTR(lastException.HelpLink);
            exceptionInfo.scode = lastException.HResult;
        }

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

    public nint CreateInstance(nint pUnkOuter, in Guid riid)
    {
        throw new NotImplementedException();
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
    
    [LibraryImport("Hybrid.WebView2.Native")]
    private static unsafe partial int DispatchMember(nint instance, void* member, DispatchParameterDetails* details, int cDetails, ref DISPPARAMS dispParams, nint result);
}