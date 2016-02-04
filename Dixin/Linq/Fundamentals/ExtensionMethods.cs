namespace Dixin.Linq.Fundamentals
{
    internal partial class Methods
    {
        internal static bool Same(Methods @this, Methods other)
        {
            Methods arg0 = @this;
            Methods arg1 = other;
            return arg0 == arg1;
        }

        internal bool SameTo(Methods other)
        {
            Methods arg0 = this;
            Methods arg1 = other;
            return arg0 == arg1;
        }
    }
}
