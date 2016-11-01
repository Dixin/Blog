namespace Dixin.Linq.EntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.MappingViews;
    using System.Data.Entity.SqlServer;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public static partial class DbContextExtensions
    {
        public static DbQueryCommandTree Convert(this IObjectContextAdapter context, Expression expression)
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

    public static partial class DbContextExtensions
    {
        public static EntityContainer Container
            (this DbContext context) => ((IObjectContextAdapter)context)
                .ObjectContext
                .MetadataWorkspace
                .GetItemCollection(DataSpace.CSpace)
                .GetItems<EntityContainer>()
                .Single();

        public static ObjectContext ObjectContext
            (this DbContext context) => ((IObjectContextAdapter)context).ObjectContext;

        public static TDbConnection Connection<TDbConnection>(this DbContext context)
            where TDbConnection : DbConnection =>
                ((EntityConnection)context.ObjectContext().Connection)?.StoreConnection as TDbConnection;
    }

    public static partial class DbContextExtensions
    {
        public static IDictionary<EntitySetBase, DbMappingView> GeteMappingViews(
            this IObjectContextAdapter context, out IList<EdmSchemaError> errors)
        {
            StorageMappingItemCollection mappings = (StorageMappingItemCollection)context.ObjectContext
                .MetadataWorkspace.GetItemCollection(DataSpace.CSSpace);
            errors = new List<EdmSchemaError>();
            return mappings.GenerateViews(errors);
        }
    }
}
