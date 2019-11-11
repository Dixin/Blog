// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DynamicWrapperExtensions.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright © Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the DynamicWrapperExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Dixin.Dynamic
{
    public static class DynamicWrapperExtensions
    {
        #region Public Methods

        public static dynamic ToDynamic<T>(this T value) // where T : class
            => new DynamicWrapper<T>(ref value);

        #endregion
    }
}