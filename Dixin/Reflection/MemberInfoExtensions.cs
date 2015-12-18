namespace Dixin.Reflection
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    public static class MemberInfoExtensions
    {
        public static bool IsObsolete(this MemberInfo member)
        {
            Contract.Requires<ArgumentNullException>(member != null);

            return Attribute.IsDefined(member, typeof(ObsoleteAttribute), false);
        }
    }
}