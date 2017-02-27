namespace Tutorial.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    internal abstract class Sequence
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public abstract Iterator GetEnumerator(); // Must be public.
    }

    internal abstract class Iterator
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public abstract bool MoveNext(); // Must be public.

        public abstract object Current { get; } // Must be public.
    }

    internal abstract class GenericSequence<T>
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public abstract GenericIterator<T> GetEnumerator(); // Must be public.
    }

    internal abstract class GenericIterator<T>
    {
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public abstract bool MoveNext(); // Must be public.

        public abstract T Current { get; } // Must be public.
    }

    internal static partial class IteratorPattern
    {
        internal static void ForEach<T>(Sequence sequence, Action<T> processNext)
        {
            foreach (T value in sequence)
            {
                processNext(value);
            }
        }

        internal static void ForEach<T>(GenericSequence<T> sequence, Action<T> processNext)
        {
            foreach (T value in sequence)
            {
                processNext(value);
            }
        }
    }

    internal static partial class IteratorPattern
    {
        internal static void CompiledForEach<T>(Sequence sequence, Action<T> processNext)
        {
            Iterator iterator = sequence.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                {
                    T value = (T)iterator.Current;
                    processNext(value);
                }
            }
            finally
            {
                (iterator as IDisposable)?.Dispose();
            }
        }

        internal static void CompiledForEach<T>(GenericSequence<T> sequence, Action<T> processNext)
        {
            GenericIterator<T> iterator = sequence.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                {
                    T value = iterator.Current;
                    processNext(value);
                }
            }
            finally
            {
                (iterator as IDisposable)?.Dispose();
            }
        }
    }

    internal class SinglyLinkedListNode<T>
    {
        internal SinglyLinkedListNode(T value, SinglyLinkedListNode<T> next = null)
        {
            this.Value = value;
            this.Next = next;
        }

        public T Value { get; }

        public SinglyLinkedListNode<T> Next { get; }
    }

    internal class LinkedListSequence<T> : GenericSequence<T>
    {
        private readonly SinglyLinkedListNode<T> head;

        internal LinkedListSequence(SinglyLinkedListNode<T> head)
        {
            this.head = head;
        }

        public override GenericIterator<T> GetEnumerator() => new LinkedListIterator<T>(this.head);
    }

    internal class LinkedListIterator<T> : GenericIterator<T>
    {
        private SinglyLinkedListNode<T> node; // State.

        internal LinkedListIterator(SinglyLinkedListNode<T> head)
        {
            this.node = new SinglyLinkedListNode<T>(default(T), head);
        }

        public override bool MoveNext()
        {
            if (this.node.Next != null)
            {
                this.node = this.node.Next; // State change.
                return true;
            }
            return false;
        }

        public override T Current => this.node.Value;
    }

    internal static partial class IteratorPattern
    {
        internal static void ForEach(SinglyLinkedListNode<int> head)
        {
            LinkedListSequence<int> sequence = new LinkedListSequence<int>(head);
            foreach (int value in sequence)
            {
                value.WriteLine();
            }
        }

        internal static void ForEach<T>(T[] array, Action<T> processNext)
        {
            foreach (T value in array)
            {
                processNext(value);
            }
        }

        internal static void CompiledForEach<T>(T[] array, Action<T> processNext)
        {
            for (int index = 0; index < array.Length; index++)
            {
                T value = array[index];
                processNext(value);
            }
        }

        internal static void ForEach(string @string, Action<char> processNext)
        {
            foreach (char value in @string)
            {
                processNext(value);
            }
        }

        internal static void CompiledForEach(string @string, Action<char> processNext)
        {
            for (int index = 0; index < @string.Length; index++)
            {
                char value = @string[index];
                processNext(value);
            }
        }
    }

    internal static partial class IteratorPattern
    {
        internal static void Iterate<T>
            (GenericSequence<T> sequence, Action<T> processNext) => Iterate(sequence.GetEnumerator(), processNext);

        private static void Iterate<T>(GenericIterator<T> iterator, Action<T> processNext)
        {
            if (iterator.MoveNext())
            {
                processNext(iterator.Current);
                Iterate(iterator, processNext); // Recursion.
            }
        }
    }

    internal static partial class IteratorPattern
    {
        internal static void NonGenericSequences()
        {
            Type nonGenericEnumerable = typeof(IEnumerable);
            Type genericEnumerable = typeof(IEnumerable<>);
            IEnumerable<Type> nonGenericSequences = typeof(object).GetTypeInfo().Assembly // Core library.
                .GetExportedTypes()
                .Where(type =>
                {
                    if (type == nonGenericEnumerable || type == genericEnumerable)
                    {
                        return false;
                    }
                    Type[] interfaces = type.GetInterfaces();
                    return interfaces.Any(@interface => @interface == nonGenericEnumerable)
                        && !interfaces.Any(@interface =>
                            @interface.GetTypeInfo().IsGenericType
                            && @interface.GetGenericTypeDefinition() == genericEnumerable);
                })
                .OrderBy(type => type.FullName); // Define query.
            foreach (Type nonGenericSequence in nonGenericSequences) // Execute query.
            {
                nonGenericSequence.FullName.WriteLine();
            }
#if NETFX
            // System.Array
            // System.Collections.ArrayList
            // System.Collections.BitArray
            // System.Collections.CollectionBase
            // System.Collections.DictionaryBase
            // System.Collections.Hashtable
            // System.Collections.ICollection
            // System.Collections.IDictionary
            // System.Collections.IList
            // System.Collections.Queue
            // System.Collections.ReadOnlyCollectionBase
            // System.Collections.SortedList
            // System.Collections.Stack
            // System.Resources.IResourceReader
            // System.Resources.ResourceReader
            // System.Resources.ResourceSet
            // System.Runtime.Remoting.Channels.BaseChannelObjectWithProperties
            // System.Runtime.Remoting.Channels.BaseChannelSinkWithProperties
            // System.Runtime.Remoting.Channels.BaseChannelWithProperties
            // System.Security.AccessControl.AuthorizationRuleCollection
            // System.Security.AccessControl.CommonAcl
            // System.Security.AccessControl.DiscretionaryAcl
            // System.Security.AccessControl.GenericAcl
            // System.Security.AccessControl.RawAcl
            // System.Security.AccessControl.SystemAcl
            // System.Security.NamedPermissionSet
            // System.Security.Permissions.KeyContainerPermissionAccessEntryCollection
            // System.Security.PermissionSet
            // System.Security.Policy.ApplicationTrustCollection
            // System.Security.Policy.Evidence
            // System.Security.ReadOnlyPermissionSet
#else
            // System.Array
            // System.Collections.BitArray
            // System.Collections.CollectionBase
            // System.Collections.ICollection
            // System.Collections.IDictionary
            // System.Collections.IList
            // System.Resources.IResourceReader
            // System.Resources.ResourceSet
#endif
        }
    }
}

#if DEMO
namespace System.Collections
{
    public interface IEnumerable // Sequence.
    {
        IEnumerator GetEnumerator();
    }

    public interface IEnumerator // Iterator.
    {
        object Current { get; }

        bool MoveNext();

        void Reset(); // For COM interoperability.
    }
}

namespace System
{
    public interface IDisposable
    {
        void Dispose();
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerable<T> : IEnumerable // Sequence.
    {
        IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<T> : IDisposable, IEnumerator // Iterator.
    {
        T Current { get; }
    }
}

namespace System.Collections.Generic
{
    public interface IEnumerable<out T> : IEnumerable // Sequence.
    {
        IEnumerator<T> GetEnumerator();
    }

    public interface IEnumerator<out T> : IDisposable, IEnumerator // Iterator.
    {
        T Current { get; }
    }
}
#endif