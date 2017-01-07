namespace Dixin.Linq.EntityFramework
{
#if NETFX
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
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static partial class Translation
    {
        internal static void WhereAndSelect(WideWorldImporters adventureWorks)
        {
            // IQueryable<string> products = AdventureWorks.Products
            //    .Where(product => product.Name.StartsWith("M")).Select(product => product.Name);
            IQueryable<StockItem> sourceQueryable = adventureWorks.StockItems;
            IQueryable<StockItem> whereQueryable = sourceQueryable.Where(product => product.StockItemName.StartsWith("M"));
            IQueryable<string> selectQueryable = whereQueryable.Select(product => product.StockItemName); // Define query.
            selectQueryable.WriteLines(); // Execute query.
        }
    }

    internal static partial class Translation
    {
        internal static void WhereAndSelectExpressions(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> sourceQueryable = adventureWorks.StockItems; // DbSet<Product>.

            // MethodCallExpression sourceMergeAsCallExpression = (MethodCallExpression)sourceQuery.Expression;
            ObjectQuery<StockItem> objectQuery = new ObjectQuery<StockItem>(
                $"[{nameof(WideWorldImporters)}].[{nameof(WideWorldImporters.StockItems)}]",
                ((IObjectContextAdapter)adventureWorks).ObjectContext,
                MergeOption.AppendOnly);
            MethodInfo mergeAsMethod = typeof(ObjectQuery<StockItem>)
                .GetTypeInfo().GetDeclaredMethods("MergeAs").Single();
            MethodCallExpression sourceMergeAsCallExpression = Expression.Call(
                instance: Expression.Constant(objectQuery),
                method: mergeAsMethod,
                arguments: Expression.Constant(MergeOption.AppendOnly, typeof(MergeOption)));
            sourceQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            IQueryProvider sourceQueryProvider = sourceQueryable.Provider; // DbQueryProvider.

            // Expression<Func<Product, bool>> predicateExpression = product => product.Name.StartsWith("M");
            ParameterExpression productParameterExpression = Expression.Parameter(typeof(StockItem), "product");
            Func<string, bool> startsWithMethod = string.Empty.StartsWith;
            Expression<Func<StockItem, bool>> predicateExpression = Expression.Lambda<Func<StockItem, bool>>(
                Expression.Call(
                    instance: Expression.Property(productParameterExpression, nameof(StockItem.StockItemName)),
                    method: startsWithMethod.Method,
                    arguments: Expression.Constant("M", typeof(string))),
                productParameterExpression);
            predicateExpression.WriteLine();
            // product => product.Name.StartsWith("M")

            // IQueryable<Product> whereQueryable = sourceQueryable.Where(predicateExpression);
            Func<IQueryable<StockItem>, Expression<Func<StockItem, bool>>, IQueryable<StockItem>> whereMethod =
                Queryable.Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: whereMethod.Method,
                arg0: sourceMergeAsCallExpression,
                arg1: Expression.Quote(predicateExpression));
            IQueryable<StockItem> whereQueryable = sourceQueryProvider
                .CreateQuery<StockItem>(whereCallExpression); // DbQuery<Product>.
            object.ReferenceEquals(whereCallExpression, whereQueryable.Expression).WriteLine(); // True.
            whereQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Where(product => product.Name.StartsWith("M"))
            IQueryProvider whereQueryProvider = whereQueryable.Provider; // DbQueryProvider.

            // Expression<Func<Product, string>> selectorExpression = product => product.Name;
            Expression<Func<StockItem, string>> selectorExpression = Expression.Lambda<Func<StockItem, string>>(
                    Expression.Property(productParameterExpression, nameof(StockItem.StockItemName)),
                    productParameterExpression);
            selectorExpression.WriteLine();
            // product => product.Name

            // IQueryable<string> selectQueryable = whereQueryable.Select(selectorExpression);
            Func<IQueryable<StockItem>, Expression<Func<StockItem, string>>, IQueryable<string>> selectMethod =
                Queryable.Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: selectMethod.Method,
                arg0: whereCallExpression,
                arg1: Expression.Quote(selectorExpression));
            IQueryable<string> selectQueryable = whereQueryProvider
                .CreateQuery<string>(selectCallExpression); // DbQuery<Product>.
            object.ReferenceEquals(selectCallExpression, selectQueryable.Expression).WriteLine(); // True.
            selectQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Where(product => product.Name.StartsWith("M"))
            //    .Select(product => product.Name)

            selectQueryable.WriteLines(); // Execute query.
        }

        internal static void WhereAndSelectCompileExpressions(WideWorldImporters adventureWorks)
        {
            Expression expression = adventureWorks.StockItems
               .Where(product => product.StockItemName.StartsWith("M")).Select(product => product.StockItemName).Expression;
            DbQueryCommandTree commandTree = adventureWorks.Compile(expression);
            commandTree.WriteLine();
        }

        internal static DbQueryCommandTree WhereAndSelectCompiledExpressions(WideWorldImporters adventureWorks)
        {
            MetadataWorkspace metadata = ((IObjectContextAdapter)adventureWorks).ObjectContext.MetadataWorkspace;
            TypeUsage stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetPrimitiveTypes(DataSpace.CSpace)
                .Single(type => type.ClrEquivalentType == typeof(string)));
            TypeUsage nameRowTypeUsage = TypeUsage.CreateDefaultTypeUsage(RowType.Create(
                Enumerable.Repeat(EdmProperty.Create(nameof(StockItem.StockItemName), stringTypeUsage), 1),
                Enumerable.Empty<MetadataProperty>()));
            TypeUsage productTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetType(nameof(StockItem), "CodeFirstDatabaseSchema", DataSpace.SSpace));
            EntitySet productEntitySet = metadata
                .GetEntityContainer("CodeFirstDatabase", DataSpace.SSpace)
                .GetEntitySetByName(nameof(StockItem), false);

            DbProjectExpression query = DbExpressionBuilder.Project(
                DbExpressionBuilder.BindAs(
                    DbExpressionBuilder.Filter(
                        DbExpressionBuilder.BindAs(
                            DbExpressionBuilder.Scan(productEntitySet), "Extent1"),
                        DbExpressionBuilder.Like(
                            DbExpressionBuilder.Property(
                                DbExpressionBuilder.Variable(productTypeUsage, "Extent1"), nameof(StockItem.StockItemName)),
                            DbExpressionBuilder.Constant("M%"))),
                    "Filter1"),
                DbExpressionBuilder.New(
                    nameRowTypeUsage,
                    DbExpressionBuilder.Property(
                        DbExpressionBuilder.Variable(productTypeUsage, "Filter1"), nameof(StockItem.StockItemName))));
            DbQueryCommandTree commandTree = new DbQueryCommandTree(metadata, DataSpace.SSpace, query);
            return commandTree.WriteLine();
        }

        internal static void WhereAndSelectGenerateSql(WideWorldImporters adventureWorks)
        {
            DbQueryCommandTree commandTree = WhereAndSelectCompiledExpressions(adventureWorks);
            DbCommand command = adventureWorks.Generate(commandTree);
            command.CommandText.WriteLine();
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE [Extent1].[Name] LIKE N'M%'
        }

        internal static void SelectAndFirst(WideWorldImporters adventureWorks)
        {
            // string first = AdventureWorks.Products.Select(product => product.Name).First();
            IQueryable<StockItem> sourceQueryable = adventureWorks.StockItems;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.StockItemName);
            string first = selectQueryable.First().WriteLine();
        }

        internal static void SelectAndFirstExpressions(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> sourceQueryable = adventureWorks.StockItems;
            sourceQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)

            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.StockItemName);
            selectQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            MethodCallExpression selectCallExpression = (MethodCallExpression)selectQueryable.Expression;
            IQueryProvider selectQueryProvider = selectQueryable.Provider; // DbQueryProvider.

            // string first = selectQueryable.First();
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(firstMethod.Method, selectCallExpression);
            firstCallExpression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Dixin.Linq.EntityFramework.Product])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            //    .First()
            string first = selectQueryProvider.Execute<string>(firstCallExpression).WriteLine(); // Execute query.
        }

        internal static void SelectAndFirstQuery(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> sourceQueryable = adventureWorks.StockItems;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.StockItemName);

            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(firstMethod.Method, selectQueryable.Expression);
            // IQueryable<string> firstQueryable = selectQueryable.Provider._internalQuery.ObjectQueryProvider
            //    .CreateQuery<string>(firstCallExpression);
            // Above _internalQuery, ObjectQueryProvider and CreateQuery are not public. Reflection is needed:
            Assembly entityFrameworkAssembly = typeof(DbContext).Assembly;
            Type dbQueryProviderType = entityFrameworkAssembly.GetType(
                "System.Data.Entity.Internal.Linq.DbQueryProvider");
            FieldInfo internalQueryField = dbQueryProviderType.GetField(
                "_internalQuery", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
            Type internalQueryType = entityFrameworkAssembly.GetType("System.Data.Entity.Internal.Linq.IInternalQuery");
            PropertyInfo objectQueryProviderProperty = internalQueryType.GetProperty("ObjectQueryProvider");
            Type objectQueryProviderType = entityFrameworkAssembly.GetType(
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
            string first = firstMappingMethod(firstQueryable).WriteLine(); // Execute query.
        }

        internal static DbQueryCommandTree SelectAndFirstCompiledExpressions(WideWorldImporters adventureWorks)
        {
            MetadataWorkspace metadata = ((IObjectContextAdapter)adventureWorks).ObjectContext.MetadataWorkspace;
            TypeUsage stringTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetPrimitiveTypes(DataSpace.CSpace)
                .Single(type => type.ClrEquivalentType == typeof(string)));
            TypeUsage nameRowTypeUsage = TypeUsage.CreateDefaultTypeUsage(RowType.Create(
                Enumerable.Repeat(EdmProperty.Create(nameof(StockItem.StockItemName), stringTypeUsage), 1),
                Enumerable.Empty<MetadataProperty>()));
            TypeUsage productTypeUsage = TypeUsage.CreateDefaultTypeUsage(metadata
                .GetType(nameof(StockItem), "CodeFirstDatabaseSchema", DataSpace.SSpace));
            EntitySet productEntitySet = metadata
                .GetEntityContainer("CodeFirstDatabase", DataSpace.SSpace)
                .GetEntitySetByName(nameof(StockItem), false);

            DbProjectExpression query = DbExpressionBuilder.Project(
                DbExpressionBuilder.BindAs(
                    DbExpressionBuilder.Limit(
                        DbExpressionBuilder.Scan(productEntitySet),
                        DbExpressionBuilder.Constant(1)),
                    "Limit1"),
                DbExpressionBuilder.New(
                    nameRowTypeUsage,
                    DbExpressionBuilder.Property(
                        DbExpressionBuilder.Variable(productTypeUsage, "Limit1"), nameof(StockItem.StockItemName))));
            DbQueryCommandTree commandTree = new DbQueryCommandTree(metadata, DataSpace.SSpace, query);
            return commandTree.WriteLine();
        }

        internal static void SelectAndFirstGenerateSql(WideWorldImporters adventureWorks)
        {
            DbQueryCommandTree commandTree = SelectAndFirstCompiledExpressions(adventureWorks);
            DbCommand command = adventureWorks.Generate(commandTree);
            command.CommandText.WriteLine();
            // SELECT TOP (1) 
            //    [c].[Name] AS [Name]
            //    FROM [Production].[Product] AS [c]
        }
    }

    public static partial class DbContextExtensions
    {
        public static DbQueryCommandTree Compile(this IObjectContextAdapter context, Expression expression)
        {
            ObjectContext objectContext = context.ObjectContext;

            // DbExpression dbExpression = new ExpressionConverter(
            //    Funcletizer.CreateQueryFuncletizer(objectContext), expression).Convert();
            // DbQueryCommandTree commandTree = objectContext.MetadataWorkspace.CreateQueryCommandTree(dbExpression);
            // List<ProviderCommandInfo> providerCommands;
            // PlanCompiler.Compile(
            //    commandTree, out providerCommands, out columnMap, out columnCount, out entitySets);
            // return (DbQueryCommandTree)providerCommands.Single().CommandTree;
            // ExpressionConverter, Funcletizer and PlanCompiler are not public. Reflection is needed:
            Assembly entityFrameworkAssembly = typeof(DbContext).Assembly;
            Type funcletizerType = entityFrameworkAssembly.GetType(
                "System.Data.Entity.Core.Objects.ELinq.Funcletizer");
            MethodInfo createQueryFuncletizerMethod = funcletizerType.GetMethod(
                "CreateQueryFuncletizer", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod);
            Type expressionConverterType = entityFrameworkAssembly.GetType(
                "System.Data.Entity.Core.Objects.ELinq.ExpressionConverter");
            ConstructorInfo expressionConverterConstructor = expressionConverterType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Type[] { funcletizerType, typeof(Expression) },
                null);
            MethodInfo convertMethod = expressionConverterType.GetMethod(
                "Convert", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);
            object funcletizer = createQueryFuncletizerMethod.Invoke(null, new object[] { objectContext });
            object expressionConverter = expressionConverterConstructor.Invoke(
                new object[] { funcletizer, expression });
            DbExpression dbExpression = (DbExpression)convertMethod.Invoke(expressionConverter, new object[0]);
            DbQueryCommandTree commandTree = objectContext.MetadataWorkspace.CreateQueryCommandTree(dbExpression);
            Type planCompilerType = entityFrameworkAssembly.GetType(
                "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler");
            MethodInfo compileMethod = planCompilerType.GetMethod(
                "Compile", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod);
            object[] arguments = new object[] { commandTree, null, null, null, null };
            compileMethod.Invoke(null, arguments);
            Type providerCommandInfoType = entityFrameworkAssembly.GetType(
                "System.Data.Entity.Core.Query.PlanCompiler.ProviderCommandInfo");
            PropertyInfo commandTreeProperty = providerCommandInfoType.GetProperty(
                "CommandTree", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
            object providerCommand = ((IEnumerable<object>)arguments[1]).Single();
            return (DbQueryCommandTree)commandTreeProperty.GetValue(providerCommand);
        }
    }

    public static partial class DbContextExtensions
    {
        public static DbCommand Generate(this IObjectContextAdapter context, DbQueryCommandTree commandTree)
        {
            MetadataWorkspace metadataWorkspace = context.ObjectContext.MetadataWorkspace;
            StoreItemCollection itemCollection = (StoreItemCollection)metadataWorkspace
                .GetItemCollection(DataSpace.SSpace);
            DbCommandDefinition commandDefinition = SqlProviderServices.Instance
                .CreateCommandDefinition(itemCollection.ProviderManifest, commandTree);
            return commandDefinition.CreateCommand();
            // SqlVersion sqlVersion = ((SqlProviderManifest)itemCollection.ProviderManifest).SqlVersion;
            // SqlGenerator sqlGenerator = new SqlGenerator(sqlVersion);
            // HashSet<string> paramsToForceNonUnicode;
            // string sql = sqlGenerator.GenerateSql(commandTree, out paramsToForceNonUnicode)
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
            DbProviderManifest providerManifest, DbCommandTree commandTree) => 
                (DbCommandDefinition)RedirectCall(providerManifest, commandTree.WriteLine());
    }

    public partial class LogProviderServices
    {
        public override void RegisterInfoMessageHandler(DbConnection connection, Action<string> handler) => 
            Sql.RegisterInfoMessageHandler(connection, handler);

        protected override DbCommand CloneDbCommand(DbCommand fromDbCommand) => 
            (DbCommand)RedirectCall(fromDbCommand);

        protected override void SetDbParameterValue(DbParameter parameter, TypeUsage parameterType, object value) => 
            RedirectCall(parameter, parameterType, value);

        protected override string GetDbProviderManifestToken(DbConnection connection) => 
            (string)RedirectCall(connection);

        protected override DbProviderManifest GetDbProviderManifest(string manifestToken) => 
            (DbProviderManifest)RedirectCall(manifestToken);

        protected override DbSpatialDataReader GetDbSpatialDataReader(DbDataReader fromReader, string versionHint) => 
            (DbSpatialDataReader)RedirectCall<DbDataReader, string>(fromReader, versionHint);

        protected override DbSpatialServices DbGetSpatialServices(string versionHint) => 
            (DbSpatialServices)RedirectCall(versionHint);

        protected override string DbCreateDatabaseScript(
            string providerManifestToken, StoreItemCollection storeItemCollection) => 
                (string)RedirectCall(providerManifestToken, storeItemCollection);

        protected override void DbCreateDatabase(
            DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection) => 
                RedirectCall(connection, commandTimeout, storeItemCollection);

        protected override bool DbDatabaseExists(
            DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection) => 
                (bool)RedirectCall(connection, commandTimeout, storeItemCollection);

        protected override bool DbDatabaseExists(
            DbConnection connection, int? commandTimeout, Lazy<StoreItemCollection> storeItemCollection) => 
                (bool)RedirectCall(connection, commandTimeout, storeItemCollection);

        protected override void DbDeleteDatabase(
            DbConnection connection, int? commandTimeout, StoreItemCollection storeItemCollection) => 
                RedirectCall(connection, commandTimeout, storeItemCollection);
    }
#else
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.Expressions;
    using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Microsoft.EntityFrameworkCore.Query.Sql;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.EntityFrameworkCore.Storage.Internal;
    using Microsoft.Extensions.DependencyInjection;

    using Remotion.Linq;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
    using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
    using Remotion.Linq.Parsing.Structure;
    using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
    using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;

    internal static partial class Translation
    {
        internal static void WhereAndSelect(WideWorldImporters adventureWorks)
        {
            // IQueryable<string> products = AdventureWorks.Products
            //    .Where(product => product.Name.StartsWith("M")).Select(product => product.Name);
            IQueryable<SupplierCategory> sourceQueryable = adventureWorks.SupplierCategories;
            IQueryable<SupplierCategory> whereQueryable = sourceQueryable.Where(category => category.SupplierCategoryName.StartsWith("A"));
            IQueryable<string> selectQueryable = whereQueryable.Select(category => category.SupplierCategoryName); // Define query.
            selectQueryable.WriteLines(); // Execute query.
        }

        internal static void WhereAndSelectExpressions(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> sourceQueryable = adventureWorks.SupplierCategories; // DbSet<ProductCategory>.
            ConstantExpression sourceConstantExpression = (ConstantExpression)sourceQueryable.Expression;
            sourceQueryable.Expression.WriteLine();
            // value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[ProductCategory])
            IQueryProvider sourceQueryProvider = sourceQueryable.Provider; // EntityQueryProvider.

            // Expression<Func<ProductCategory, bool>> predicateExpression = product => product.Name.StartsWith("M");
            ParameterExpression productParameterExpression = Expression.Parameter(typeof(SupplierCategory), "category");
            Func<string, bool> startsWithMethod = string.Empty.StartsWith;
            Expression<Func<SupplierCategory, bool>> predicateExpression = Expression.Lambda<Func<SupplierCategory, bool>>(
                Expression.Call(
                    instance: Expression.Property(productParameterExpression, nameof(SupplierCategory.SupplierCategoryName)),
                    method: startsWithMethod.GetMethodInfo(),
                    arguments: Expression.Constant("A", typeof(string))),
                productParameterExpression);
            predicateExpression.WriteLine(); // product => product.Name.StartsWith("M")

            // IQueryable<ProductCategory> whereQueryable = sourceQueryable.Where(predicateExpression);
            Func<IQueryable<SupplierCategory>, Expression<Func<SupplierCategory, bool>>, IQueryable<SupplierCategory>> whereMethod =
                Queryable.Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: whereMethod.GetMethodInfo(),
                arg0: sourceConstantExpression,
                arg1: Expression.Quote(predicateExpression));
            IQueryable<SupplierCategory> whereQueryable = sourceQueryProvider
                .CreateQuery<SupplierCategory>(whereCallExpression); // EntityQueryable<ProductCategory>.
            object.ReferenceEquals(whereCallExpression, whereQueryable.Expression).WriteLine(); // True
            whereQueryable.Expression.WriteLine();
            // value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[ProductCategory])
            //    .Where(product => product.Name.StartsWith("M"))
            IQueryProvider whereQueryProvider = whereQueryable.Provider; // EntityQueryProvider.

            // Expression<Func<ProductCategory, string>> selectorExpression = product => product.Name;
            Expression<Func<SupplierCategory, string>> selectorExpression = Expression.Lambda<Func<SupplierCategory, string>>(
                    Expression.Property(productParameterExpression, nameof(SupplierCategory.SupplierCategoryName)),
                    productParameterExpression);
            selectorExpression.WriteLine(); // product => product.Name

            // IQueryable<string> selectQueryable = whereQueryable.Select(selectorExpression);
            Func<IQueryable<SupplierCategory>, Expression<Func<SupplierCategory, string>>, IQueryable<string>> selectMethod =
                Queryable.Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: selectMethod.GetMethodInfo(),
                arg0: whereCallExpression,
                arg1: Expression.Quote(selectorExpression));
            IQueryable<string> selectQueryable = whereQueryProvider
                .CreateQuery<string>(selectCallExpression); // EntityQueryable<ProductCategory>.
            object.ReferenceEquals(selectCallExpression, selectQueryable.Expression).WriteLine(); // True
            selectQueryable.Expression.WriteLine();
            // value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[ProductCategory])
            //    .Where(product => product.Name.StartsWith("M"))
            //    .Select(product => product.Name)
            using (IEnumerator<string> iterator = selectQueryable.GetEnumerator())
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.WriteLine();
                }
            }
        }

        internal static SelectExpression WhereAndSelectCompiledExpressions(WideWorldImporters adventureWorks)
        {
            IQuerySqlGeneratorFactory sqlGeneratorFactory = adventureWorks.GetService<IQuerySqlGeneratorFactory>();
            IDatabaseProviderServices databaseProviderServices = adventureWorks.GetService<IDatabaseProviderServices>();
            QueryCompilationContext compilationContext = databaseProviderServices.QueryCompilationContextFactory.Create(async: false);
            SelectExpression selectExpression = new SelectExpression(sqlGeneratorFactory, (RelationalQueryCompilationContext)compilationContext);
            MainFromClause querySource = new MainFromClause("category", typeof(SupplierCategory), Expression.Constant(adventureWorks.SupplierCategories));
            TableExpression tableExpression = new TableExpression(nameof(SupplierCategory), WideWorldImporters.Purchasing, querySource.ItemName, querySource);
            selectExpression.AddTable(tableExpression);
            IEntityType categoryEntityType = adventureWorks.Model.FindEntityType(typeof(SupplierCategory));
            IProperty nameProperty = categoryEntityType.FindProperty(nameof(SupplierCategory.SupplierCategoryName));
            AliasExpression nameAlias = new AliasExpression(new ColumnExpression(nameof(SupplierCategory.SupplierCategoryName), nameProperty, tableExpression));
            selectExpression.AddToProjection(nameAlias);
            ConstantExpression patternExpression = Expression.Constant("A");
            selectExpression.Predicate = Expression.AndAlso(
                new LikeExpression(
                    nameAlias,
                    Expression.Add(patternExpression, Expression.Constant("%"), new Func<string, string, string>(string.Concat).GetMethodInfo())),
                Expression.Equal(
                    new SqlFunctionExpression("CHARINDEX", typeof(int), new Expression[] { patternExpression, nameAlias }),
                    Expression.Constant(1)));
            return selectExpression.WriteLine();
        }

        internal static void WhereAndSelectGenerateSql(WideWorldImporters adventureWorks)
        {
            SelectExpression selectExpression = WhereAndSelectCompiledExpressions(adventureWorks);
            IRelationalCommand command = adventureWorks.Generate(selectExpression);
            command.CommandText.WriteLine();
            // SELECT [category].[Name]
            // FROM [Production].[ProductCategory] AS [category]
            // WHERE [category].[Name] LIKE N'A' + N'%' AND (CHARINDEX(N'A', [category].[Name]) = 1)
        }

        internal static void SelectAndFirst(WideWorldImporters adventureWorks)
        {
            // string first = AdventureWorks.Products.Select(product => product.Name).First();
            IQueryable<SupplierCategory> sourceQueryable = adventureWorks.SupplierCategories;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.SupplierCategoryName);
            string first = selectQueryable.First().WriteLine();
        }

        internal static void SelectAndFirstExpressions(WideWorldImporters adventureWorks)
        {
            IQueryable<SupplierCategory> sourceQueryable = adventureWorks.SupplierCategories;
            sourceQueryable.Expression.WriteLine();
            // value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[ProductCategory])
            //    .MergeAs(AppendOnly)

            IQueryable<string> selectQueryable = sourceQueryable.Select(category => category.SupplierCategoryName);
            selectQueryable.Expression.WriteLine();
            // value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[ProductCategory])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            MethodCallExpression selectCallExpression = (MethodCallExpression)selectQueryable.Expression;
            IQueryProvider selectQueryProvider = selectQueryable.Provider; // DbQueryProvider.

            // string first = selectQueryable.First();
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(firstMethod.GetMethodInfo(), selectCallExpression);
            firstCallExpression.WriteLine();
            // value(Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable`1[ProductCategory])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            //    .First()
            string first = selectQueryProvider.Execute<string>(firstCallExpression).WriteLine(); // Execute query.
        }

        internal static SelectExpression SelectAndFirstCompiledExpressions(WideWorldImporters adventureWorks)
        {
            IQuerySqlGeneratorFactory sqlGeneratorFactory = adventureWorks.GetService<IQuerySqlGeneratorFactory>();
            IDatabaseProviderServices databaseProviderServices = adventureWorks.GetService<IDatabaseProviderServices>();
            QueryCompilationContext compilationContext = databaseProviderServices.QueryCompilationContextFactory.Create(async: false);
            SelectExpression selectExpression = new SelectExpression(sqlGeneratorFactory, (RelationalQueryCompilationContext)compilationContext);
            MainFromClause querySource = new MainFromClause("category", typeof(SupplierCategory), Expression.Constant(adventureWorks.SupplierCategories));
            TableExpression tableExpression = new TableExpression(nameof(SupplierCategory), WideWorldImporters.Purchasing, querySource.ItemName, querySource);
            selectExpression.AddTable(tableExpression);
            IEntityType categoryEntityType = adventureWorks.Model.FindEntityType(typeof(SupplierCategory));
            IProperty nameProperty = categoryEntityType.FindProperty(nameof(SupplierCategory.SupplierCategoryName));
            selectExpression.AddToProjection(new AliasExpression(new ColumnExpression(nameof(SupplierCategory.SupplierCategoryName), nameProperty, tableExpression)));
            selectExpression.Limit = Expression.Constant(1);
            return selectExpression.WriteLine();
        }

        internal static void SelectAndFirstGenerateSql(WideWorldImporters adventureWorks)
        {
            SelectExpression selectExpression = SelectAndFirstCompiledExpressions(adventureWorks);
            IRelationalCommand command = adventureWorks.Generate(selectExpression);
            command.CommandText.WriteLine();
            // SELECT TOP(1) [category].[Name]
            // FROM[Production].[ProductCategory] AS[category]
        }
    }

    public static partial class DbContextExtensions
    {
        internal sealed class NullEvaluatableExpressionFilter : EvaluatableExpressionFilterBase
        {
        }

        public static (SelectExpression, IReadOnlyDictionary<string, object>) Compile(
            this DbContext dbContext, Expression expression)
        {
            IDatabaseProviderServices databaseProviderServices = dbContext.GetService<IDatabaseProviderServices>();
            QueryCompilationContext compilationContext = databaseProviderServices.QueryCompilationContextFactory.Create(async: false);
            INodeTypeProvider nodeTypeProvider = dbContext.GetService<MethodInfoBasedNodeTypeRegistry>();
            IQueryContextFactory queryContextFactory = dbContext.GetService<IQueryContextFactory>();
            QueryContext queryContext = queryContextFactory.Create();
            ISensitiveDataLogger<QueryCompiler> logger = dbContext.GetService<ISensitiveDataLogger<QueryCompiler>>();
            expression = ParameterExtractingExpressionVisitor.ExtractParameters(expression, queryContext, new NullEvaluatableExpressionFilter(), logger);
            QueryParser queryParser = new QueryParser(new ExpressionTreeParser(
                nodeTypeProvider,
                new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())));
            QueryModel queryModel = queryParser.GetParsedQuery(expression);
            EntityQueryModelVisitor queryModelVisitor = compilationContext.CreateQueryModelVisitor();
            queryModelVisitor.CreateQueryExecutor<object>(queryModel);
            var selectExpression = ((SqlServerQueryModelVisitor)queryModelVisitor).TryGetQuery(queryModel.MainFromClause);
            selectExpression.QuerySource = queryModel.MainFromClause;
            return (selectExpression, queryContext.ParameterValues);
        }

        public static IRelationalCommand Generate(
            this DbContext dbContext, SelectExpression selectExpression, IReadOnlyDictionary<string, object> parameters = null)
        {
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            IQuerySqlGenerator sqlGenerator = sqlGeneratorFactory.CreateDefault(selectExpression);
            return sqlGenerator.GenerateSql(parameters ?? new Dictionary<string, object>());
        }

        public static Func<DbDataReader, TEntity> GetMaterializer<TEntity>(
            this DbContext dbContext, SelectExpression selectExpression, IReadOnlyDictionary<string, object> parameters = null)
        {
            IMaterializerFactory materializerFactory = dbContext.GetService<IMaterializerFactory>();
            IRelationalAnnotationProvider annotationProvider = dbContext.GetService<IRelationalAnnotationProvider>();
            Func<ValueBuffer, object> materializee = materializerFactory
                .CreateMaterializer(
                    dbContext.Model.FindEntityType(typeof(TEntity)),
                    selectExpression,
                    (property, expression) => expression.AddToProjection(
                        annotationProvider.For(property).ColumnName, property, selectExpression.QuerySource),
                    selectExpression.QuerySource)
                .Compile();
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            IQuerySqlGenerator sqlGenerator = sqlGeneratorFactory.CreateDefault(selectExpression);
            IRelationalValueBufferFactoryFactory valueBufferFactory = dbContext.GetService<IRelationalValueBufferFactoryFactory>();
            return dbReader => (TEntity)materializee(sqlGenerator.CreateValueBufferFactory(valueBufferFactory, dbReader).Create(dbReader));
        }
    }
#endif

    internal static partial class Translation
    {
        internal static void StringIsNullOrEmpty(WideWorldImporters adventureWorks)
        {
            IQueryable<string> products = adventureWorks.StockItems
                .Select(product => product.StockItemName)
                .Where(name => string.IsNullOrEmpty(name));
            adventureWorks.Compile(products.Expression).WriteLine();
        }

        private static bool FilterName(string name) => string.IsNullOrEmpty(name);

        internal static void RemoteMethodCall(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IQueryable<string> products = source
                .Select(product => product.StockItemName)
                .Where(name => FilterName(name)); // Define query.
            products.WriteLines(); // Execute query.
            // NotSupportedException: LINQ to Entities does not recognize the method 'Boolean FilterName(System.String)' method, and this method cannot be translated into a store expression.
        }

        internal static void LocalMethodCall(WideWorldImporters adventureWorks)
        {
            IQueryable<StockItem> source = adventureWorks.StockItems;
            IEnumerable<string> products = source
                .Select(product => product.StockItemName) // LINQ to Entities.
                .AsEnumerable() // LINQ to Objects.
                .Where(name => FilterName(name)); // Define query.
            products.WriteLines(); // Execute query.
        }

#if NETFX
        internal static void DbFunction(WideWorldImporters adventureWorks)
        {
            var suppliers = adventureWorks.Suppliers.Select(photo => new
            {
                Name = photo.SupplierName,
                UnupdatedDays = DbFunctions.DiffDays(photo.ValidFrom, DateTime.Now)
            });
            adventureWorks.Compile(suppliers.Expression).WriteLine();
        }

        internal static void SqlFunction(WideWorldImporters adventureWorks)
        {
            IQueryable<string> products = adventureWorks.StockItems
                .Select(product => product.StockItemName)
                .Where(name => SqlFunctions.PatIndex(name, "%o%a%") > 0);
            adventureWorks.Compile(products.Expression).WriteLine();
        }

        internal static void StringIsNullOrEmptySql(WideWorldImporters adventureWorks)
        {
            IQueryable<string> products = adventureWorks.StockItems
                .Select(product => product.StockItemName)
                .Where(name => string.IsNullOrEmpty(name));
            adventureWorks.Generate(adventureWorks.Compile(products.Expression)).CommandText.WriteLine();
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE (LEN([Extent1].[Name])) = 0
        }

        internal static void DbFunctionSql(WideWorldImporters adventureWorks)
        {
            var suppliers = adventureWorks.Suppliers.Select(photo => new
            {
                Name = photo.SupplierName,
                UnupdatedDays = DbFunctions.DiffDays(photo.ValidFrom, DateTime.Now)
            });
            adventureWorks.Generate(adventureWorks.Compile(suppliers.Expression).WriteLine()).CommandText.WriteLine();
            // SELECT 
            //    1 AS [C1], 
            //    [Extent1].[LargePhotoFileName] AS [LargePhotoFileName], 
            //    DATEDIFF (day, [Extent1].[ModifiedDate], SysDateTime()) AS [C2]
            //    FROM [Production].[ProductPhoto] AS [Extent1]
        }

        internal static void SqlFunctionSql(WideWorldImporters adventureWorks)
        {
            IQueryable<string> products = adventureWorks.StockItems
                .Select(product => product.StockItemName)
                .Where(name => SqlFunctions.PatIndex(name, "%o%a%") > 0);
            adventureWorks.Generate(adventureWorks.Compile(products.Expression).WriteLine()).CommandText.WriteLine();
            // SELECT
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE ( CAST(PATINDEX([Extent1].[Name], N'%o%a%') AS int)) > 0
        }
#endif
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
