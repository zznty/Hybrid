using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Hybrid.Com.DynCall;

public class DynamicCallVm : SafeHandle
{
    public DynamicCallVm(nint stackSize = 4096) : base(0, true)
    {
        SetHandle(PInvoke.NewVm(stackSize));
    }

    protected override bool ReleaseHandle()
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.FreeVm(handle);
        return true;
    }
    
    public DynamicCallMode Mode
    {
        set
        {
            ObjectDisposedException.ThrowIf(IsInvalid, this);
            
            PInvoke.Mode(handle, value);
        }
    }

    public override bool IsInvalid => handle == 0;

    public void Reset()
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ResetVm(handle);
    }

    public void PushArg(bool value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgBool(handle, value);
    }

    public void PushArg(byte value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgByte(handle, value);
    }
    
    public void PushArg(short value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgShort(handle, value);
    }
    
    public void PushArg(int value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgInt(handle, value);
    }
    
    public void PushArg(long value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgLong(handle, value);
    }
    
    public void PushArg(float value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgFloat(handle, value);
    }
    
    public void PushArg(double value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgDouble(handle, value);
    }
    
    public void PushArg(nint value)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.ArgPointer(handle, value);
    }

    public void Call(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        PInvoke.CallVoid(handle, funcPtr);
    }

    public bool CallBool(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallBool(handle, funcPtr);
    }
    
    public byte CallByte(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallByte(handle, funcPtr);
    }
    
    public short CallShort(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallShort(handle, funcPtr);
    }
    
    public int CallInt(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallInt(handle, funcPtr);
    }
    
    public long CallLong(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallLong(handle, funcPtr);
    }
    
    public float CallFloat(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallFloat(handle, funcPtr);
    }
    
    public double CallDouble(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallDouble(handle, funcPtr);
    }
    
    public nint CallPointer(nint funcPtr)
    {
        ObjectDisposedException.ThrowIf(IsInvalid, this);
        return PInvoke.CallPointer(handle, funcPtr);
    }
    
    // todo support varargs
}

file static class PInvoke
{
    private const string DllName = "DynCall";
    
    [DllImport(DllName, EntryPoint = "dcNewCallVM")]
    public static extern nint NewVm(nint size);
    
    [DllImport(DllName, EntryPoint = "dcFree")]
    public static extern void FreeVm(nint vm);
    
    [DllImport(DllName, EntryPoint = "dcReset")]
    public static extern void ResetVm(nint vm);
    
    [DllImport(DllName, EntryPoint = "dcMode")]
    public static extern void Mode(nint vm, DynamicCallMode mode);

    [DllImport(DllName, EntryPoint = "dcBeginCallAggr")]
    public static extern void BeginCallAggr(nint vm, nint ag);
    
    [DllImport(DllName, EntryPoint = "dcArgBool")]
    public static extern void ArgBool(nint vm, [MarshalAs(UnmanagedType.Bool)] bool value);

    [DllImport(DllName, EntryPoint = "dcArgChar")]
    public static extern void ArgByte(nint vm, byte value);
    
    [DllImport(DllName, EntryPoint = "dcArgShort")]
    public static extern void ArgShort(nint vm, short value);
    
    [DllImport(DllName, EntryPoint = "dcArgInt")]
    public static extern void ArgInt(nint vm, int value);
    
    [DllImport(DllName, EntryPoint = "dcArgLong")]
    public static extern void ArgLong(nint vm, long value);
    
    [DllImport(DllName, EntryPoint = "dcArgFloat")]
    public static extern void ArgFloat(nint vm, float value);
    
    [DllImport(DllName, EntryPoint = "dcArgDouble")]
    public static extern void ArgDouble(nint vm, double value);
    
    [DllImport(DllName, EntryPoint = "dcArgPointer")]
    public static extern void ArgPointer(nint vm, nint value);
    
    [DllImport(DllName, EntryPoint = "dcArgAggr")]
    public static extern void ArgAggr(nint vm, nint ag, nint value);
    
    [DllImport(DllName, EntryPoint = "dcCallVoid")]
    public static extern void CallVoid(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallBool")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CallBool(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallChar")]
    public static extern byte CallByte(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallShort")]
    public static extern short CallShort(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallInt")]
    public static extern int CallInt(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallLong")]
    public static extern long CallLong(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallFloat")]
    public static extern float CallFloat(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallDouble")]
    public static extern double CallDouble(nint vm, nint funcPtr);
    
    [DllImport(DllName, EntryPoint = "dcCallPointer")]
    public static extern nint CallPointer(nint vm, nint funcPtr);
    
    /// <remarks>
    /// retval is written to *ret, returns ret
    /// </remarks>
    [DllImport(DllName, EntryPoint = "dcCallAggr")]
    public static extern nint CallAggr(nint vm, nint funcPtr, nint ag, out nint ret);
    
    [DllImport(DllName, EntryPoint = "dcGetError")]
    public static extern int GetError(nint vm);

    [DllImport(DllName, EntryPoint = "dcNewAggr")]
    public static extern nint NewAggr(nint maxFieldCount, nint size);
    
    [DllImport(DllName, EntryPoint = "dcFreeAggr")]
    public static extern void FreeAggr(nint ag);

    /// <remarks>
    /// if type == DC_SIGCHAR_AGGREGATE, pass DCaggr* of nested struct/union in __arglist
    /// </remarks>
    [DllImport(DllName, EntryPoint = "dcAggrField")]
    public static extern void AggrField(nint ag, sbyte type, int offset, nint arrayLen, __arglist);

    /// <summary>
    /// to indicate end of struct definition, required
    /// </summary>
    [DllImport(DllName, EntryPoint = "dcCloseAggr")]
    public static extern void CloseAggr(nint ag);
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum DynamicCallMode
{
    // DEFAULT
    
    /// <summary>
    /// C default (platform native)
    /// </summary>
    C_DEFAULT = 0,
    /// <summary>
    /// for C++ calls where the first param is hidden this ptr (platform native)
    /// </summary>
    C_DEFAULT_THIS = 99,
    /// <summary>
    /// to be set for vararg calls' non-hidden (e.g. C++ this ptr), named arguments
    /// </summary>
    C_ELLIPSIS = 100,
    /// <summary>
    /// to be set for vararg calls' non-hidden (e.g. C++ this ptr), variable arguments (in ... part)
    /// </summary>
    C_ELLIPSIS_VARARGS = 101,
    
    // PLATFORM SPECIFIC
    
    C_X86_CDECL = 1,
    C_X86_WIN32_STD = 2,
    C_X86_WIN32_FAST_MS = 3,
    C_X86_WIN32_FAST_GNU = 4,
    C_X86_WIN32_THIS_MS = 5,
    /// <summary>
    /// alias - identical to cdecl (w/ this-ptr as 1st arg)
    /// </summary>
    C_X86_WIN32_THIS_GNU = C_X86_CDECL,
    C_X64_WIN64 = 7,
    /// <summary>
    /// only needed when using aggregate by value as a return type
    /// </summary>
    C_X64_WIN64_THIS = 70,
    C_X64_SYSV = 8,
    C_X64_SYSV_THIS = C_X64_SYSV,
    C_PPC32_DARWIN = 9,
    C_PPC32_OSX = C_PPC32_DARWIN,
    C_ARM_ARM_EABI = 10,
    C_ARM_THUMB_EABI = 11,
    C_ARM_ARMHF = 30,
    C_MIPS32_EABI = 12,
    /// <summary>
    /// alias - deprecated.
    /// </summary>
    [Obsolete("Use C_MIPS32_EABI instead.")]
    C_MIPS32_PSPSDK = C_MIPS32_EABI,
    C_PPC32_SYSV = 13,
    /// <summary>
    /// alias
    /// </summary>
    C_PPC32_LINUX = C_PPC32_SYSV,
    C_ARM_ARM = 14,
    C_ARM_THUMB = 15,
    C_MIPS32_O32 = 16,
    C_MIPS64_N32 = 17,
    C_MIPS64_N64 = 18,
    C_X86_PLAN9 = 19,
    C_SPARC32 = 20,
    C_SPARC64 = 21,
    C_ARM64 = 22,
    C_PPC64 = 23,
    /// <summary>
    /// alias
    /// </summary>
    C_PPC64_LINUX = C_PPC64,
    
    // syscalls, default
    
    SYS_DEFAULT = 200,
    
    // syscalls, platform specific
    
    SYS_X86_INT80H_LINUX = 201,
    SYS_X86_INT80H_BSD = 202,
    SYS_X64_SYSCALL_SYSV = 204,
    SYS_PPC32 = 210,
    SYS_PPC64 = 211,
}