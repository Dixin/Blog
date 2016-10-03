namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static partial class Translation
    {
        private static readonly AdventureWorks AdventureWorks = new AdventureWorks();

        internal static void WhereAndSelect()
        {
            // IQueryable<string> products = AdventureWorks.Products
            //    .Where(product => product.Name.StartsWith("M")).Select(product => product.Name);
            IQueryable<Product> sourceQueryable = AdventureWorks.Products;
            IQueryable<Product> whereQueryable = sourceQueryable.Where(product => product.Name.StartsWith("M"));
            IQueryable<string> selectQueryable = whereQueryable.Select(product => product.Name); // Define query.
            selectQueryable.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }
    }

    internal static partial class Translation
    {
        internal static void WhereAndSelectExpressions()
        {
            IQueryable<Product> sourceQueryable = AdventureWorks.Products; // DbSet<Product>.

            // MethodCallExpression sourceMergeAsCallExpression = (MethodCallExpression)sourceQuery.Expression;
            ObjectQuery<Product> objectQuery = new ObjectQuery<Product>(
                $"[{nameof(AdventureWorks)}].[{nameof(AdventureWorks.Products)}]",
                ((IObjectContextAdapter)AdventureWorks).ObjectContext,
                MergeOption.AppendOnly);
            MethodInfo mergeAsMethod = typeof(ObjectQuery<Product>)
                .GetTypeInfo().GetDeclaredMethods("MergeAs").Single();
            MethodCallExpression sourceMergeAsCallExpression = Expression.Call(
                instance: Expression.Constant(objectQuery),
                method: mergeAsMethod,
                arguments: Expression.Constant(MergeOption.AppendOnly, typeof(MergeOption)));
            Trace.WriteLine(sourceQueryable.Expression);
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            IQueryProvider sourceQueryProvider = sourceQueryable.Provider; // DbQueryProvider.

            // Expression<Func<Product, bool>> predicateExpression = product => product.Name.StartsWith("M");
            ParameterExpression productParameterExpression = Expression.Parameter(typeof(Product), "product");
            Func<string, bool> startsWithMethod = string.Empty.StartsWith;
            Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                Expression.Call(
                    instance: Expression.Property(productParameterExpression, nameof(Product.Name)),
                    method: startsWithMethod.Method,
                    arguments: Expression.Constant("M", typeof(string))),
                productParameterExpression);
            Trace.WriteLine(predicateExpression);
            // product => product.Name.StartsWith("M")

            // IQueryable<Product> whereQueryable = sourceQueryable.Where(predicateExpression);
            Func<IQueryable<Product>, Expression<Func<Product, bool>>, IQueryable<Product>> whereMethod =
                Queryable.Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: whereMethod.Method,
                arg0: sourceMergeAsCallExpression,
                arg1: Expression.Quote(predicateExpression));
            IQueryable<Product> whereQueryable = sourceQueryProvider
                .CreateQuery<Product>(whereCallExpression); // DbQuery<Product>.
            Trace.WriteLine(object.ReferenceEquals(whereCallExpression, whereQueryable.Expression)); // True.
            Trace.WriteLine(whereQueryable.Expression);
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Where(product => product.Name.StartsWith("M"))
            IQueryProvider whereQueryProvider = whereQueryable.Provider; // DbQueryProvider.

            // Expression<Func<Product, string>> selectorExpression = product => product.Name;
            Expression<Func<Product, string>> selectorExpression = Expression.Lambda<Func<Product, string>>(
                    Expression.Property(productParameterExpression, nameof(Product.Name)),
                    productParameterExpression);
            Trace.WriteLine(selectorExpression);
            // product => product.Name

            // IQueryable<string> selectQueryable = whereQueryable.Select(selectorExpression);
            Func<IQueryable<Product>, Expression<Func<Product, string>>, IQueryable<string>> selectMethod =
                Queryable.Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: selectMethod.Method,
                arg0: whereCallExpression,
                arg1: Expression.Quote(selectorExpression));
            IQueryable<string> selectQueryable = whereQueryProvider
                .CreateQuery<string>(selectCallExpression); // DbQuery<Product>.
            Trace.WriteLine(object.ReferenceEquals(selectCallExpression, selectQueryable.Expression)); // True.
            Trace.WriteLine(selectQueryable.Expression);
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Where(product => product.Name.StartsWith("M"))
            //    .Select(product => product.Name)

            selectQueryable.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static void WhereAndSelectExpressionsToDbExpressions()
        {
            Expression expression = AdventureWorks.Products
               .Where(product => product.Name.StartsWith("M")).Select(product => product.Name).Expression;
            DbQueryCommandTree commandTree = AdventureWorks.Convert(expression);
            Trace.WriteLine(commandTree);
        }

        internal static DbQueryCommandTree WhereAndSelectDbExpressions()
        {
            MetadataWorkspace metadata = ((IObjectContextAdapter)AdventureWorks).ObjectContext.MetadataWorkspace;
            TypeUsage stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetPrimitiveTypes(DataSpace.CSpace)
                .Single(type => type.ClrEquivalentType == typeof(string)));
            TypeUsage nameRowTypeUsage = TypeUsage.CreateDefaultTypeUsage(RowType.Create(
                Enumerable.Repeat(EdmProperty.Create(nameof(Product.Name), stringTypeUsage), 1),
                Enumerable.Empty<MetadataProperty>()));
            TypeUsage productTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetType(nameof(Product), "CodeFirstDatabaseSchema", DataSpace.SSpace));
            EntitySet productEntitySet = metadata
                .GetEntityContainer("CodeFirstDatabase", DataSpace.SSpace)
                .GetEntitySetByName(nameof(Product), false);

            DbProjectExpression query = DbExpressionBuilder.Project(
                DbExpressionBuilder.BindAs(
                    DbExpressionBuilder.Filter(
                        DbExpressionBuilder.BindAs(
                            DbExpressionBuilder.Scan(productEntitySet), "Extent1"),
                        DbExpressionBuilder.Like(
                            DbExpressionBuilder.Property(
                                DbExpressionBuilder.Variable(productTypeUsage, "Extent1"), nameof(Product.Name)),
                            DbExpressionBuilder.Constant("M%"))),
                    "Filter1"),
                DbExpressionBuilder.New(
                    nameRowTypeUsage,
                    DbExpressionBuilder.Property(
                        DbExpressionBuilder.Variable(productTypeUsage, "Filter1"), nameof(Product.Name))));
            DbQueryCommandTree commandTree = new DbQueryCommandTree(metadata, DataSpace.SSpace, query);
            Trace.WriteLine(commandTree);
            return commandTree;
        }

        internal static void WhereAndSelectDbExpressionsToSql()
        {
            DbQueryCommandTree commandTree = WhereAndSelectDbExpressions();
            string sql = AdventureWorks.Generate(commandTree).CommandText;
            Trace.WriteLine(sql);
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE [Extent1].[Name] LIKE N'M%'
        }

        internal static void SelectAndFirst()
        {
            // string first = AdventureWorks.Products.Select(product => product.Name).First();
            IQueryable<Product> sourceQueryable = AdventureWorks.Products;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            string first = selectQueryable.First();
            Trace.WriteLine(first);
        }

        internal static void SelectAndFirstExpressions()
        {
            IQueryable<Product> sourceQueryable = AdventureWorks.Products;
            Trace.WriteLine(sourceQueryable.Expression);
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)

            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            Trace.WriteLine(selectQueryable.Expression);
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            MethodCallExpression selectCallExpression = (MethodCallExpression)selectQueryable.Expression;
            IQueryProvider selectQueryProvider = selectQueryable.Provider; // DbQueryProvider.

            // string first = selectQueryable.First();
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(firstMethod.Method, selectCallExpression);
            Trace.WriteLine(firstCallExpression);
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            //    .First()
            string first = selectQueryProvider.Execute<string>(firstCallExpression); // Execute query.
            Trace.WriteLine(first);
        }

        internal static void SelectAndFirstQuery()
        {
            IQueryable<Product> sourceQueryable = AdventureWorks.Products;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);

            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(firstMethod.Method, selectQueryable.Expression);
            // IQueryable<string> firstQueryable = selectQueryable.Provider._internalQuery.ObjectQueryProvider
            //    .CreateQuery<string>(firstCallExpression);
            // Above _internalQuery, ObjectQueryProvider and CreateQuery are not public. Reflection is needed:
            Assembly entityFrmaeworkAssembly = typeof(DbContext).Assembly;
            Type dbQueryProviderType = entityFrmaeworkAssembly.GetType(
                "System.Data.Entity.Internal.Linq.DbQueryProvider");
            FieldInfo internalQueryField = dbQueryProviderType.GetField(
                "_internalQuery", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            Type internalQueryType = entityFrmaeworkAssembly.GetType("System.Data.Entity.Internal.Linq.IInternalQuery");
            PropertyInfo objectQueryProviderProperty = internalQueryType.GetProperty("ObjectQueryProvider");
            Type objectQueryProviderType = entityFrmaeworkAssembly.GetType(
                "System.Data.Entity.Core.Objects.ELinq.ObjectQueryProvider");
            MethodInfo createQueryMethod = objectQueryProviderType
                .GetMethod(
                    "CreateQuery",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod,
                    null,
                    new Type[] { typeof(Expression) },
                    null)
                .MakeGenericMethod(typeof(string));
            object internalQuery = internalQueryField.GetValue(selectQueryable.Provider);
            object objectProvider = objectQueryProviderProperty.GetValue(internalQuery);
            IQueryable<string> firstQueryable = (IQueryable<string>)createQueryMethod.Invoke(
                objectProvider, new object[] { firstCallExpression });

            Func<IEnumerable<string>, string> firstMappingMethod = Enumerable.First;
            string first = firstMappingMethod(firstQueryable); // Execute query.
            Trace.WriteLine(first);
        }

        internal static DbQueryCommandTree SelectAndFirstDbExpressions()
        {
            MetadataWorkspace metadata = ((IObjectContextAdapter)AdventureWorks).ObjectContext.MetadataWorkspace;
            TypeUsage stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetPrimitiveTypes(DataSpace.CSpace)
                .Single(type => type.ClrEquivalentType == typeof(string)));
            TypeUsage nameRowTypeUsage = TypeUsage.CreateDefaultTypeUsage(RowType.Create(
                Enumerable.Repeat(EdmProperty.Create(nameof(Product.Name), stringTypeUsage), 1),
                Enumerable.Empty<MetadataProperty>()));
            TypeUsage productTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetType(nameof(Product), "CodeFirstDatabaseSchema", DataSpace.SSpace));
            EntitySet productEntitySet = metadata
                .GetEntityContainer("CodeFirstDatabase", DataSpace.SSpace)
                .GetEntitySetByName(nameof(Product), false);

            DbProjectExpression query = DbExpressionBuilder.Project(
                DbExpressionBuilder.BindAs(
                    DbExpressionBuilder.Limit(
                        DbExpressionBuilder.Scan(productEntitySet),
                        DbExpressionBuilder.Constant(1)),
                    "Limit1"),
                DbExpressionBuilder.New(
                    nameRowTypeUsage,
                    DbExpressionBuilder.Property(
                        DbExpressionBuilder.Variable(productTypeUsage, "Limit1"), nameof(Product.Name))));
            DbQueryCommandTree commandTree = new DbQueryCommandTree(metadata, DataSpace.SSpace, query);
            Trace.WriteLine(commandTree);
            return commandTree;
        }

        internal static void SelectAndFirstDbExpressionsToSql()
        {
            DbQueryCommandTree commandTree = SelectAndFirstDbExpressions();
            string sql = AdventureWorks.Generate(commandTree).CommandText;
            Trace.WriteLine(sql);
            // SELECT TOP (1) 
            //    [c].[Name] AS [Name]
            //    FROM [Production].[Product] AS [c]
        }
    }

    internal static partial class Translation
    {
        internal static DbQueryCommandTree StringIsNullOrEmptyDbExpressions()
        {
            IQueryable<string> products = AdventureWorks.Products
                .Select(product => product.Name)
                .Where(name => string.IsNullOrEmpty(name));
            return AdventureWorks.Convert(products.Expression);
        }

        private static bool FilterName(string name) => string.IsNullOrEmpty(name);

        internal static void RemoteMethodCall()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IQueryable<string> products = source
                .Select(product => product.Name)
                .Where(name => FilterName(name)); // Define query.
            products.ForEach(product => Trace.WriteLine(product)); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Boolean FilterName(System.String)' method, and this method cannot be translated into a store expression.
        }

        internal static void LocalMethodCall()
        {
            IQueryable<Product> source = AdventureWorks.Products;
            IEnumerable<string> products = source
                .Select(product => product.Name) // LINQ to Entities.
                .AsEnumerable() // LINQ to Objects.
                .Where(name => FilterName(name)); // Define query.
            products.ForEach(product => Trace.WriteLine(product)); // Execute query.
        }

        internal static DbQueryCommandTree DbFunctionDbExpressions()
        {
            var photos = AdventureWorks.ProductPhotos.Select(photo => new
            {
                LargePhotoFileName = photo.LargePhotoFileName,
                UnmodifiedDays = DbFunctions.DiffDays(photo.ModifiedDate, DateTime.Now)
            });
            return AdventureWorks.Convert(photos.Expression);
        }

        internal static DbQueryCommandTree SqlFunctionDbExpressions()
        {
            IQueryable<string> products = AdventureWorks.Products
                .Select(product => product.Name)
                .Where(name => SqlFunctions.PatIndex(name, "%o%a%") > 0);
            return AdventureWorks.Convert(products.Expression);
        }

        internal static void StringIsNullOrEmptySql()
        {
            string sql = AdventureWorks.Generate(StringIsNullOrEmptyDbExpressions()).CommandText;
            Trace.WriteLine(sql);
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE (LEN([Extent1].[Name])) = 0
        }

        internal static void DbFunctionSql()
        {
            string sql = AdventureWorks.Generate(DbFunctionDbExpressions()).CommandText;
            Trace.WriteLine(sql);
            // SELECT 
            //    1 AS [C1], 
            //    [Extent1].[LargePhotoFileName] AS [LargePhotoFileName], 
            //    DATEDIFF (day, [Extent1].[ModifiedDate], SysDateTime()) AS [C2]
            //    FROM [Production].[ProductPhoto] AS [Extent1]
        }

        internal static void SqlFunctionSql()
        {
            string sql = AdventureWorks.Generate(SqlFunctionDbExpressions()).CommandText;
            Trace.WriteLine(sql);
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE ( CAST(PATINDEX([Extent1].[Name], N'%o%a%') AS int)) > 0
        }
    }

    public partial class LogProviderServices : DbProviderServices
    {
        private static readonly SqlProviderServices Sql = SqlProviderServices.Instance;

        private static object RedirectCall(
            Type[] argumentTypes, object[] arguments, [CallerMemberName] string methodName = null)
            => typeof(SqlProviderServices)
                .GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,
                    null,
                    argumentTypes,
                    null)
                .Invoke(Sql, arguments);

        private static object RedirectCall<T>(T arg, [CallerMemberName] string methodName = null)
            => RedirectCall(new Type[] { typeof(T) }, new object[] { arg }, methodName);

        private static object RedirectCall<T1, T2>(T1 arg1, T2 arg2, [CallerMemberName] string methodName = null)
            => RedirectCall(new Type[] { typeof(T1), typeof(T2) }, new object[] { arg1, arg2 }, methodName);

        private static object RedirectCall<T1, T2, T3>(
            T1 arg1, T2 arg2, T3 arg3, [CallerMemberName] string methodName = null) => RedirectCall(
                new Type[] { typeof(T1), typeof(T2), typeof(T3) }, new object[] { arg1, arg2, arg3 }, methodName);
    }

    public partial class LogProviderServices
    {
        protected override DbCommandDefinition CreateDbCommandDefinition(
            DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            Trace.WriteLine(commandTree);
            return (DbCommandDefinition)RedirectCall(providerManifest, commandTree);
        }
    }

    public partial class LogProviderServices
    {
        public override void RegisterInfoMessageHandler(DbConnection connection, Action<string> handler)
                => Sql.RegisterInfoMessageHandler(connection, handler);

        protected override DbCommand CloneDbCommand(DbCommand fromDbCommand)
            => (DbCommand)RedirectCall(fromDbCommand);

        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value)
            => RedirectCall(parameter, parameterType, value);

        protected override string GetDbProviderManifestToken(DbConnection connection)
            => (string)RedirectCall(connection);

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
            => (DbProviderManifest)RedirectCall(manifestToken);

        protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string versionHint)
            => (DbSpatialDataReader)RedirectCall<DbDataReader, string>(fromReader, versionHint);

        protected override DbSpatialServices DbGetSpatialServices(string versionHint)
            => (DbSpatialServices)RedirectCall(versionHint);

        protected override string DbCreateDatabaseScript(
            string providerManifestToken, StoreItemCollection storeItemCollection)
            => (string)RedirectCall(providerManifestToken, storeItemCollection);

        protected override void DbCreateDatabase(
            DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
            => RedirectCall(connection, commandTimeout, storeItemCollection);

        protected override bool DbDatabaseExists(
            DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
            => (bool)RedirectCall(connection, commandTimeout, storeItemCollection);

        protected override bool DbDatabaseExists(
            DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection)
            => (bool)RedirectCall(connection, commandTimeout, storeItemCollection);

        protected override void DbDeleteDatabase(
            DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection)
            => RedirectCall(connection, commandTimeout, storeItemCollection);
    }

#if DEMO
    public class LogConfiguration : DbConfiguration
    {
        public LogConfiguration()
        {
            this.SetProviderServices(SqlProviderServices.ProviderInvariantName, new LogProviderServices());
        }
    }
#endif
}

#if DEMO
namespace System.Linq
{
    using System.Collections;
    using System.Collections.Generic;

    public interface IQueryable<out T> : IEnumerable<T>, IEnumerable, IQueryable
    {
        // Expression Expression { get; } from IQueryable.

        // Type ElementType { get; } from IQueryable.

        // IQueryProvider Provider { get; } from IQueryable.

        // IEnumerator<T> GetEnumerator(); from IEnumerable<T>.
    }
}

namespace System.Linq
{
    using System.Linq.Expressions;

    public interface IQueryProvider
    {
        IQueryable CreateQuery(Expression expression);

        IQueryable<TElement> CreateQuery<TElement>(Expression expression);

        object Execute(Expression expression);

        TResult Execute<TResult>(Expression expression);
    }
}

namespace System.Linq
{
    using System.Linq.Expressions;

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, IQueryable<TSource>> currentMethod =
                Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression,
                arg1: Expression.Quote(predicate));
            return source.Provider.CreateQuery<TSource>(whereCallExpression);
        }

        public static IQueryable<TResult> Select<TSource, TResult>(
            this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, TResult>>, IQueryable<TResult>> currentMethod =
                Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression,
                arg1: Expression.Quote(selector));
            return source.Provider.CreateQuery<TResult>(selectCallExpression);
        }

        public static TSource First<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource> currentMethod = First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression,
                arg1: Expression.Quote(predicate));
            return source.Provider.Execute<TSource>(firstCallExpression);
        }

        public static TSource First<TSource>(this IQueryable<TSource> source)
        {
            Func<IQueryable<TSource>, TSource> currentMethod = First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: currentMethod.Method,
                arg0: source.Expression);
            return source.Provider.Execute<TSource>(firstCallExpression);
        }

        // Other methods...
    }
}

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    public abstract class DbExpression
    {
        public virtual DbExpressionKind ExpressionKind { get; }

        public virtual TypeUsage ResultType { get; }

        // Other members.
    }

    public sealed class DbFilterExpression : DbExpression
    {
        public DbExpressionBinding Input { get; }

        public DbExpression Predicate { get; }

        // Other members.
    }

    public sealed class DbProjectExpression : DbExpression
    {
        public DbExpressionBinding Input { get; }

        public DbExpression Projection { get; }

        // Other members.
    }

    public sealed class DbLimitExpression : DbExpression
    {
        public DbExpression Argument { get; }

        public DbExpression Limit { get; }

        // Other members.
    }
}

namespace System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder
{
    using System.Data.Entity.Core.Metadata.Edm;

    public static class DbExpressionBuilder
    {
        public static DbFilterExpression Filter(this DbExpressionBinding input, DbExpression predicate);

        public static DbProjectExpression Project(this DbExpressionBinding input, DbExpression projection);

        public static DbLimitExpression Limit(this DbExpression argument, DbExpression count);

        public static DbScanExpression Scan(this EntitySetBase targetSet);

        public static DbPropertyExpression Property(this DbExpression instance, string propertyName);

        public static DbVariableReferenceExpression Variable(this TypeUsage type, string name);

        public static DbConstantExpression Constant(object value);

        // Other methods...
    }
}

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    public abstract class DbCommandTree
    {
        public IEnumerable<KeyValuePair<string, TypeUsage>> Parameters { get; }
    }
    public sealed class DbQueryCommandTree : DbCommandTree
    {
        public DbExpression Query { get; }
    }
}

namespace System.Data.Entity.Core.Common.CommandTrees
{
    public abstract class DbExpressionVisitor<TResultType>
    {
        public abstract TResultType Visit(DbFilterExpression expression);

        public abstract TResultType Visit(DbProjectExpression expression);

        public abstract TResultType Visit(DbLimitExpression expression);

        public abstract TResultType Visit(DbScanExpression expression);

        public abstract TResultType Visit(DbPropertyExpression expression);

        public abstract TResultType Visit(DbVariableReferenceExpression expression);

        public abstract TResultType Visit(DbConstantExpression expression);

        // Other methods.
    }
}

namespace System.Data.Entity.SqlServer.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;

    internal interface ISqlFragment
    {
    }

    internal class SqlGenerator : DbExpressionVisitor<ISqlFragment>
    {
        internal string GenerateSql(DbQueryCommandTree tree, out HashSet<string> paramsToForceNonUnicode);

        // Other members.
    }
}


namespace System.Data.Entity.Core.Common
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Infrastructure.DependencyResolution;

    public abstract class DbProviderServices : IDbDependencyResolver
    {
        protected abstract DbCommandDefinition CreateDbCommandDefinition(
            DbProviderManifest providerManifest, DbCommandTree commandTree);

        // Other members.
    }
}

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;

    public sealed class SqlProviderServices : DbProviderServices
    {
        protected override DbCommandDefinition CreateDbCommandDefinition(
            DbProviderManifest providerManifest, DbCommandTree commandTree);

        // Other members.
    }
}
#endif
