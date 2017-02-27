namespace Tutorial.LinqToEntities
{
#if EF
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
        internal static void WhereAndSelect(AdventureWorks adventureWorks)
        {
            // IQueryable<string> products = AdventureWorks.Products
            //    .Where(product => product.Name.Length > 10).Select(product => product.Name);
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<Product> whereQueryable = sourceQueryable.Where(product => product.Name.Length > 10);
            IQueryable<string> selectQueryable = whereQueryable.Select(product => product.Name); // Define query.
            selectQueryable.WriteLines(); // Execute query.
        }
    }

    internal static partial class Translation
    {
        internal static void WhereAndSelectLinqExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products; // DbSet<Product>.

            // MethodCallExpression sourceMergeAsCallExpression = (MethodCallExpression)sourceQuery.Expression;
            ObjectQuery<Product> objectQuery = new ObjectQuery<Product>(
                $"[{nameof(AdventureWorks)}].[{nameof(AdventureWorks.Products)}]",
                ((IObjectContextAdapter)adventureWorks).ObjectContext,
                MergeOption.AppendOnly);
            MethodInfo mergeAsMethod = typeof(ObjectQuery<Product>)
                .GetTypeInfo().GetDeclaredMethods("MergeAs").Single();
            MethodCallExpression sourceMergeAsCallExpression = Expression.Call(
                instance: Expression.Constant(objectQuery),
                method: mergeAsMethod,
                arguments: Expression.Constant(MergeOption.AppendOnly, typeof(MergeOption)));
            sourceQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Product])
            //    .MergeAs(AppendOnly)
            IQueryProvider sourceQueryProvider = sourceQueryable.Provider; // DbQueryProvider.

            // Expression<Func<Product, bool>> predicateExpression = product => product.Name.Length > 10;
            ParameterExpression productParameterExpression = Expression.Parameter(typeof(Product), "product");
            Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                body: Expression.GreaterThan(
                    left: Expression.Property(
                        expression: Expression.Property(
                            expression: productParameterExpression, propertyName: nameof(Product.Name)), 
                        propertyName: nameof(string.Length)),
                    right: Expression.Constant(10)),
                parameters: productParameterExpression);

            // IQueryable<Product> whereQueryable = sourceQueryable.Where(predicateExpression);
            Func<IQueryable<Product>, Expression<Func<Product, bool>>, IQueryable<Product>> whereMethod =
                Queryable.Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: whereMethod.Method,
                arg0: sourceMergeAsCallExpression,
                arg1: Expression.Quote(predicateExpression));
            IQueryable<Product> whereQueryable = sourceQueryProvider
                .CreateQuery<Product>(whereCallExpression); // DbQuery<Product>.
            object.ReferenceEquals(whereCallExpression, whereQueryable.Expression).WriteLine(); // True.
            IQueryProvider whereQueryProvider = whereQueryable.Provider; // DbQueryProvider.

            // Expression<Func<Product, string>> selectorExpression = product => product.Name;
            Expression<Func<Product, string>> selectorExpression = Expression.Lambda<Func<Product, string>>(
                    Expression.Property(productParameterExpression, nameof(Product.Name)),
                    productParameterExpression);
            selectorExpression.WriteLine();
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
            
            selectQueryable.WriteLines(); // Execute query.
        }

        internal static void CompileWhereAndSelectExpressions(AdventureWorks adventureWorks)
        {
            Expression linqExpression = adventureWorks.Products
               .Where(product => product.Name.Length > 10).Select(product => product.Name).Expression;
            DbQueryCommandTree result = adventureWorks.Compile(linqExpression);
            result.WriteLine();
        }

        internal static DbQueryCommandTree WhereAndSelectDatabaseExpressions(AdventureWorks adventureWorks)
        {
            MetadataWorkspace metadata = ((IObjectContextAdapter)adventureWorks).ObjectContext.MetadataWorkspace;
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
                        DbExpressionBuilder.GreaterThan(
                            DbExpressionBuilder.Invoke(
                                ((IObjectContextAdapter)adventureWorks).ObjectContext.MetadataWorkspace
                                    .GetFunctions("LEN", "SqlServer", DataSpace.SSpace).First(),
                                DbExpressionBuilder.Property(
                                    DbExpressionBuilder.Variable(productTypeUsage, "Extent1"), nameof(Product.Name))),
                            DbExpressionBuilder.Constant(10))),
                    "Filter1"),
                DbExpressionBuilder.New(
                    nameRowTypeUsage,
                    DbExpressionBuilder.Property(
                        DbExpressionBuilder.Variable(productTypeUsage, "Filter1"), nameof(Product.Name))));
            DbQueryCommandTree result = new DbQueryCommandTree(metadata, DataSpace.SSpace, query);
            return result.WriteLine();
        }

        internal static void SelectAndFirst(AdventureWorks adventureWorks)
        {
            // string first = AdventureWorks.Products.Select(product => product.Name).First();
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            string first = selectQueryable.First().WriteLine();
        }

        internal static void SelectAndFirstLinqExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            sourceQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Product])
            //    .MergeAs(AppendOnly)

            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            selectQueryable.Expression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Product])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            MethodCallExpression selectCallExpression = (MethodCallExpression)selectQueryable.Expression;
            IQueryProvider selectQueryProvider = selectQueryable.Provider; // DbQueryProvider.

            // string first = selectQueryable.First();
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(firstMethod.Method, selectCallExpression);
            firstCallExpression.WriteLine();
            // value(System.Data.Entity.Core.Objects.ObjectQuery`1[Product])
            //    .MergeAs(AppendOnly)
            //    .Select(product => product.Name)
            //    .First()
            string first = selectQueryProvider.Execute<string>(firstCallExpression).WriteLine(); // Execute query.
        }

        internal static void CompileSelectAndFirstExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);

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

        internal static DbQueryCommandTree SelectAndFirstDatabaseExpressions(AdventureWorks adventureWorks)
        {
            MetadataWorkspace metadata = ((IObjectContextAdapter)adventureWorks).ObjectContext.MetadataWorkspace;
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
            return commandTree.WriteLine();
        }
    }

    public static partial class DbContextExtensions
    {
        public static DbQueryCommandTree Compile(this IObjectContextAdapter context, Expression linqExpression)
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
                new object[] { funcletizer, linqExpression });
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
        public static DbCommand Generate(this IObjectContextAdapter context, DbQueryCommandTree compilation)
        {
            MetadataWorkspace metadataWorkspace = context.ObjectContext.MetadataWorkspace;
            StoreItemCollection itemCollection = (StoreItemCollection)metadataWorkspace
                .GetItemCollection(DataSpace.SSpace);
            DbCommandDefinition commandDefinition = SqlProviderServices.Instance
                .CreateCommandDefinition(itemCollection.ProviderManifest, compilation);
            return commandDefinition.CreateCommand();
            // SqlVersion sqlVersion = ((SqlProviderManifest)itemCollection.ProviderManifest).SqlVersion;
            // SqlGenerator sqlGenerator = new SqlGenerator(sqlVersion);
            // string sql = sqlGenerator.GenerateSql(commandTree, HashSet<string> out paramsToForceNonUnicode)
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
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Metadata;
    using Microsoft.EntityFrameworkCore.Query;
    using Microsoft.EntityFrameworkCore.Query.Expressions;
    using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
    using Microsoft.EntityFrameworkCore.Query.Internal;
    using Microsoft.EntityFrameworkCore.Query.Sql;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.DependencyInjection;

    using Remotion.Linq;
    using Remotion.Linq.Clauses;
    using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
    using Remotion.Linq.Parsing.ExpressionVisitors.TreeEvaluation;
    using Remotion.Linq.Parsing.Structure;
    using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
    using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

    internal static partial class Translation
    {
        internal static void WhereAndSelect(AdventureWorks adventureWorks)
        {
            // IQueryable<string> products = adventureWorks.Products
            //    .Where(product => product.Name.Length > 10)
            //    .Select(product => product.Name);
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<Product> whereQueryable = sourceQueryable.Where(product => product.Name.Length > 10);
            IQueryable<string> selectQueryable = whereQueryable.Select(product => product.Name); // Define query.
            foreach (string result in selectQueryable) // Execute query.
            {
                result.WriteLine();
            }
        }
    }

    internal static partial class Translation
    {
        internal static void WhereAndSelectLinqExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products; // DbSet<Product>.
            ConstantExpression sourceConstantExpression = (ConstantExpression)sourceQueryable.Expression;
            IQueryProvider sourceQueryProvider = sourceQueryable.Provider; // EntityQueryProvider/DbQueryProvider.

            // Expression<Func<Product, bool>> predicateExpression = product => product.Name.Length > 10;
            ParameterExpression productParameterExpression = Expression.Parameter(typeof(Product), "product");
            Expression<Func<Product, bool>> predicateExpression = Expression.Lambda<Func<Product, bool>>(
                body: Expression.GreaterThan(
                    left: Expression.Property(
                        expression: Expression.Property(
                            expression: productParameterExpression, propertyName: nameof(Product.Name)),
                        propertyName: nameof(string.Length)),
                    right: Expression.Constant(10)),
                parameters: productParameterExpression);

            // IQueryable<Product> whereQueryable = sourceQueryable.Where(predicateExpression);
            Func<IQueryable<Product>, Expression<Func<Product, bool>>, IQueryable<Product>> whereMethod =
                Queryable.Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: whereMethod.GetMethodInfo(),
                arg0: sourceConstantExpression,
                arg1: Expression.Quote(predicateExpression));
            IQueryable<Product> whereQueryable = sourceQueryProvider
                .CreateQuery<Product>(whereCallExpression); // EntityQueryable<Product>/DbQuery<Product>.
            IQueryProvider whereQueryProvider = whereQueryable.Provider; // EntityQueryProvider/DbQueryProvider.

            // Expression<Func<Product, string>> selectorExpression = product => product.Name;
            Expression<Func<Product, string>> selectorExpression = Expression.Lambda<Func<Product, string>>(
                body: Expression.Property(productParameterExpression, nameof(Product.Name)),
                parameters: productParameterExpression);

            // IQueryable<string> selectQueryable = whereQueryable.Select(selectorExpression);
            Func<IQueryable<Product>, Expression<Func<Product, string>>, IQueryable<string>> selectMethod =
                Queryable.Select;
            MethodCallExpression selectCallExpression = Expression.Call(
                method: selectMethod.GetMethodInfo(),
                arg0: whereCallExpression,
                arg1: Expression.Quote(selectorExpression));
            IQueryable<string> selectQueryable = whereQueryProvider
                .CreateQuery<string>(selectCallExpression); // EntityQueryable<Product>/DbQuery<Product>.

            using (IEnumerator<string> iterator = selectQueryable.GetEnumerator()) // Execute query.
            {
                while (iterator.MoveNext())
                {
                    iterator.Current.WriteLine();
                }
            }
        }

        internal static SelectExpression WhereAndSelectDatabaseExpressions(AdventureWorks adventureWorks)
        {
            QueryCompilationContext compilationContext = adventureWorks.GetService<IDatabaseProviderServices>()
                .QueryCompilationContextFactory
                .Create(async: false);
            SelectExpression databaseExpression = new SelectExpression(
                querySqlGeneratorFactory: adventureWorks.GetService<IQuerySqlGeneratorFactory>(),
                queryCompilationContext: (RelationalQueryCompilationContext)compilationContext);
            MainFromClause querySource = new MainFromClause(
                itemName: "product",
                itemType: typeof(Product),
                fromExpression: Expression.Constant(adventureWorks.ProductCategories));
            TableExpression tableExpression = new TableExpression(
                table: nameof(Product),
                schema: AdventureWorks.Production,
                alias: querySource.ItemName,
                querySource: querySource);
            databaseExpression.AddTable(tableExpression);
            IEntityType productEntityType = adventureWorks.Model.FindEntityType(typeof(Product));
            IProperty nameProperty = productEntityType.FindProperty(nameof(Product.Name));
            AliasExpression nameAlias = new AliasExpression(new ColumnExpression(
                name: nameof(Product.Name), property: nameProperty, tableExpression: tableExpression));
            databaseExpression.AddToProjection(nameAlias);
            databaseExpression.Predicate = Expression.GreaterThan(
                left: new SqlFunctionExpression(
                    functionName: "LEN",
                    returnType: typeof(int),
                    arguments: new Expression[] { nameAlias }),
                right: Expression.Constant(10));
            return databaseExpression.WriteLine();
        }

        internal static void CompileWhereAndSelectExpressions(AdventureWorks adventureWorks)
        {
            Expression linqExpression = adventureWorks.Products
                .Where(product => product.Name.Length > 10)
                .Select(product => product.Name).Expression;
            (SelectExpression DatabaseExpression, IReadOnlyDictionary<string, object> Parameters) result =
                adventureWorks.Compile(linqExpression);
            result.DatabaseExpression.WriteLine();
            result.Parameters.WriteLines(parameter => $"{parameter.Key}: {parameter.Value}");
        }

        internal static void SelectAndFirst(AdventureWorks adventureWorks)
        {
            // string first = adventureWorks.Products.Select(product => product.Name).First();
            IQueryable<Product> sourceQueryable = adventureWorks.Products;
            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            string first = selectQueryable.First().WriteLine(); // Execute query.
        }

        internal static void SelectAndFirstLinqExpressions(AdventureWorks adventureWorks)
        {
            IQueryable<Product> sourceQueryable = adventureWorks.Products;

            IQueryable<string> selectQueryable = sourceQueryable.Select(product => product.Name);
            MethodCallExpression selectCallExpression = (MethodCallExpression)selectQueryable.Expression;
            IQueryProvider selectQueryProvider = selectQueryable.Provider; // DbQueryProvider.

            // string first = selectQueryable.First();
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: firstMethod.GetMethodInfo(), arg0: selectCallExpression);

            string first = selectQueryProvider.Execute<string>(firstCallExpression).WriteLine(); // Execute query.
        }

        internal static void CompileSelectAndFirstExpressions(AdventureWorks adventureWorks)
        {
            var selectQueryable = adventureWorks.Products
                .Select(product => product.Name).Expression;
            Func<IQueryable<string>, string> firstMethod = Queryable.First;
            MethodCallExpression linqExpression = Expression.Call(
                method: firstMethod.GetMethodInfo(), arg0: selectQueryable);
            (SelectExpression DatabaseExpression, IReadOnlyDictionary<string, object> Parameters) compilation =
                adventureWorks.Compile(linqExpression);
            compilation.DatabaseExpression.WriteLine();
            compilation.Parameters.WriteLines(parameter => $"{parameter.Key}: {parameter.Value}");
        }

        internal static SelectExpression SelectAndFirstDatabaseExpressions(AdventureWorks adventureWorks)
        {
            QueryCompilationContext compilationContext = adventureWorks.GetService<IDatabaseProviderServices>()
                .QueryCompilationContextFactory
                .Create(async: false);
            SelectExpression selectExpression = new SelectExpression(
                querySqlGeneratorFactory: adventureWorks.GetService<IQuerySqlGeneratorFactory>(),
                queryCompilationContext: (RelationalQueryCompilationContext)compilationContext);
            MainFromClause querySource = new MainFromClause(
                itemName: "product",
                itemType: typeof(Product),
                fromExpression: Expression.Constant(adventureWorks.ProductCategories));
            TableExpression tableExpression = new TableExpression(
                table: nameof(Product),
                schema: AdventureWorks.Production,
                alias: querySource.ItemName,
                querySource: querySource);
            selectExpression.AddTable(tableExpression);
            IEntityType productEntityType = adventureWorks.Model.FindEntityType(typeof(Product));
            IProperty nameProperty = productEntityType.FindProperty(nameof(Product.Name));
            selectExpression.AddToProjection(new AliasExpression(new ColumnExpression(
                name: nameof(Product.Name), property: nameProperty, tableExpression: tableExpression)));
            selectExpression.Limit = Expression.Constant(1);
            return selectExpression.WriteLine();
        }
    }

    public static partial class DbContextExtensions
    {
        public partial class ApiCompilationFilter : EvaluatableExpressionFilterBase { }

        public static (SelectExpression, IReadOnlyDictionary<string, object>) Compile(
            this DbContext dbContext, Expression linqExpression)
        {
            IDatabaseProviderServices databaseProviderServices = dbContext.GetService<IDatabaseProviderServices>();
            QueryCompilationContext compilationContext = databaseProviderServices.QueryCompilationContextFactory
                .Create(async: false);
            INodeTypeProvider nodeTypeProvider = dbContext.GetService<MethodInfoBasedNodeTypeRegistry>();
            IQueryContextFactory queryContextFactory = dbContext.GetService<IQueryContextFactory>();
            QueryContext queryContext = queryContextFactory.Create();
            ISensitiveDataLogger<QueryCompiler> logger = dbContext.GetService<ISensitiveDataLogger<QueryCompiler>>();
            linqExpression = ParameterExtractingExpressionVisitor.ExtractParameters(
                linqExpression, queryContext, new ApiCompilationFilter(), logger);
            QueryParser queryParser = new QueryParser(new ExpressionTreeParser(
                nodeTypeProvider: nodeTypeProvider,
                processor: new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                {
                    new PartialEvaluatingExpressionTreeProcessor(new ApiCompilationFilter()),
                    new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                })));
            QueryModel queryModel = queryParser.GetParsedQuery(linqExpression);

            RelationalQueryModelVisitor queryModelVisitor = (RelationalQueryModelVisitor)compilationContext
                .CreateQueryModelVisitor();
            Type resultType = queryModel.GetResultType();
            if (resultType.IsConstructedGenericType && resultType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            {
                resultType = resultType.GenericTypeArguments.Single();
            }
            queryModelVisitor.GetType().GetTypeInfo()
                .GetMethod(nameof(RelationalQueryModelVisitor.CreateQueryExecutor))
                .MakeGenericMethod(resultType)
                .Invoke(queryModelVisitor, new object[] { queryModel });
            SelectExpression databaseExpression = queryModelVisitor.TryGetQuery(queryModel.MainFromClause);
            databaseExpression.QuerySource = queryModel.MainFromClause;
            return (databaseExpression, queryContext.ParameterValues);
        }
    }

    public static partial class DbContextExtensions
    {
        public partial class ApiCompilationFilter : EvaluatableExpressionFilterBase
        {
            private static readonly PropertyInfo dateTimeUtcNow = typeof(DateTime).GetTypeInfo()
                .GetProperty(nameof(DateTime.UtcNow));

            public override bool IsEvaluatableMember(MemberExpression memberExpression) =>
                memberExpression.Member != dateTimeUtcNow;
        }
    }

    public static partial class DbContextExtensions
    {
        public static IRelationalCommand Generate(
            this DbContext dbContext,
            SelectExpression databaseExpression,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            IQuerySqlGenerator sqlGenerator = sqlGeneratorFactory.CreateDefault(databaseExpression);
            return sqlGenerator.GenerateSql(parameters ?? new Dictionary<string, object>());
        }

        public static IEnumerable<TResult> Materialize<TResult>(
            this DbContext dbContext,
            SelectExpression databaseExpression,
            IRelationalCommand sql,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            Func<DbDataReader, TResult> materializer = dbContext.GetMaterializer<TResult>(databaseExpression, parameters);
            using (RelationalDataReader reader = sql.ExecuteReader(
                connection: dbContext.GetService<IRelationalConnection>(), parameterValues: parameters))
            {
                while (reader.DbDataReader.Read())
                {
                    yield return materializer(reader.DbDataReader);
                }
            }
        }

        public static Func<DbDataReader, TResult> GetMaterializer<TResult>(
            this DbContext dbContext,
            SelectExpression databaseExpression,
            IReadOnlyDictionary<string, object> parameters = null)
        {
            IMaterializerFactory materializerFactory = dbContext.GetService<IMaterializerFactory>();
            IRelationalAnnotationProvider annotationProvider = dbContext.GetService<IRelationalAnnotationProvider>();
            Func<ValueBuffer, object> materializee = materializerFactory
                .CreateMaterializer(
                    entityType: dbContext.Model.FindEntityType(typeof(TResult)),
                    selectExpression: databaseExpression,
                    projectionAdder: (property, expression) => expression.AddToProjection(
                        annotationProvider.For(property).ColumnName, property, databaseExpression.QuerySource),
                    querySource: databaseExpression.QuerySource)
                .Compile();
            IQuerySqlGeneratorFactory sqlGeneratorFactory = dbContext.GetService<IQuerySqlGeneratorFactory>();
            IQuerySqlGenerator sqlGenerator = sqlGeneratorFactory.CreateDefault(databaseExpression);
            IRelationalValueBufferFactoryFactory valueBufferFactory = dbContext.GetService<IRelationalValueBufferFactoryFactory>();
            return dbReader => (TResult)materializee(sqlGenerator.CreateValueBufferFactory(valueBufferFactory, dbReader).Create(dbReader));
        }

        public static IEnumerable<TEntity> MaterializeEntity<TEntity>(
            this DbContext dbContext,
            IRelationalCommand sql,
            IReadOnlyDictionary<string, object> parameters = null)
            where TEntity : class
        {
            return dbContext.Set<TEntity>().FromSql(
                sql: sql.CommandText,
                parameters: parameters.Select(parameter => new SqlParameter(parameter.Key, parameter.Value)).ToArray());
        }
    }
#endif

    internal static partial class Translation
    {
        private static bool FilterName(string name) => name.Length > 10;

        internal static void WhereAndSelectWithCustomPredicate(AdventureWorks adventureWorks)
        {
            //var q = adventureWorks.Products.Skip(1).Take(2);
            //var compile = adventureWorks.Compile(q.Expression);
            //var sql = adventureWorks.Generate(compile.Item1, compile.Item2);
            //var r = adventureWorks.MaterializeEntity<Product>(sql, compile.Item2);
            IQueryable<Product> source = adventureWorks.Products;
            IQueryable<string> products = source
                .Where(product => FilterName(product.Name))
                .Select(product => product.Name); // Define query.
            products.WriteLines(); // Execute query.
#if EF
            // NotSupportedException: LINQ to Entities does not recognize the method 'Boolean FilterName(System.String)' method, and this method cannot be translated into a store expression.
#else
            // SELECT [product].[Name]
            // FROM [Production].[Product] AS [product]
#endif
        }

        internal static void WhereAndSelectWithLocalPredicate(AdventureWorks adventureWorks)
        {
            IQueryable<Product> source = adventureWorks.Products;
            IEnumerable<string> products = source
                .Select(product => product.Name) // LINQ to Entities.
                .AsEnumerable() // LINQ to Objects.
                .Where(name => FilterName(name)); // Define query, IEnumerable<string> instead of IQueryable<string>.
            products.WriteLines(); // Execute query.
        }

#if EF
        internal static void DbFunction(AdventureWorks adventureWorks)
        {
            var photos = adventureWorks.ProductPhotos.Select(photo => new
            {
                LargePhotoFileName = photo.LargePhotoFileName,
                UnmodifiedDays = DbFunctions.DiffDays(photo.ModifiedDate, DateTime.UtcNow)
            });
            adventureWorks.Compile(photos.Expression).WriteLine();
            photos.WriteLines();
            // SELECT 
            //    1 AS [C1], 
            //    [Extent1].[LargePhotoFileName] AS [LargePhotoFileName], 
            //    DATEDIFF (day, [Extent1].[ModifiedDate], SysUtcDateTime()) AS [C2]
            //    FROM [Production].[ProductPhoto] AS [Extent1]
        }

        internal static void SqlFunction(AdventureWorks adventureWorks)
        {
            IQueryable<string> products = adventureWorks.Products
                .Select(product => product.Name)
                .Where(name => SqlFunctions.PatIndex(name, "%Touring%50%") > 0); // Define query.
            products.WriteLines(); // Execute query.
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE ( CAST(PATINDEX([Extent1].[Name], N'%Touring%50%') AS int)) > 0
        }

        internal static void DbFunctionSql(AdventureWorks adventureWorks)
        {
            var photos = adventureWorks.ProductPhotos.Select(photo => new
            {
                LargePhotoFileName = photo.LargePhotoFileName,
                UnmodifiedDays = DbFunctions.DiffDays(photo.ModifiedDate, DateTime.Now)
            });
            adventureWorks.Generate(adventureWorks.Compile(photos.Expression).WriteLine()).CommandText.WriteLine();
            // SELECT 
            //    1 AS [C1], 
            //    [Extent1].[LargePhotoFileName] AS [LargePhotoFileName], 
            //    DATEDIFF (day, [Extent1].[ModifiedDate], SysDateTime()) AS [C2]
            //    FROM [Production].[ProductPhoto] AS [Extent1]
        }

        internal static void SqlFunctionSql(AdventureWorks adventureWorks)
        {
            IQueryable<string> products = adventureWorks.Products
                .Select(product => product.Name)
                .Where(name => SqlFunctions.PatIndex(name, "%o%a%") > 0);
            adventureWorks.Generate(adventureWorks.Compile(products.Expression).WriteLine()).CommandText.WriteLine();
            // SELECT
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE ( CAST(PATINDEX([Extent1].[Name], N'%o%a%') AS int)) > 0
        }
#endif

#if EF
        internal static void WhereAndSelectSql(AdventureWorks adventureWorks)
        {
            DbQueryCommandTree databaseExpressionAndParameters = WhereAndSelectDatabaseExpressions(adventureWorks);
            DbCommand sql = adventureWorks.Generate(databaseExpressionAndParameters);
            sql.CommandText.WriteLine();
            // SELECT 
            //    [Extent1].[Name] AS [Name]
            //    FROM [Production].[Product] AS [Extent1]
            //    WHERE [Extent1].[Name] LIKE N'M%'
        }
        
        internal static void SelectAndFirstSql(AdventureWorks adventureWorks)
        {
            DbQueryCommandTree databaseExpressionAndParameters = SelectAndFirstDatabaseExpressions(adventureWorks);
            DbCommand sql = adventureWorks.Generate(databaseExpressionAndParameters);
            sql.CommandText.WriteLine();
            // SELECT TOP (1) 
            //    [c].[Name] AS [Name]
            //    FROM [Production].[Product] AS [c]
        }
#else
        internal static void WhereAndSelectSql(AdventureWorks adventureWorks)
        {
            SelectExpression databaseExpression = WhereAndSelectDatabaseExpressions(adventureWorks);
            IRelationalCommand sql = adventureWorks.Generate(databaseExpression: databaseExpression, parameters: null);
            sql.CommandText.WriteLine();
            // SELECT [product].[Name]
            // FROM [Production].[ProductCategory] AS [product]
            // WHERE LEN([product].[Name]) > 10
        }

        internal static void SelectAndFirstSql(AdventureWorks adventureWorks)
        {
            SelectExpression databaseExpression = SelectAndFirstDatabaseExpressions(adventureWorks);
            IRelationalCommand sql = adventureWorks.Generate(databaseExpression: databaseExpression, parameters: null);
            sql.CommandText.WriteLine();
            // SELECT TOP(1) [product].[Name]
            // FROM [Production].[Product] AS [product]
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
namespace Microsoft.EntityFrameworkCore.Query.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    using Remotion.Linq.Clauses;

    public class SelectExpression : TableExpressionBase
    {
        public virtual IReadOnlyList<Expression> Projection { get; } // SELECT.

        public virtual bool IsDistinct { get; set; } // DISTINCT.

        public virtual Expression Limit { get; set; } // TOP.

        public virtual IReadOnlyList<TableExpressionBase> Tables { get; } // FROM.

        public virtual Expression Predicate { get; set; } // WHERE.

        public virtual IReadOnlyList<Ordering> OrderBy { get; } // ORDER BY.

        public virtual Expression Offset { get; set; } // OFFSET.

        public override Type Type { get; }
    }
}

namespace Microsoft.EntityFrameworkCore.Query.ExpressionTranslators.Internal
{
    using System.Linq.Expressions;

    using Microsoft.EntityFrameworkCore.Query.Expressions;

    public class SqlServerStringLengthTranslator : IMemberTranslator
    {
        public virtual Expression Translate(MemberExpression memberExpression) =>
            memberExpression.Expression != null
            && memberExpression.Expression.Type == typeof(string)
            && memberExpression.Member.Name == nameof(string.Length)
                ? new SqlFunctionExpression("LEN", memberExpression.Type, new Expression[] { memberExpression.Expression })
                : null;
    }
}

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    using System.Collections.Generic;

    using Microsoft.EntityFrameworkCore.Storage;

    public interface IQuerySqlGenerator
    {
        IRelationalCommand GenerateSql(IReadOnlyDictionary<string, object> parameterValues);

        // Other members.
    }
}

namespace Microsoft.EntityFrameworkCore.Storage
{
    using System.Collections.Generic;

    public interface IRelationalCommand
    {
        string CommandText { get; }

        IReadOnlyList<IRelationalParameter> Parameters { get; }

        RelationalDataReader ExecuteReader(
            IRelationalConnection connection, IReadOnlyDictionary<string, object> parameterValues);

        // Other members.
    }
}

namespace System.Data.Common
{
    public abstract class DbCommand : Component, IDbCommand, IDisposable
    {
        public abstract string CommandText { get; set; }

        public DbParameterCollection Parameters { get; }

        public DbDataReader ExecuteReader();
    }
}

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
    using System.Reflection;

    public static class Queryable
    {
        public static IQueryable<TSource> Where<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, IQueryable<TSource>> currentMethod =
                Where;
            MethodCallExpression whereCallExpression = Expression.Call(
                method: currentMethod.GetMethodInfo(),
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
                method: currentMethod.GetMethodInfo(),
                arg0: source.Expression,
                arg1: Expression.Quote(selector));
            return source.Provider.CreateQuery<TResult>(selectCallExpression);
        }

        public static TSource First<TSource>(
            this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            Func<IQueryable<TSource>, Expression<Func<TSource, bool>>, TSource> currentMethod = First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: currentMethod.GetMethodInfo(),
                arg0: source.Expression,
                arg1: Expression.Quote(predicate));
            return source.Provider.Execute<TSource>(firstCallExpression);
        }

        public static TSource First<TSource>(this IQueryable<TSource> source)
        {
            Func<IQueryable<TSource>, TSource> currentMethod = First;
            MethodCallExpression firstCallExpression = Expression.Call(
                method: currentMethod.GetMethodInfo(),
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
