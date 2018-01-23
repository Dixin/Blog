namespace Tutorial.CategoryTheory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.FSharp.Core;

    public interface IMonoid<T>
    {
        T Multiply(T value1, T value2);

        T Unit();
    }

    public class Int32SumMonoid : IMonoid<int>
    {
        public int Multiply(int value1, int value2) => value1 + value2;

        public int Unit() => 0;
    }

    public class Int32ProductMonoid : IMonoid<int>
    {
        public int Multiply(int value1, int value2) => value1 * value2;

        public int Unit() => 1;
    }

    public class ClockMonoid : IMonoid<uint>
    {
        public uint Multiply(uint value1, uint value2)
        {
            uint result = (value1 + value2) % this.Unit();
            return result != 0 ? result : this.Unit();
        }

        public uint Unit() => 12U;
    }

    public class StringConcatMonoid : IMonoid<string>
    {
        public string Multiply(string value1, string value2) => string.Concat(value1, value2);

        public string Unit() => string.Empty;
    }

    public class EnumerableConcatMonoid<T> : IMonoid<IEnumerable<T>>
    {
        public IEnumerable<T> Multiply(IEnumerable<T> value1, IEnumerable<T> value2) => value1.Concat(value2);

        public IEnumerable<T> Unit() => Enumerable.Empty<T>();
    }

    public class BooleanAndMonoid : IMonoid<bool>
    {
        public bool Multiply(bool value1, bool value2) => value1 && value2;

        public bool Unit() => true;
    }

    public class BooleanOrMonoid : IMonoid<bool>
    {
        public bool Multiply(bool value1, bool value2) => value1 || value2;

        public bool Unit() => false;
    }

#if DEMO
    public class VoidMonoid : IMonoid<void>
    {
        public void Multiply(void value1, void value2) => default;

        public void Unit() => default;
    }
#endif

    public class UnitMonoid : IMonoid<Unit>
    {
        public Unit Multiply(Unit value1, Unit value2) => null;

        public Unit Unit() => null;
    }

    public class MonoidCategory<T> : ICategory<Type, T>
    {
        private readonly IMonoid<T> monoid;

        public MonoidCategory(IMonoid<T> monoid)
        {
            this.monoid = monoid;
        }

        public IEnumerable<Type> Objects { get { yield return typeof(T); } }

        public T Compose(T morphism2, T morphism1) => this.monoid.Multiply(morphism1, morphism2);

        public T Id(Type @object) => this.monoid.Unit();
    }
}

#if DEMO
namespace System
{
    using System.Runtime.InteropServices;

	[ComVisible(true)]
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct Void
	{
	}
}

namespace Microsoft.FSharp.Core
{
    using System;

    [CompilationMapping(SourceConstructFlags.ObjectType)]
    [Serializable]
    public sealed class Unit : IComparable
    {
        internal Unit() { }

        public override int GetHashCode() => 0;

        public override bool Equals(object obj) => 
            obj == null || LanguagePrimitives.IntrinsicFunctions.TypeTestGeneric<Unit>(obj);

        int IComparable.CompareTo(object obj) => 0;
    }
}
#endif
