namespace Dixin.Linq.Fundamentals
{
    using System;

    internal static partial class HigherOrderFunction
    {
        internal class DataType { }

        internal static DataType FirstOrder(DataType dataValue)
        {
            return dataValue;
        }

        internal static void CallFirstOrder()
        {
            DataType inputValue = default(DataType);
            DataType outputValue = FirstOrder(inputValue);
        }
    }

    internal static partial class HigherOrderFunction
    {
        internal delegate void FunctionType();

        internal static FunctionType HigherOrder(FunctionType functionValue)
        {
            return functionValue;
        }

        internal static void CallHigherOrder()
        {
            FunctionType inputValue = default(FunctionType);
            FunctionType outputValue = HigherOrder(inputValue);
        }
    }

    internal static partial class HigherOrderFunction
    {
        internal static void Lambda()
        {
            Action firstOrder1 = () => { };
            Action<Action> higherOrder1 = action => action();

            Func<int> firstOrder2 = () => default(int);
            Func<Func<int>> higherOrder2 = () => firstOrder2;
        }
    }

    internal static partial class FirstClass
    {
        internal class ObjectType
        {
            internal ObjectType InnerObject { get; set; }
        }

        internal delegate void FunctionType();

        internal static void ObjectInstance()
        {
            ObjectType objectValue = new ObjectType();
        }

        internal static void FunctionInstance()
        {
            FunctionType functionValue1 = FunctionInstance; // Named function.
            FunctionType functionValue2 = () => { }; // Anonymous function.
        }
    }

    internal static partial class FirstClass
    {
        internal static ObjectType objectField = new ObjectType();

        internal static FunctionType functionField1 = FunctionInstance; // Named function.

        internal static FunctionType functionField2 = () => { }; // Anonymous function.
    }

    internal static partial class FirstClass
    {
        internal static ObjectType InputOutputObject(ObjectType objectValue) => objectValue;

        internal static FunctionType InputOutputFunction(FunctionType functionValue) => functionValue;
    }

    internal static partial class FirstClass
    {
        internal static void NestedObject()
        {
            ObjectType outerObject = new ObjectType()
            {
                InnerObject = new ObjectType()
            };
        }

        internal static void NestedFunction()
        {
            object nonLocalVariable = new object();
            FunctionType outerFunction = () =>
                {
                    object outerLocalVariable = nonLocalVariable;
                    FunctionType innerFunction = () =>
                        {
                            object innerLocalVariable = nonLocalVariable;
                        };
                };
        }
    }

    internal static partial class FirstClass
    {
        internal static void ObjectEquality()
        {
            ObjectType objectValue1;
            ObjectType objectValue2;
            objectValue1 = objectValue2 = new ObjectType();
            bool areEqual1 = objectValue1 == objectValue2; // true.

            ObjectType objectValue3 = null;
            bool areEqual2 = objectValue2 == objectValue3; // false.
        }

        internal static void FunctionEquality()
        {
            FunctionType functionValue1;
            FunctionType functionValue2;
            functionValue1 = functionValue2 = () => { };
            bool areEqual1 = functionValue1 == functionValue2; // true.

            FunctionType functionValue3 = null;
            bool areEqual2 = functionValue2 == functionValue3; // false.
        }
    }
}

#if ERROR
namespace System.Collections.Generic
{
    public class List<T>
    {
        public void Sort(Comparison<T> comparison);
    }
}

namespace System
{
    public delegate int Comparison<in T>(T x, T y);
}
#endif