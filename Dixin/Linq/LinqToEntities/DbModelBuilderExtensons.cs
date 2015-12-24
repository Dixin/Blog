namespace Dixin.Linq.LinqToEntities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;
    using System.Reflection;

    public static class DbModelBuilderExtensons
    {
        public static void AddComplexTypesFromAssembly(this DbModelBuilder modelBuilder, Assembly assembly)
        {
            IEnumerable<Type> complexTypes = assembly.ExportedTypes.Where(type => Attribute.IsDefined((MemberInfo) type, typeof(ComplexTypeAttribute)));
            MethodInfo complexTypeMethod = typeof(DbModelBuilder).GetMethod(nameof(modelBuilder.ComplexType));
            complexTypes.ForEach(complexType => 
                complexTypeMethod.MakeGenericMethod(complexType).Invoke(modelBuilder, null));
        }

        public static void FunctionConvention<TDbContext>(
            this DbModelBuilder modelBuilder, bool addComplexTypesFromAssembly = true)
        {
            modelBuilder.AddComplexTypesFromAssembly(typeof(TDbContext).Assembly);
            modelBuilder.Conventions.Add(new FunctionConvention<TDbContext>(modelBuilder));
        }
    }
}