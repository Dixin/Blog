// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldInfoExtensions.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright © Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the FieldInfoExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dixin.Reflection
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    public static class FieldInfoExtensions
    {
        #region Methods

        public static void SetValue<T>(this FieldInfo field, ref T obj, object value)
        {
            Contract.Requires<ArgumentNullException>(field != null);

            if (typeof(T).IsValueType)
            {
                field.SetValueDirect(__makeref(obj), value);
            }
            else
            {
                field.SetValue(obj, value);
            }
        }

        #endregion
    }
}