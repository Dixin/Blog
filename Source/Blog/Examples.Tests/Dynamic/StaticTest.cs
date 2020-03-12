// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StaticTest.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright © Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the StaticTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Examples.Tests.Dynamic
{
    internal class StaticTest
    {
#pragma warning disable 649
        private static int value;
#pragma warning restore 649

        internal static int Value => value;

        internal static int Method() => 2;
    }
}