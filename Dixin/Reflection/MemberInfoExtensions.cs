namespace Dixin.Reflection
{
    using System;
    using System.Reflection;

    using Dixin.Common;

    public static class MemberInfoExtensions
    {
        public static bool IsObsolete(this MemberInfo member)
        {
            member.NotNull(nameof(member));

            return Attribute.IsDefined(member, typeof(ObsoleteAttribute), false);
        }
    }
}