namespace Dixin.Linq.Fundamentals
{
    public partial class Methods
    {
        public static bool Same(Methods @this, Methods other)
        {
            Methods arg0 = @this;
            Methods arg1 = other;
            return arg0 == arg1;
        }

        public bool SameTo(Methods other)
        {
            Methods arg0 = this;
            Methods arg1 = other;
            return arg0 == arg1;
        }
    }
}
