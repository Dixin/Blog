// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITest.cs" company="WebOS - http://www.coolwebos.com">
//   Copyright ?Dixin 2010 http://weblogs.asp.net/dixin
// </copyright>
// <summary>
//   Defines the ITest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Examples.Tests.Dynamic
{
    public interface ITest
    {
        #region Properties

        int Property
        {
            get;
            set;
        }

        #endregion

        #region Indexers

        string this[int x, int y]
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        int Method(int value);

        #endregion
    }
}