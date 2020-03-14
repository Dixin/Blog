// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructTest.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright ?Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the StructTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Examples.Tests.Dynamic
{
    internal struct StructTest
    {
        private int value;

        internal StructTest(int value)
        {
            this.value = value;
        }

        internal int Value
        {
            get => this.value;

            set => this.value = value;
        }
    }
}