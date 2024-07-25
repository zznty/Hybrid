#include <windows.h>
#include <oleauto.h>
#include <vector>
#include "../Libs/DynCall/dyncall/dyncall.h"

// Enum for DispatchParameterFlags
enum DispatchParameterFlags : BYTE
{
    None = 0,
    In = 1,
    Out = 2
};

// Struct for DispatchParameterDetails
struct DispatchParameterDetails
{
    VARTYPE type;
    DispatchParameterFlags flags;
};

void UnwrapVariant(VARIANT* var)
{
    if (var->vt == VT_VARIANT)
    {
        const auto innerVar = var->pvarVal;

        var->vt = innerVar->vt;
        var->byref = innerVar->byref;
        
        CoTaskMemFree(innerVar);
    }
}

HRESULT CallHResultWithReturnValue(DCCallVM* vm, const PVOID member, VARIANT* result)
{
    void* resultPtr;
    dcArgPointer(vm, &resultPtr);

    const auto hResult = dcCallInt(vm, member);

    result->byref = resultPtr;
    
    return hResult;
}

// Function to dispatch the member
extern "C" {    
    __declspec(dllexport) HRESULT DispatchMember(const PVOID instance, const PVOID member, const DispatchParameterDetails* details, const int cDetails, const DISPPARAMS* params, VARIANT* result)
    {
        if (instance == nullptr || member == nullptr || details == nullptr || params == nullptr || result == nullptr)
            return E_INVALIDARG;
        
        const auto vm = dcNewCallVM(4096);

        HRESULT hr = S_OK;
        __try
        {
            dcMode(vm, DC_CALL_C_X64_WIN64);
            dcReset(vm);

            dcArgPointer(vm, instance);
            for (int i = params->cArgs - 1; i >= 0; --i)
            {
                const auto paramVt = details[params->cArgs - i].type;
                const auto arg = &params->rgvarg[i];
                if (arg->vt != paramVt && paramVt == VT_VARIANT && arg->vt != VT_VARIANT) // wrap value into variant if func expects variant parameter
                {
                    const auto var = static_cast<VARIANT*>(CoTaskMemAlloc(sizeof(VARIANT)));

                    var->vt = arg->vt;
                    var->byref = arg->byref;

                    arg->vt = VT_VARIANT;
                    arg->pvarVal = var;
                }
                else if (arg->vt != paramVt)
                    return DISP_E_BADVARTYPE;
                
                switch (arg->vt)
                {
                    case VT_VOID:
                    case VT_PTR:
                        dcArgPointer(vm, arg->byref);
                        break;
                    case VT_I1:
                    case VT_UI1:
                        dcArgChar(vm, arg->cVal);
                        break;
                    case VT_I2:
                    case VT_UI2:
                        dcArgShort(vm, arg->iVal);
                        break;
                    case VT_I4:
                    case VT_UI4:
                    case VT_HRESULT:
                        dcArgInt(vm, arg->intVal);
                        break;
                    case VT_I8:
                    case VT_UI8:
                        dcArgLongLong(vm, arg->llVal);
                        break;
                    case VT_R4:
                        dcArgFloat(vm, arg->fltVal);
                        break;
                    case VT_R8:
                        dcArgDouble(vm, arg->dblVal);
                        break;
                    case VT_BOOL:
                        dcArgBool(vm, arg->boolVal);
                        break;
                    default:
                        dcArgPointer(vm, arg->byref);
                        break;
                }
            }

            VariantInit(result);

            if (cDetails - 1 > params->cArgs && details[0].type == VT_HRESULT && details[cDetails - 1].flags & Out)
            {
                hr = CallHResultWithReturnValue(vm, member, result);
                if (SUCCEEDED(hr))
                    result->vt = details[cDetails - 1].type;
                return hr;
            }
            
            result->vt = details[0].type;

            switch (result->vt)
            {
                case VT_HRESULT:
                case VT_I4:
                case VT_UI4:
                    result->intVal = dcCallInt(vm, member);
                    break;
                case VT_I8:
                case VT_UI8:
                    result->llVal = dcCallLongLong(vm, member);
                    break;
                case VT_R4:
                    result->fltVal = dcCallFloat(vm, member);
                    break;
                case VT_R8:
                    result->dblVal = dcCallDouble(vm, member);
                    break;
                case VT_BOOL:
                    result->intVal = dcCallBool(vm, member);
                    break;
                case VT_BSTR:
                case VT_PTR:
                case VT_UNKNOWN:
                case VT_DISPATCH:
                case VT_VARIANT:
                case VT_SAFEARRAY:
                case VT_CARRAY:
                case VT_USERDEFINED:
                case VT_BYREF:
                case VT_RECORD:
                case VT_VOID:
                case VT_ERROR:
                    result->byref = dcCallPointer(vm, member);
                    break;
                default:
                    dcCallVoid(vm, member);
                    break;
            }
        }
        __finally
        {
            dcFree(vm);

            for (size_t i = 0; i < params->cArgs; ++i)
            {
                if (params->rgvarg[i].vt == VT_VARIANT)
                {
                    const auto var = static_cast<VARIANT*>(params->rgvarg[i].byref);
                    if (var != nullptr && SUCCEEDED(VariantClear(var)))
                        CoTaskMemFree(var);
                }
            }

            UnwrapVariant(result);
        }

        return hr;
    }
}

