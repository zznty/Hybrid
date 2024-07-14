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

        __try
        {
            dcMode(vm, DC_CALL_C_X64_WIN64);
            dcReset(vm);

            dcArgPointer(vm, instance);
            for (int i = params->cArgs - 1; i >= 0; --i)
            {
                if (params->rgvarg[i].vt != details[params->cArgs - i].type)
                    return DISP_E_BADVARTYPE;
                
                switch (params->rgvarg[i].vt)
                {
                    case VT_VOID:
                    case VT_PTR:
                        dcArgPointer(vm, params->rgvarg[i].byref);
                        break;
                    case VT_I1:
                    case VT_UI1:
                        dcArgChar(vm, params->rgvarg[i].cVal);
                        break;
                    case VT_I2:
                    case VT_UI2:
                        dcArgShort(vm, params->rgvarg[i].iVal);
                        break;
                    case VT_I4:
                    case VT_UI4:
                    case VT_HRESULT:
                        dcArgInt(vm, params->rgvarg[i].intVal);
                        break;
                    case VT_I8:
                    case VT_UI8:
                        dcArgLongLong(vm, params->rgvarg[i].llVal);
                        break;
                    case VT_R4:
                        dcArgFloat(vm, params->rgvarg[i].fltVal);
                        break;
                    case VT_R8:
                        dcArgDouble(vm, params->rgvarg[i].dblVal);
                        break;
                    case VT_BOOL:
                        dcArgBool(vm, params->rgvarg[i].boolVal);
                        break;
                    default:
                        dcArgPointer(vm, params->rgvarg[i].byref);
                        break;
                }
            }

            VariantInit(result);

            if (cDetails - 1 > params->cArgs && details[0].type == VT_HRESULT && details[cDetails - 1].flags & Out)
            {
                result->vt = details[cDetails - 1].type;
                return CallHResultWithReturnValue(vm, member, result);
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
        }

        return S_OK;
    }
}

