namespace Hybrid.Com.SourceGenerator;

public static class HashCodeExtensions
{
    public static void CombineHashCode(this ref int hashCode, int value) => 
        hashCode = unchecked(hashCode * 314159 + value);
}