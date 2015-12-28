namespace EntityFramework.Functions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Reflection;

    public static partial class Function
    {
        // The hard coded schema name "CodeFirstDatabaseSchema" is used by Entity Frameork.
        public const string CodeFirstDatabaseSchema = nameof(CodeFirstDatabaseSchema);

        public static void AddFunctions(this DbModel model, Type functionsType)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (functionsType == null)
            {
                throw new ArgumentNullException(nameof(functionsType));
            }

            functionsType
                .GetMethods(BindingFlags.Public | BindingFlags.InvokeMethod
                    | BindingFlags.Instance | BindingFlags.Static)
                .Select(methodInfo => new
                {
                    MethodInfo = methodInfo,
                    FunctionAttribute = methodInfo.GetCustomAttribute<FunctionAttribute>()
                })
                .Where(method => method.FunctionAttribute != null)
                .ForEach(method => model.AddFunction(method.MethodInfo, method.FunctionAttribute));
        }

        public static void AddFunction(
            this DbModel model,
            MethodInfo methodInfo,
            FunctionAttribute functionAttribute)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (functionAttribute == null)
            {
                throw new ArgumentNullException(nameof(functionAttribute));
            }

            /*
    <!-- SSDL content -->
    <edmx:StorageModels>
      <Schema Namespace="CodeFirstDatabaseSchema" Provider="System.Data.SqlClient" ProviderManifestToken="2012" Alias="Self" xmlns:store="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator" xmlns:customannotation="http://schemas.microsoft.com/ado/2013/11/edm/customannotation" xmlns="http://schemas.microsoft.com/ado/2009/11/edm/ssdl">
        <Function Name="ufnGetContactInformation" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="true" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo">
          <Parameter Name="PersonID" Type="int" Mode="In" />
          <ReturnType>
            <CollectionType>
              <RowType>
                <Property Name="PersonID" Type="int" Nullable="false" />
                <Property Name="FirstName" Type="nvarchar" MaxLength="50" />
                <Property Name="LastName" Type="nvarchar" MaxLength="50" />
                <Property Name="JobTitle" Type="nvarchar" MaxLength="50" />
                <Property Name="BusinessEntityType" Type="nvarchar" MaxLength="50" />
              </RowType>
            </CollectionType>
          </ReturnType>
        </Function>
        <Function Name="ufnGetProductListPrice" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="true" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo" ReturnType="money">
          <Parameter Name="ProductID" Type="int" Mode="In" />
          <Parameter Name="OrderDate" Type="datetime" Mode="In" />
        </Function>
        <Function Name="ufnGetProductStandardCost" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="false" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo">
          <Parameter Name="ProductID" Type="int" Mode="In" />
          <Parameter Name="OrderDate" Type="datetime" Mode="In" />
          <CommandText>
            SELECT [dbo].[ufnGetProductListPrice](@ProductID, @OrderDate)
          </CommandText>
        </Function>
        <Function Name="uspGetCategoryAndSubCategory" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="false" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo">
          <Parameter Name="CategoryID" Type="int" Mode="In" />
        </Function>
        <Function Name="uspGetManagerEmployees" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="false" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo">
          <Parameter Name="BusinessEntityID" Type="int" Mode="In" />
        </Function>
        <EntityContainer Name="CodeFirstDatabase">
        </EntityContainer>
      </Schema>
    </edmx:StorageModels>
            */
            // Build above <StorageModels> imperatively.
            string functionName = functionAttribute.FunctionName;
            if (string.IsNullOrWhiteSpace(functionName))
            {
                functionName = methodInfo.Name;
            }

            EdmFunction storeFunction = EdmFunction.Create(
                functionName,
                CodeFirstDatabaseSchema, // model.StoreModel.Container.Name is "CodeFirstDatabase".
                DataSpace.SSpace, // <edmx:StorageModels>
                new EdmFunctionPayload()
                {
                    Schema = functionAttribute.Schema,
                    IsAggregate = functionAttribute.IsAggregate,
                    IsBuiltIn = functionAttribute.IsBuiltIn,
                    IsNiladic = functionAttribute.IsNiladic,
                    IsComposable = functionAttribute.IsComposable,
                    ParameterTypeSemantics = functionAttribute.ParameterTypeSemantics,
                    Parameters = model.GetStoreParameters(methodInfo, functionAttribute),
                    ReturnParameters = model.GetStoreReturnParameters(methodInfo, functionAttribute),
                    CommandText = methodInfo.GetStoreCommandText(functionAttribute, functionName)
                },
                null);
            model.StoreModel.AddItem(storeFunction);

            switch (functionAttribute.Type)
            {
                // Aggregate/Built in/Niladic/Composable scalar-valued function has no <FunctionImport> or <FunctionImportMapping>.
                case FunctionType.ComposableScalarValuedFunction:
                case FunctionType.AggregateFunction:
                case FunctionType.BuiltInFunction:
                case FunctionType.NiladicFunction:
                    return;
            }

            /*
    <!-- CSDL content -->
    <edmx:ConceptualModels>
      <Schema Namespace="AdventureWorks" Alias="Self" annotation:UseStrongSpatialTypes="false" xmlns:annotation="http://schemas.microsoft.com/ado/2009/02/edm/annotation" xmlns:customannotation="http://schemas.microsoft.com/ado/2013/11/edm/customannotation" xmlns="http://schemas.microsoft.com/ado/2009/11/edm">
        <EntityContainer Name="AdventureWorks" annotation:LazyLoadingEnabled="true">
          <FunctionImport Name="ufnGetContactInformation" IsComposable="true" ReturnType="Collection(AdventureWorks.ContactInformation)">
            <Parameter Name="PersonID" Mode="In" Type="Int32" />
          </FunctionImport>
          <FunctionImport Name="uspGetCategoryAndSubCategory" ReturnType="Collection(AdventureWorks.CategoryAndSubCategory)">
            <Parameter Name="CategoryID" Mode="In" Type="Int32" />
          </FunctionImport>
          <FunctionImport Name="uspGetManagerEmployees" ReturnType="Collection(AdventureWorks.ManagerEmployee)">
            <Parameter Name="BusinessEntityID" Mode="In" Type="Int32" />
          </FunctionImport>
          <FunctionImport Name="ufnGetProductStandardCost" ReturnType="Collection(Decimal)">
            <Parameter Name="ProductID" Mode="In" Type="Int32" />
            <Parameter Name="OrderDate" Mode="In" Type="DateTime" />
          </FunctionImport>
        </EntityContainer>
      </Schema>
    </edmx:ConceptualModels>
            */
            // Build above <ConceptualModels> imperatively.
            EdmFunction modelFunction = EdmFunction.Create(
                storeFunction.Name,
                model.ConceptualModel.Container.Name,
                DataSpace.CSpace, // <edmx:ConceptualModels>
                new EdmFunctionPayload
                {
                    IsFunctionImport = true,
                    IsComposable = storeFunction.IsComposableAttribute,
                    Parameters = model.GetModelParameters(methodInfo, storeFunction),
                    ReturnParameters = model.GetModelReturnParameters(methodInfo, functionAttribute),
                    EntitySets = model.GetModelEntitySets(methodInfo, functionAttribute)
                },
                null);
            model.ConceptualModel.Container.AddFunctionImport(modelFunction);

            /*
    <!-- C-S mapping content -->
    <edmx:Mappings>
      <Mapping Space="C-S" xmlns="http://schemas.microsoft.com/ado/2009/11/mapping/cs">
        <EntityContainerMapping StorageEntityContainer="CodeFirstDatabase" CdmEntityContainer="AdventureWorks">
          <FunctionImportMapping FunctionImportName="ufnGetContactInformation" FunctionName="AdventureWorks.ufnGetContactInformation">
            <ResultMapping>
              <ComplexTypeMapping TypeName="AdventureWorks.ContactInformation">
                <ScalarProperty Name="PersonID" ColumnName="PersonID" />
                <ScalarProperty Name="FirstName" ColumnName="FirstName" />
                <ScalarProperty Name="LastName" ColumnName="LastName" />
                <ScalarProperty Name="JobTitle" ColumnName="JobTitle" />
                <ScalarProperty Name="BusinessEntityType" ColumnName="BusinessEntityType" />
              </ComplexTypeMapping>
            </ResultMapping>
          </FunctionImportMapping>
          <FunctionImportMapping FunctionImportName="uspGetCategoryAndSubCategory" FunctionName="AdventureWorks.uspGetCategoryAndSubCategory">
            <ResultMapping>
              <ComplexTypeMapping TypeName="AdventureWorks.CategoryAndSubCategory">
                <ScalarProperty Name="ProductCategoryID" ColumnName="ProductCategoryID" />
                <ScalarProperty Name="Name" ColumnName="Name" />
              </ComplexTypeMapping>
            </ResultMapping>
          </FunctionImportMapping>
          <FunctionImportMapping FunctionImportName="uspGetManagerEmployees" FunctionName="AdventureWorks.uspGetManagerEmployees">
            <ResultMapping>
              <ComplexTypeMapping TypeName="AdventureWorks.ManagerEmployee">
                <ScalarProperty Name="RecursionLevel" ColumnName="RecursionLevel" />
                <ScalarProperty Name="OrganizationNode" ColumnName="OrganizationNode" />
                <ScalarProperty Name="ManagerFirstName" ColumnName="ManagerFirstName" />
                <ScalarProperty Name="ManagerLastName" ColumnName="ManagerLastName" />
                <ScalarProperty Name="BusinessEntityID" ColumnName="BusinessEntityID" />
                <ScalarProperty Name="FirstName" ColumnName="FirstName" />
                <ScalarProperty Name="LastName" ColumnName="LastName" />
              </ComplexTypeMapping>
            </ResultMapping>
          </FunctionImportMapping>
          <FunctionImportMapping FunctionImportName="ufnGetProductStandardCost" FunctionName="AdventureWorks.ufnGetProductStandardCost" />
        </EntityContainerMapping>
      </Mapping>
    </edmx:Mappings>
            */
            // Build above <Mappings> imperatively.
            FunctionImportMapping mapping;
            if (modelFunction.IsComposableAttribute)
            {
                mapping = new FunctionImportMappingComposable(
                    modelFunction,
                    storeFunction,
                    new FunctionImportResultMapping(),
                    model.ConceptualToStoreMapping);
            }
            else
            {
                mapping = new FunctionImportMappingNonComposable(
                    modelFunction,
                    storeFunction,
                    Enumerable.Empty<FunctionImportResultMapping>(),
                    model.ConceptualToStoreMapping);
            }

            model.ConceptualToStoreMapping.AddFunctionImportMapping(mapping);
        }

        private static IList<FunctionParameter> GetStoreParameters
            (this DbModel model, MethodInfo methodInfo, FunctionAttribute functionAttribute) => methodInfo
                .GetParameters()
                .Select((parameterInfo, index) =>
                    {
                        string parameterName = parameterInfo.GetCustomAttribute<ParameterAttribute>()?.Name;
                        if (string.IsNullOrWhiteSpace(parameterName))
                        {
                            parameterName = parameterInfo.Name;
                        }

                        switch (functionAttribute.Type)
                        {
                            case FunctionType.NiladicFunction:
                                throw new NotSupportedException(
                                    $"Parameter of method {methodInfo.Name} is not supporteds.");

                            case FunctionType.AggregateFunction:
                                {
                                    if (index == 0)
                                    {
                                        return FunctionParameter.Create(
                                            parameterName,
                                            model.GetStoreParameterPrimitiveType(
                                                    methodInfo, parameterInfo, functionAttribute)
                                                .GetCollectionType(), // Must be collection type.
                                            ParameterMode.In);
                                    }

                                    // Aggregate function with more than more parameter is not supported by entity framework.
                                    throw new NotSupportedException(
                                        $"Method {methodInfo.Name} has more than one parameters and is not supported by Entity Framework.");
                                }
                        }

                        return FunctionParameter.Create(
                            parameterName,
                            model.GetStoreParameterPrimitiveType(methodInfo, parameterInfo, functionAttribute),
                            parameterInfo.ParameterType == typeof(ObjectParameter)
                                ? ParameterMode.InOut
                                : ParameterMode.In);
                    })
                .ToArray();

        private static PrimitiveType GetStoreParameterPrimitiveType(
            this DbModel model, MethodInfo methodInfo, ParameterInfo parameterInfo, FunctionAttribute functionAttribute)
        {
            // <Parameter Name="PersonID" Type="int" Mode="In" />
            Type parameterClrType = parameterInfo.ParameterType;
            ParameterAttribute parameterAttribute = parameterInfo.GetCustomAttribute<ParameterAttribute>();
            Type parameterAttributeClrType = parameterAttribute?.ClrType;

            if (parameterClrType.IsGenericType)
            {
                Type parameterClrTypeDefinition = parameterClrType.GetGenericTypeDefinition();
                if (parameterClrTypeDefinition == typeof(IEnumerable<>)
                    || parameterClrTypeDefinition == typeof(IQueryable<>))
                {
                    if (functionAttribute.Type == FunctionType.AggregateFunction)
                    {
                        // Aggregate function has one IEnumerable<T> or IQueryable<T> parameter. 
                        parameterClrType = parameterClrType.GetGenericArguments().Single();
                    }
                    else
                    {
                        throw new NotSupportedException(
                            $"Parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported. {typeof(IEnumerable<>).FullName} parameter must be used for {nameof(FunctionType)}.{nameof(FunctionType.AggregateFunction)} method.");
                    }
                }
            }

            if (parameterClrType == typeof(ObjectParameter))
            {
                // ObjectParameter must be used for stored procedure parameter.
                if (functionAttribute.Type != FunctionType.StoredProcedure)
                {
                    throw new NotSupportedException(
                        $"Parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported. {nameof(ObjectParameter)} parameter must be used for {nameof(FunctionType)}.{nameof(FunctionType.StoredProcedure)} method.");
                }

                // ObjectParameter.Type is available only when methodInfo is called. 
                // When building model, its store type/clr type must be provided by ParameterAttribute.
                if (parameterAttributeClrType == null)
                {
                    throw new NotSupportedException(
                        $"Parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported. {nameof(ObjectParameter)} parameter must have {nameof(ParameterAttribute)} with {nameof(ParameterAttribute.ClrType)} specified, with optional {nameof(ParameterAttribute.DbType)}.");
                }

                parameterClrType = parameterAttributeClrType;
            }
            else
            {
                // When parameter is not ObjectParameter, ParameterAttribute.ClrType should be either not specified, or the same as parameterClrType.
                if (parameterAttributeClrType != null && parameterAttributeClrType != parameterClrType)
                {
                    throw new NotSupportedException(
                        $"Parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported. It is of {parameterClrType.FullName} type, but its {nameof(ParameterAttribute)}.{nameof(ParameterAttribute.ClrType)} has a fifferent type {parameterAttributeClrType.FullName}");
                }
            }

            string storePrimitiveTypeName = parameterAttribute?.DbType;
            return !string.IsNullOrEmpty(storePrimitiveTypeName)
                ? model.GetStorePrimitiveType(storePrimitiveTypeName, methodInfo, parameterInfo)
                : model.GetStorePrimitiveType(parameterClrType, methodInfo, parameterInfo);
        }

        private static PrimitiveType GetStorePrimitiveType(
            this DbModel model, string storeEdmTypeName, MethodInfo methodInfo, ParameterInfo parameterInfo)
        {
            // targetStoreEdmType = model.ProviderManifest.GetStoreType(TypeUsage.CreateDefaultTypeUsage(primitiveEdmType)).EdmType;
            PrimitiveType storePrimitiveType = model
                .ProviderManifest
                .GetStoreTypes()
                .FirstOrDefault(primitiveType => primitiveType.Name.EqualsOrdinal(storeEdmTypeName));
            if (storePrimitiveType == null)
            {
                throw new NotSupportedException(
                    $"The specified {nameof(ParameterAttribute)}.{nameof(ParameterAttribute.DbType)} '{storeEdmTypeName}' for parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported in database.");
            }

            return storePrimitiveType;
        }

        private static PrimitiveType GetStorePrimitiveType(
            this DbModel model, Type clrType, MethodInfo methodInfo, ParameterInfo parameterInfo)
        {
            // targetStoreEdmType = model.ProviderManifest.GetStoreType(TypeUsage.CreateDefaultTypeUsage(primitiveEdmType)).EdmType;
            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                clrType = clrType.GetGenericArguments().Single();
            }

            PrimitiveType storePrimitiveType = model
                .ProviderManifest
                .GetStoreTypes()
                .FirstOrDefault(primitiveType => primitiveType.ClrEquivalentType == clrType);
            if (storePrimitiveType == null)
            {
                throw new NotSupportedException(
                    $"The specified type {clrType.FullName} for parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported in database.");
            }

            return storePrimitiveType;
        }

        private static IList<FunctionParameter> GetStoreReturnParameters(
            this DbModel model, MethodInfo methodInfo, FunctionAttribute functionAttribute)
        {
            ParameterInfo returnParameterInfo = methodInfo.ReturnParameter;
            if (returnParameterInfo == null || returnParameterInfo.ParameterType == typeof(void))
            {
                throw new NotSupportedException($"The return type of {methodInfo.Name} is not supported.");
            }

            ParameterAttribute returnParameterAttribute = returnParameterInfo.GetCustomAttribute<ParameterAttribute>();
            ResultTypeAttribute[] returnTypeAttributes = methodInfo.GetCustomAttributes<ResultTypeAttribute>().ToArray();

            if (functionAttribute.Type == FunctionType.StoredProcedure)
            {
                if (returnParameterAttribute != null)
                {
                    throw new NotSupportedException(
                        $"{nameof(ParameterAttribute)} for return value of method {methodInfo.Name} is not supported.");
                }

                return new FunctionParameter[0];
            }

            if (returnTypeAttributes.Any())
            {
                throw new NotSupportedException($"{nameof(ResultTypeAttribute)} for method {methodInfo.Name} is not supported.");
            }

            if (functionAttribute.Type == FunctionType.TableValuedFunction)
            {
                if (returnParameterAttribute != null)
                {
                    throw new NotSupportedException(
                        $"{nameof(ParameterAttribute)} for return value of method {methodInfo.Name} is not supported.");
                }

                /*
        <CollectionType>
          <RowType>
            <Property Name="PersonID" Type="int" Nullable="false" />
            <Property Name="FirstName" Type="nvarchar" MaxLength="50" />
            <Property Name="LastName" Type="nvarchar" MaxLength="50" />
            <Property Name="JobTitle" Type="nvarchar" MaxLength="50" />
            <Property Name="BusinessEntityType" Type="nvarchar" MaxLength="50" />
          </RowType>
        </CollectionType>
                */
                // returnParameterInfo.ParameterType is IQueryable<T>.
                Type storeReturnParameterClrType = returnParameterInfo.ParameterType.GetGenericArguments().Single();
                StructuralType modelReturnParameterStructuralType = model.GetModelStructualType(
                    storeReturnParameterClrType, methodInfo);
                RowType storeReturnParameterRowType = RowType.Create(
                    modelReturnParameterStructuralType
                        .Members
                        .Select(member => EdmProperty.Create(
                            member.Name, model.ProviderManifest.GetStoreType(member.TypeUsage))),
                    null); // Collection of RowType.
                return new FunctionParameter[]
                    {
                        FunctionParameter.Create(
                            "ReturnType",
                            storeReturnParameterRowType.GetCollectionType(),
                            ParameterMode.ReturnValue)
                    };
            }

            if (functionAttribute.Type == FunctionType.NonComposableScalarValuedFunction)
            {
                // Non-composable scalar-valued function.
                return new FunctionParameter[0];
            }

            // Composable scalar-valued/Aggregate/Built in/Niladic function.
            // <Function Name="ufnGetProductListPrice" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="true" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo" 
            //    ReturnType ="money">
            PrimitiveType storeReturnParameterPrimitiveType = model.GetStoreParameterPrimitiveType(
                    methodInfo, returnParameterInfo, functionAttribute);
            return new FunctionParameter[]
                {
                    FunctionParameter.Create(
                        "ReturnType", storeReturnParameterPrimitiveType, ParameterMode.ReturnValue)
                };
        }

        private static IList<FunctionParameter> GetModelParameters(
            this DbModel model, MethodInfo methodInfo, EdmFunction storeFunction)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters().ToArray();
            return storeFunction
                .Parameters
                .Select((storeParameter, index) =>
                    {
                        ParameterInfo parameterInfo = parameters[index];
                        return FunctionParameter.Create(
                            parameterInfo.GetCustomAttribute<ParameterAttribute>()?.Name ?? parameterInfo.Name,
                            model.GetModelParameterPrimitiveType(methodInfo, parameterInfo),
                            storeParameter.Mode);
                    })
                .ToArray();
        }

        private static IList<FunctionParameter> GetModelReturnParameters(
            this DbModel model, MethodInfo methodInfo, FunctionAttribute functionAttribute)
        {
            ParameterInfo returnParameterInfo = methodInfo.ReturnParameter;
            if (returnParameterInfo == null || returnParameterInfo.ParameterType == typeof(void))
            {
                throw new NotSupportedException($"The return parameter type of {methodInfo.Name} is not supported.");
            }

            ParameterAttribute returnParameterAttribute = returnParameterInfo.GetCustomAttribute<ParameterAttribute>();
            ResultTypeAttribute[] returnTypeAttributes = methodInfo.GetCustomAttributes<ResultTypeAttribute>().ToArray();
            IEnumerable<EdmType> modelReturnParameterEdmTypes;
            if (functionAttribute.Type == FunctionType.StoredProcedure)
            {
                if (returnParameterAttribute != null)
                {
                    throw new NotSupportedException(
                        $"{nameof(ParameterAttribute)} for method {methodInfo.Name} is not supported.");
                }

                modelReturnParameterEdmTypes = methodInfo
                    .GetStoredProcedureReturnTypes()
                    .Select(clrType => model.GetModelStructualType(clrType, methodInfo));
            }
            else
            {
                if (returnTypeAttributes.Any())
                {
                    throw new NotSupportedException(
                        $"{nameof(ResultTypeAttribute)} for method {methodInfo.Name} is not supported.");
                }

                if (functionAttribute.Type == FunctionType.TableValuedFunction)
                {
                    // returnParameterInfo.ParameterType is IQueryable<T>.
                    Type returnParameterClrType = returnParameterInfo.ParameterType.GetGenericArguments().Single();
                    StructuralType modelReturnParameterStructuralType = model.GetModelStructualType(
                        returnParameterClrType, methodInfo);
                    modelReturnParameterEdmTypes = Enumerable.Repeat(modelReturnParameterStructuralType, 1);
                }
                else
                {
                    Type returnParameterClrType = returnParameterInfo.ParameterType;
                    Type returnParameterAttributeClrType = returnParameterAttribute?.ClrType;
                    if (returnParameterAttributeClrType != null
                        && returnParameterAttributeClrType != returnParameterClrType)
                    {
                        throw new NotSupportedException(
                            $"Return parameter of method {methodInfo.Name} is of {returnParameterClrType.FullName}, but its {nameof(ParameterAttribute)}.{nameof(ParameterAttribute.ClrType)} has a fifferent type {returnParameterAttributeClrType.FullName}");
                    }

                    PrimitiveType returnParameterPrimitiveType = model.GetModelPrimitiveType(
                        returnParameterClrType, methodInfo);
                    modelReturnParameterEdmTypes = Enumerable.Repeat(returnParameterPrimitiveType, 1);
                }
            }

            return modelReturnParameterEdmTypes
                .Select((edmType, index) => FunctionParameter.Create(
                    $"ReturnType{index}",
                    edmType.GetCollectionType(),
                    ParameterMode.ReturnValue))
                .ToArray();
        }

        private static PrimitiveType GetModelParameterPrimitiveType(
            this DbModel model, MethodInfo methodInfo, ParameterInfo parameterInfo)
        {
            // <Parameter Name="PersonID" Mode="In" Type="Int32" />
            Type parameterClrType = parameterInfo.ParameterType;
            ParameterAttribute parameterAttribute = parameterInfo.GetCustomAttribute<ParameterAttribute>();
            Type parameterAttributeClrType = parameterAttribute?.ClrType;
            if (parameterClrType == typeof(ObjectParameter))
            {
                // ObjectParameter.Type is available only when methodInfo is called.
                // When building model, its store type/clr type must be provided by ParameterAttribute.
                if (parameterAttributeClrType == null)
                {
                    throw new NotSupportedException(
                        $"Parameter {parameterInfo.Name} of method {methodInfo.Name} is not supported. {nameof(ObjectParameter)} parameter must have {nameof(ParameterAttribute)} with {nameof(ParameterAttribute.ClrType)} specified.");
                }

                parameterClrType = parameterAttributeClrType;
            }
            else
            {
                // When parameter is not ObjectParameter, ParameterAttribute.ClrType should be the same as parameterClrType, or not specified.
                if (parameterAttributeClrType != null && parameterAttributeClrType != parameterClrType)
                {
                    throw new NotSupportedException(
                        $"Parameter {parameterInfo.Name} of method {methodInfo.Name} if of {parameterClrType.FullName}, but its {nameof(ParameterAttribute)}.{nameof(ParameterAttribute.ClrType)} has a fifferent type {parameterAttributeClrType.FullName}");
                }
            }

            return model.GetModelPrimitiveType(parameterClrType, methodInfo);
        }

        private static PrimitiveType GetModelPrimitiveType(this DbModel model, Type clrType, MethodInfo methodInfo)
        {
            // Parameter and return parameter can be Nullable<T>.
            // Return parameter can be IQueryable<T>, ObjectResult<T>.
            if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type genericTypeDefinition = clrType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>)
                    || genericTypeDefinition == typeof(IQueryable<>)
                    || genericTypeDefinition == typeof(ObjectResult<>))
                {
                    clrType = clrType.GetGenericArguments().Single(); // Gets T from Nullable<T>.
                }
            }

            if (clrType.IsEnum)
            {
                EnumType modelEnumType = model
                    .ConceptualModel
                    .EnumTypes
                    .FirstOrDefault(enumType => enumType.FullName.EqualsOrdinal(clrType.FullName));
                if (modelEnumType == null)
                {
                    throw new NotSupportedException(
                        $"Enum type {nameof(clrType.FullName)} in method {methodInfo.Name} is not supported in conceptual model.");
                }

                return modelEnumType.UnderlyingType;
            }

            // clrType is not enum.
            PrimitiveType modelPrimitiveType = PrimitiveType
                .GetEdmPrimitiveTypes()
                .FirstOrDefault(primitiveType => primitiveType.ClrEquivalentType == clrType);
            if (modelPrimitiveType == null)
            {
                throw new NotSupportedException(
                    $"Type {nameof(clrType.FullName)} in method {methodInfo.Name} is not supported in conceptual model.");
            }

            return modelPrimitiveType;
        }

        private static StructuralType GetModelStructualType(
            this DbModel model, Type clrType, MethodInfo methodInfo)
        {
            StructuralType modelStructualType = model
                .ConceptualModel
                .EntityTypes
                .OfType<StructuralType>()
                .Concat(model.ConceptualModel.ComplexTypes)
                .FirstOrDefault(structuralType => structuralType.FullName.EqualsOrdinal(clrType.FullName));
            if (modelStructualType == null)
            {
                // Cannot add missing complex type instantly. The following code does not work.
                // if (Attribute.IsDefined(clrType, typeof(ComplexTypeAttribute)))
                // {
                //    MethodInfo complexTypeMethod = typeof(DbModelBuilder).GetMethod(nameof(modelBuilder.ComplexType));
                //    complexTypeMethod.MakeGenericMethod(clrType).Invoke(modelBuilder, null);
                //    model.Compile();
                //    modelStructualType = model
                //        .ConceptualModel
                //        .ComplexTypes
                //        .FirstOrDefault(complexType=> complexType.FullName.EqualsOrdinal(clrType.FullName));
                // }

                throw new NotSupportedException(
                    $"{clrType.FullName} for method {methodInfo.Name} is not supported in conceptual model as a structual type.");
            }

            return modelStructualType;
        }

        private static IList<EntitySet> GetModelEntitySets(this DbModel model, MethodInfo methodInfo, FunctionAttribute functionAttribute)
        {
            ParameterInfo returnParameterInfo = methodInfo.ReturnParameter;
            if (returnParameterInfo == null || returnParameterInfo.ParameterType == typeof(void))
            {
                throw new NotSupportedException($"The return parameter type of {methodInfo.Name} is not supported.");
            }

            if (functionAttribute.Type == FunctionType.StoredProcedure
                && returnParameterInfo.ParameterType != typeof(int))
            {
                // returnParameterInfo.ParameterType is ObjectResult<T>.
                Type[] returnParameterClrTypes = methodInfo.GetStoredProcedureReturnTypes().ToArray();
                if (returnParameterClrTypes.Length > 1)
                {
                    // Stored procedure has more than one result. 
                    // EdmFunctionPayload.EntitySets must be provided. Otherwise, an ArgumentException will be thrown:
                    // The EntitySets parameter must not be null for functions that return multiple result sets.
                    return returnParameterClrTypes.Select(clrType =>
                    {
                        EntitySet modelEntitySet = model
                            .ConceptualModel
                            .Container
                            .EntitySets
                            .FirstOrDefault(entitySet =>
                                entitySet.ElementType.FullName.EqualsOrdinal(clrType.FullName));
                        if (modelEntitySet == null)
                        {
                            throw new NotSupportedException(
                                $"{clrType.FullName} for method {methodInfo.Name} is not supported in conceptual model as entity set.");
                        }

                        return modelEntitySet;
                    }).ToArray();
                }
            }

            // Do not return new EntitySet[0], which causes a ArgumentException:
            // The number of entity sets should match the number of return parameters.
            return null;
        }
    }
}