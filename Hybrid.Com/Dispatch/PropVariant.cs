using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Hybrid.Com.Dispatch;

[StructLayout(LayoutKind.Explicit)]
public struct PropVariant
{
    [FieldOffset(0)] private ushort _vt;

    /// <summary>
    /// FILETIME variant value.
    /// </summary>
    [FieldOffset(8)] private readonly FILETIME _fileTime;

    /// <summary>
    /// The PropArray instance to fix the variant size on x64 bit systems.
    /// </summary>
    [FieldOffset(8)] private readonly PropArray _propArray;

    [FieldOffset(8)] public IntPtr Value;
    [FieldOffset(8)] public uint UInt32Value;
    [FieldOffset(8)] public int Int32Value;
    [FieldOffset(8)] public long Int64Value;
    [FieldOffset(8)] public ulong UInt64Value;

    /// <summary>
    /// Gets or sets variant type.
    /// </summary>
    public VarEnum VarType
    {
        get => (VarEnum)_vt;
        set => _vt = (ushort)value;
    }

    /// <summary>
    /// Gets the object for this PropVariant.
    /// </summary>
    /// <returns></returns>
    public object? Object
    {
        get => this.AsObject();
        set => this.SetObject(value);
    }

    /// <summary>
    /// Determines whether the specified System.Object is equal to the current PropVariant.
    /// </summary>
    /// <param name="obj">The System.Object to compare with the current PropVariant.</param>
    /// <returns>true if the specified System.Object is equal to the current PropVariant; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return obj is PropVariant variant && Equals(variant);
    }

    /// <summary>
    /// Determines whether the specified PropVariant is equal to the current PropVariant.
    /// </summary>
    /// <param name="afi">The PropVariant to compare with the current PropVariant.</param>
    /// <returns>true if the specified PropVariant is equal to the current PropVariant; otherwise, false.</returns>
    private bool Equals(PropVariant afi)
    {
        if (afi.VarType != VarType)
        {
            return false;
        }

        if (VarType != VarEnum.VT_BSTR)
        {
            return afi.Int64Value == Int64Value;
        }

        return afi.Value == Value;
    }

    /// <summary>
    ///  Serves as a hash function for a particular type.
    /// </summary>
    /// <returns> A hash code for the current PropVariant.</returns>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Returns a System.String that represents the current PropVariant.
    /// </summary>
    /// <returns>A System.String that represents the current PropVariant.</returns>
    public override string ToString()
    {
        return "[" + Value + "] " + Int64Value.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Determines whether the specified PropVariant instances are considered equal.
    /// </summary>
    /// <param name="afi1">The first PropVariant to compare.</param>
    /// <param name="afi2">The second PropVariant to compare.</param>
    /// <returns>true if the specified PropVariant instances are considered equal; otherwise, false.</returns>
    public static bool operator ==(PropVariant afi1, PropVariant afi2)
    {
        return afi1.Equals(afi2);
    }

    /// <summary>
    /// Determines whether the specified PropVariant instances are not considered equal.
    /// </summary>
    /// <param name="afi1">The first PropVariant to compare.</param>
    /// <param name="afi2">The second PropVariant to compare.</param>
    /// <returns>true if the specified PropVariant instances are not considered equal; otherwise, false.</returns>
    public static bool operator !=(PropVariant afi1, PropVariant afi2)
    {
        return !afi1.Equals(afi2);
    }
}