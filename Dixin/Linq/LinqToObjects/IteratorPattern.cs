namespace Dixin.Linq.LinqToObjects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Dixin.Reflection;

    public class Sequence
    {
        public Iterator GetEnumerator() => new Iterator();
    }

    public class Iterator
    {
        public bool MoveNext() => false;

        public object Current { get; }
    }

    public class GenericSequence<T>
    {
        public GenericIterator<T> GetEnumerator() => new GenericIterator<T>();
    }

    public class GenericIterator<T>
    {
        public bool MoveNext() => false;

        public T Current { get; }
    }

    public static partial class IteratorPattern
    {
        public static void ForEach<T>(Sequence sequence, Action<T> processValue)
        {
            foreach (T value in sequence)
            {
                processValue(value);
            }
        }

        public static void ForEach<T>(GenericSequence<T> sequence, Action<T> processValue)
        {
            foreach (T value in sequence)
            {
                processValue(value);
            }
        }
    }

    public static partial class IteratorPattern
    {
        public static void CompiledForEach<T>(Sequence sequence, Action<T> next)
        {
            Iterator iterator = sequence.GetEnumerator();
            try
            {
                while (iterator.MoveNext())
                {
                    T value = (T)iterator.Current;
                    next(value);
                }
            }
            finally
            {
                (iterator as IDisposable)?.Dispose();
            }
        }

        public static void CompiledForEach<T>(GenericSequence<T> sequence, Action<T> next)
        {
            GenericIterator<T> genericIterator = sequence.GetEnumerator();
            try
            {
                while (genericIterator.MoveNext())
                {
                    T value = genericIterator.Current;
                    next(value);
                }
            }
            finally
            {
                (genericIterator as IDisposable)?.Dispose();
            }
        }

        public static void ForEach<T>(T[] array, Action<T> next)
        {
            foreach (T value in array)
            {
                next(value);
            }
        }

        public static void CompiledForEach<T>(T[] array, Action<T> next)
        {
            for (int index = 0; index < array.Length; index++)
            {
                T value = array[index];
                next(value);
            }
        }

        public static void ForEach(string @string, Action<char> next)
        {
            foreach (char value in @string)
            {
                next(value);
            }
        }

        public static void CompiledForEach(string @string, Action<char> next)
        {
            for (int index = 0; index < @string.Length; index++)
            {
                char value = @string[index];
                next(value);
            }
        }
    }

    public static partial class IteratorPattern
    {
        public static void Iterate<T>
            (GenericSequence<T> sequence, Action<T> next) => Iterate(sequence.GetEnumerator(), next);

        public static void Iterate<T>(GenericIterator<T> iterator, Action<T> next)
        {
            if (iterator.MoveNext())
            {
                next(iterator.Current);
                Iterate(iterator, next); // Recursion.
            }
        }
    }

    public static partial class IteratorPattern
    {
        public static IEnumerable<Type> NonGenericSequences(Assembly assembly)
        {
            Type nonGenericEnumerable = typeof(IEnumerable);
            Type genericEnumerable = typeof(IEnumerable<>);

            return assembly
                .ExportedTypes
                .Where(type => type != nonGenericEnumerable && nonGenericEnumerable.IsAssignableFrom(type))
                .Except(assembly
                .ExportedTypes
                    .Where(type => type.IsAssignableTo(genericEnumerable)))
                .OrderBy(type => type.FullName);
        }

        public static void NonGenericSequences()
        {
            foreach (Type nonGenericSequence in NonGenericSequences(typeof(object).Assembly)) // mscorlib.dll.
            {
                //Console.WriteLine(nonGenericSequence.FullName);
            }
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

            foreach (Type nonGenericSequence in NonGenericSequences(typeof(Uri).Assembly)) // System.dll.
            {
                Trace.WriteLine(nonGenericSequence.FullName);
            }
            // System.CodeDom.CodeAttributeArgumentCollection
            // System.CodeDom.CodeAttributeDeclarationCollection
            // System.CodeDom.CodeCatchClauseCollection
            // System.CodeDom.CodeCommentStatementCollection
            // System.CodeDom.CodeDirectiveCollection
            // System.CodeDom.CodeExpressionCollection
            // System.CodeDom.CodeNamespaceCollection
            // System.CodeDom.CodeNamespaceImportCollection
            // System.CodeDom.CodeParameterDeclarationExpressionCollection
            // System.CodeDom.CodeStatementCollection
            // System.CodeDom.CodeTypeDeclarationCollection
            // System.CodeDom.CodeTypeMemberCollection
            // System.CodeDom.CodeTypeParameterCollection
            // System.CodeDom.CodeTypeReferenceCollection
            // System.CodeDom.Compiler.CompilerErrorCollection
            // System.CodeDom.Compiler.TempFileCollection
            // System.Collections.Specialized.HybridDictionary
            // System.Collections.Specialized.IOrderedDictionary
            // System.Collections.Specialized.ListDictionary
            // System.Collections.Specialized.NameObjectCollectionBase
            // System.Collections.Specialized.NameObjectCollectionBase + KeysCollection
            // System.Collections.Specialized.NameValueCollection
            // System.Collections.Specialized.OrderedDictionary
            // System.Collections.Specialized.StringCollection
            // System.Collections.Specialized.StringDictionary
            // System.ComponentModel.AttributeCollection
            // System.ComponentModel.ComponentCollection
            // System.ComponentModel.Design.DesignerCollection
            // System.ComponentModel.Design.DesignerOptionService + DesignerOptionCollection
            // System.ComponentModel.Design.DesignerVerbCollection
            // System.ComponentModel.EventDescriptorCollection
            // System.ComponentModel.IBindingList
            // System.ComponentModel.IBindingListView
            // System.ComponentModel.ListSortDescriptionCollection
            // System.ComponentModel.PropertyDescriptorCollection
            // System.ComponentModel.TypeConverter + StandardValuesCollection
            // System.Configuration.ConfigXmlDocument
            // System.Configuration.SchemeSettingElementCollection
            // System.Configuration.SettingElementCollection
            // System.Configuration.SettingsAttributeDictionary
            // System.Configuration.SettingsContext
            // System.Configuration.SettingsPropertyCollection
            // System.Configuration.SettingsPropertyValueCollection
            // System.Configuration.SettingsProviderCollection
            // System.Diagnostics.CounterCreationDataCollection
            // System.Diagnostics.EventLogEntryCollection
            // System.Diagnostics.EventLogPermissionEntryCollection
            // System.Diagnostics.InstanceDataCollection
            // System.Diagnostics.InstanceDataCollectionCollection
            // System.Diagnostics.PerformanceCounterPermissionEntryCollection
            // System.Diagnostics.ProcessModuleCollection
            // System.Diagnostics.ProcessThreadCollection
            // System.Diagnostics.TraceListenerCollection
            // System.Net.Configuration.AuthenticationModuleElementCollection
            // System.Net.Configuration.BypassElementCollection
            // System.Net.Configuration.ConnectionManagementElementCollection
            // System.Net.Configuration.WebRequestModuleElementCollection
            // System.Net.CookieCollection
            // System.Net.CredentialCache
            // System.Net.WebHeaderCollection
            // System.Security.Authentication.ExtendedProtection.Configuration.ServiceNameElementCollection
            // System.Security.Authentication.ExtendedProtection.ServiceNameCollection
            // System.Security.Cryptography.AsnEncodedDataCollection
            // System.Security.Cryptography.OidCollection
            // System.Security.Cryptography.X509Certificates.X509Certificate2Collection
            // System.Security.Cryptography.X509Certificates.X509CertificateCollection
            // System.Security.Cryptography.X509Certificates.X509ChainElementCollection
            // System.Security.Cryptography.X509Certificates.X509ExtensionCollection
            // System.Text.RegularExpressions.CaptureCollection
            // System.Text.RegularExpressions.GroupCollection
            // System.Text.RegularExpressions.MatchCollection
        }
    }

    public class Sequence<T, TData> : IEnumerable<T>
    {
        private readonly int initialThreadId;

        private readonly TData data;

        private readonly Func<TData, Iterator<T>> createEnumerator;

        private Iterator<T> initialThreadIterator;

        public Sequence(TData data, Func<TData, Iterator<T>> createEnumerator)
        {
            this.data = data;
            this.createEnumerator = createEnumerator;
            this.initialThreadId = Environment.CurrentManagedThreadId;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (this.initialThreadId == Environment.CurrentManagedThreadId)
            {
                if (this.initialThreadIterator == null)
                {
                    this.initialThreadIterator = this.createEnumerator(this.data);
                }

                if (this.initialThreadIterator.State == IteratorState.Create)
                {
                    return this.initialThreadIterator.StartState();
                }
            }

            return this.createEnumerator(this.data).StartState();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public enum IteratorState
    {
        Create = -2,

        Start = 0,

        Next = 1,

        End = -1,

        Error = -3
    }

    public class Iterator<T> : IEnumerator<T>
    {
        private readonly Action start;

        private readonly Func<bool> hasNext;

        private readonly Func<T> next;

        private readonly Action dispose;

        private readonly Action end;

        public Iterator(
            Action start = null, Func<bool> hasNext = null, Func<T> next = null, Action dispose = null, Action end = null)
        {
            this.start = start ?? (() => { });
            this.hasNext = hasNext ?? (() => false);
            this.next = next ?? (() => default(T));
            this.dispose = dispose ?? (() => { });
            this.end = end ?? (() => { });
            this.State = IteratorState.Create;
        }

        public IteratorState State { get; private set; }

        public T Current { get; private set; }

        object IEnumerator.Current => this.Current;

        public Iterator<T> StartState()
        {
            this.State = IteratorState.Start;
            return this;
        }

        public bool MoveNext()
        {
            try
            {
                switch (this.State)
                {
                    case IteratorState.Start:
                        this.start();
                        this.State = IteratorState.Next;
                        goto case IteratorState.Next;
                    case IteratorState.Next:
                        if (this.hasNext())
                        {
                            this.Current = this.next();
                            return true;
                        }

                        this.State = IteratorState.End;
                        this.dispose();
                        this.end();
                        return false;
                    default:
                        return false;
                }
            }
            catch
            {
                this.State = IteratorState.Error;
                this.Dispose();
                throw;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose()
        {
            if (this.State == IteratorState.Error || this.State == IteratorState.Next)
            {
                try
                {
                }
                finally
                {
                    // https://msdn.microsoft.com/en-us/library/ty8d3wta.aspx
                    // Unexecuted finally blocks are executed before the thread is aborted.
                    this.State = IteratorState.End;
                    this.dispose();
                }
            }
        }
    }

    public static partial class IteratorPattern
    {
        public static IEnumerable<TSource> Enumerable<TSource>
            (TSource value) => new Sequence<TSource, bool>(false, isValueIterated => new Iterator<TSource>(
                hasNext: () =>
                {
                    if (isValueIterated)
                    {
                        return false;
                    }

                    isValueIterated = true;
                    return true;
                },
                next: () => value));
    }

    public static partial class IteratorPattern
    {
        public static void ForEachEnumerable<TSource>(TSource value)
        {
            foreach (TSource _ in Enumerable(value))
            {
            }
        }

        public static void CompiledForEachEnumerable<TSource>(TSource value)
        {
            using (IEnumerator<TSource> iterator = Enumerable(value).GetEnumerator())
            { // bool isValueIterated = false;
                while (iterator.MoveNext()) // hasNext: while (!isValueIterated);
                { // hasNext: isValueIterated = true.
                    TSource _ = iterator.Current; // next: TSource _ = value;
                }
            }
        }

        public static IEnumerable<TSource> Repeat<TSource>
            (TSource value, int count) => new Sequence<TSource, int>(0, index => new Iterator<TSource>(
                hasNext: () => index++ < count,
                next: () => value));

        public static void CompiledForEachRepeat<TSource>(TSource value, int count)
        {
            using (IEnumerator<TSource> iterator = Repeat(value, count).GetEnumerator())
            { // int index = 0;
                while (iterator.MoveNext()) // hasNext: while (index++ < count)
                {
                    TSource _ = iterator.Current; // next: TSource _ = value;
                }
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>
            (IEnumerable<TSource> source, Func<TSource, TResult> selector) =>
                new Sequence<TResult, IEnumerator<TSource>>(null, sourceIterator => new Iterator<TResult>(
                    start: () => sourceIterator = source.GetEnumerator(),
                    hasNext: () => sourceIterator.MoveNext(),
                    next: () => selector(sourceIterator.Current),
                    dispose: () => sourceIterator?.Dispose(),
                    end: () => sourceIterator = null));

        public static void CompiledForEachSelect<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            using (IEnumerator<TResult> iterator = Select(source, selector).GetEnumerator())
            { // IEnumerator<TSource> sourceIterator = null;
              // start: sourceIterator = source.GetEnumerator();
                while (iterator.MoveNext()) // hasNext: while (sourceIterator.MoveNext())
                {
                    TResult _ = iterator.Current; // next: TResult _ = selector(sourceIterator.Current);
                }
            } // dispose: sourceIterator?.Dispose();
              // end: sourceIterator = null;
        }

        public static IEnumerable<TSource> Where<TSource>
            (IEnumerable<TSource> source, Func<TSource, bool> predicate) =>
                new Sequence<TSource, IEnumerator<TSource>>(null, sourceIterator => new Iterator<TSource>(
                    start: () => sourceIterator = source.GetEnumerator(),
                    hasNext: () =>
                    {
                        while (sourceIterator.MoveNext())
                        {
                            if (predicate(sourceIterator.Current))
                            {
                                return true;
                            }
                        }

                        return false;
                    },
                    next: () => sourceIterator.Current,
                    dispose: () => sourceIterator?.Dispose(),
                    end: () => sourceIterator = null));

        public static void CompiledForEachWhere<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (IEnumerator<TSource> iterator = Where(source, predicate).GetEnumerator())
            { // IEnumerator<TSource> sourceIterator = null;
              // start: sourceIterator = source.GetEnumerator();
                while (iterator.MoveNext()) // hasNext: while (sourceIterator.MoveNext())
                { // hasNext: if (predicate(sourceIterator.Current))
                    TSource _ = iterator.Current; // next: TResult _ = sourceIterator.Current;
                }
            } // dispose: sourceIterator?.Dispose();
              // end: sourceIterator = null;
        }
    }

    public static partial class Generator
    {
        public static IEnumerable<TSource> Enumerable<TSource>(TSource value)
        {
            bool isValueIterated = false;
            while (!isValueIterated) // hasNext.
            {
                isValueIterated = true; // hasNext.
                yield return value; // next.
            }
        }
    }

    public static partial class Generator
    {
        public static IEnumerable<TSource> Enumerable2<TSource>(TSource value)
        {
            yield return value;
        }

        public static IEnumerable<TSource> Repeat<TSource>(TSource value, int count)
        {
            int index = 0;
            while (index++ < count) // hasNext.
            {
                yield return value; // next.
            }
        }

        public static IEnumerable<TSource> Repeat2<TSource>(TSource value, int count)
        {
            for (int index = 0; index < count; index++)
            {
                yield return value;
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator()) // start.
            {
                while (sourceIterator.MoveNext()) // hasNext.
                {
                    yield return selector(sourceIterator.Current); // next.
                }
            }
        }

        public static IEnumerable<TResult> Select2<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (TSource value in source)
            {
                yield return selector(value);
            }
        }

        public static IEnumerable<TSource> Where<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            using (IEnumerator<TSource> sourceIterator = source.GetEnumerator()) // start.
            {
                while (sourceIterator.MoveNext()) // hasNext.
                {
                    if (predicate(sourceIterator.Current)) // hasNext.
                    {
                        yield return sourceIterator.Current; // next.
                    }
                }
            }
        }

        public static IEnumerable<TSource> Where2<TSource>(
            IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource value in source)
            {
                if (predicate(value))
                {
                    yield return value;
                }
            }
        }
    }

    public static partial class Generator
    {
        public static IEnumerable<TResult> CompiledSelect<TSource, TResult>(
            IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return new SelectGenerator<TSource, TResult>((int)IteratorState.Create)
            {
                source = source,
                selector = selector
            };
        }
    }

    [CompilerGenerated]
    public sealed class SelectGenerator<TSource, TResult> : IEnumerable<TResult>, IEnumerator<TResult>
    {
        private readonly int initialThreadId;

        private int state;

        public IEnumerable<TSource> source;

        public Func<TSource, TResult> selector;

        private TResult current;

        private IEnumerator<TSource> sourceIterator;

        [DebuggerHidden]
        public SelectGenerator(int state)
        {
            this.state = state;
            this.initialThreadId = Environment.CurrentManagedThreadId;
        }

        TResult IEnumerator<TResult>.Current
        {
            [DebuggerHidden]get { return this.current; }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]get { return this.current; }
        }

        bool IEnumerator.MoveNext()
        {
            try
            {
                switch (this.state)
                {
                    case (int)IteratorState.Start:
                        this.sourceIterator = this.source.GetEnumerator(); // start.
                        this.state = (int)IteratorState.Next;
                        goto case (int)IteratorState.Next;
                    case (int)IteratorState.Next:
                        if (this.sourceIterator.MoveNext()) // hasNext.
                        {
                            this.current = this.selector(this.sourceIterator.Current); // next.
                            return true;
                        }

                        this.state = (int)IteratorState.End;
                        this.sourceIterator?.Dispose(); // dispose.
                        this.sourceIterator = null; // end.
                        return false;
                    default:
                        return false;
                }
            }
            catch
            {
                this.state = (int)IteratorState.Error;
                (this as IDisposable).Dispose();
                throw;
            }
        }

        [DebuggerHidden]
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        [DebuggerHidden]
        IEnumerator<TResult> IEnumerable<TResult>.GetEnumerator()
        {
            if (this.state == -2 && this.initialThreadId == Environment.CurrentManagedThreadId)
            {
                this.state = 0;
                return this;
            }

            return new SelectGenerator<TSource, TResult>(0)
            {
                source = this.source,
                selector = this.selector
            };
        }

        [DebuggerHidden]
        IEnumerator IEnumerable.GetEnumerator() => (this as IEnumerable<TResult>).GetEnumerator();

        [DebuggerHidden]
        void IDisposable.Dispose()
        {
            if (this.state == (int)IteratorState.Error || this.state == (int)IteratorState.Next)
            {
                try
                {
                }
                finally
                {
                    this.state = (int)IteratorState.End;
                    this.sourceIterator?.Dispose(); // dospose.
                }
            }
        }
    }
}

#if ERROR
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

        void Reset(); // Only for COM interoperability.
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