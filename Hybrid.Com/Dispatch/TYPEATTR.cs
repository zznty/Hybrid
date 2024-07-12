using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Hybrid.Com.Dispatch;

[EditorBrowsable(EditorBrowsableState.Never)]
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TYPEATTR
{
    // Constant used with the memid fields.
    public const int MemberIdNil = unchecked((int)0xFFFFFFFF);

    // Actual fields of the TypeAttr struct.
    public Guid guid;
    public int lcid;
    public int dwReserved;
    public int memidConstructor;
    public int memidDestructor;
    public IntPtr lpstrSchema;
    public int cbSizeInstance;
    public TYPEKIND typekind;
    public short cFuncs;
    public short cVars;
    public short cImplTypes;
    public short cbSizeVft;
    public short cbAlignment;
    public TYPEFLAGS wTypeFlags;
    public short wMajorVerNum;
    public short wMinorVerNum;
    public TYPEDESC tdescAlias;
    public IDLDESC idldescType;
}