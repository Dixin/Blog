namespace Dixin.Linq.Fundamentals
{
    using System;

    public static partial class HigherOrderFunction
    {
        public class DataType { }

        public static DataType FirstOrder(DataType dataValue)
        {
            return dataValue;
        }

        public static void CallFirstOrder()
        {
            DataType inputValue = default(DataType);
            DataType outputValue = FirstOrder(inputValue);
        }
    }

    public static partial class HigherOrderFunction
    {
        public delegate void FunctionType();

        public static FunctionType HigherOrder(FunctionType functionValue)
        {
            return functionValue;
        }

        public static void CallHigherOrder()
        {
            FunctionType inputValue = default(FunctionType);
            FunctionType outputValue = HigherOrder(inputValue);
        }
    }

    public static partial class HigherOrderFunction
    {
        public static void Lambda()
        {
            Action firstOrder1 = () => { };
            Action<Action> higherOrder1 = action => action();

            Func<int> firstOrder2 = () => default(int);
            Func<Func<int>> higherOrder2 = () => firstOrder2;
        }
    }

    public static partial class FirstClass
    {
        public class ObjectType
        {
            public ObjectType InnerObject { get; set; }
        }

        public delegate void FunctionType();

        public static void ObjectInstance()
        {
            ObjectType objectValue = new ObjectType();
        }

        public static void FunctionInstance()
        {
            FunctionType functionValue1 = FunctionInstance; // Named function.
            FunctionType functionValue2 = () => { }; // Anonymous function.
        }
    }

    public static partial class FirstClass
    {
        public static ObjectType objectField = new ObjectType();

        public static FunctionType functionField1 = FunctionInstance; // Named function.

        public static FunctionType functionField2 = () => { }; // Anonymous function.
    }

    public static partial class FirstClass
    {
        public static ObjectType InputOutputObject(ObjectType objectValue) => objectValue;

        public static FunctionType InputOutputFunction(FunctionType functionValue) => functionValue;
    }

    public static partial class FirstClass
    {
        public static void NestedObject()
        {
            ObjectType outerObject = new ObjectType()
            {
                InnerObject = new ObjectType()
            };
        }

        public static void NestedFunction()
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

    public static partial class FirstClass
    {
        public static void ObjectEquality()
        {
            ObjectType objectValue1;
            ObjectType objectValue2;
            objectValue1 = objectValue2 = new ObjectType();
            bool areEqual1 = objectValue1 == objectValue2; // true.

            ObjectType objectValue3 = null;
            bool areEqual2 = objectValue2 == objectValue3; // false.
        }

        public static void FunctionEquality()
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