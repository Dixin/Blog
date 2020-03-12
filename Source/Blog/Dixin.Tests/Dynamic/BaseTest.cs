// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseTest.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright © Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the BaseTest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Examples.Tests.Dynamic
{
    using System;
    using System.Globalization;

    internal class BaseTest : ITest
    {
        #region Constants and Fields

        private readonly int[,] array = new int[10, 10];

        #endregion

        #region Properties

        public int Property2
        {
            get;
            set;
        }

        int ITest.Property
        {
            get;
            set;
        }

        #endregion

        #region Indexers

        public string this[int x, int y]
        {
            get
            {
                return this.array[x, y].ToString(CultureInfo.InvariantCulture);
            }

            set
            {
                this.array[x, y] = Convert.ToInt32(value);
            }
        }

        #endregion

        #region Implemented Interfaces

        #region ITest

        public int Method(int value) => value * 2;

        #endregion

        #endregion

        #region Methods

        private int Method2(int value) => value / 2;

        #endregion
    }
}