namespace Dixin.Common
{
    using System;
    using System.Diagnostics.Contracts;

    public static class Argument
    {
        [ContractArgumentValidator]
        internal static void IsNotNull<T>([ValidatedNotNull] this T value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            Contract.EndContractBlock();
        }
    }
}
