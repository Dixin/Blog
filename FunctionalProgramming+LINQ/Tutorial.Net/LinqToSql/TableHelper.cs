namespace Tutorial.LinqToSql
{
    using System;
    using System.Data.Linq;
    using System.Data.Linq.Mapping;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    public static class TableHelper
    {
        public static void SetForeignKey<TOther>(
            this EntityRef<TOther> entityRef, Func<bool> areEqual, Action setKey)
            where TOther : class
        {
            if (!areEqual())
            {
                if (entityRef.HasLoadedOrAssignedValue)
                {
                    throw new ForeignKeyReferenceAlreadyHasValueException();
                }

                setKey();
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        public static void Associate<TThis, TOther>(
            this TThis @this,
            Action setThisKey,
            ref EntityRef<TOther> thisEntityRef,
            TOther other,
            Func<TOther, EntitySet<TThis>> getOtherEntitySet)
            where TOther : class
            where TThis : class
        {
            TOther previousOther = thisEntityRef.Entity;
            if (previousOther != other || !thisEntityRef.HasLoadedOrAssignedValue)
            {
                if (previousOther != null)
                {
                    thisEntityRef.Entity = null;
                    getOtherEntitySet(previousOther).Remove(@this);
                }

                thisEntityRef.Entity = other;
                if (other != null)
                {
                    getOtherEntitySet(other).Add(@this);
                }
                setThisKey();
            }
        }

        public static TEntity Find<TEntity>(this DataContext database, params object[] keys)
            where TEntity : class
        {
            MetaType metaType = database.Mapping.GetMetaType(typeof(TEntity));
            if (metaType == null)
            {
                throw new NotSupportedException($"{nameof(TEntity)} must be mapped.");
            }
            MetaDataMember[] primaryKeys = database
                .Mapping
                .GetMetaType(typeof(TEntity))
                .DataMembers
                .Where(member => member.IsPrimaryKey)
                .ToArray();
            if (keys.Length != primaryKeys.Length)
            {
                throw new ArgumentException($"{nameof(keys)} must have correct number of values.", nameof(keys));
            }

            ParameterExpression entity = Expression.Parameter(typeof(TEntity), nameof(entity));
            Expression predicateBody = database
                .Mapping
                .GetMetaType(typeof(TEntity))
                .DataMembers
                .Where(member => member.IsPrimaryKey)
                .Select((primaryKey, index) => Expression.Equal(
                    Expression.Property(entity, primaryKey.Name),
                    Expression.Constant(keys[index])))
                .Aggregate<Expression, Expression>(Expression.Constant(true), Expression.AndAlso);
            return database.GetTable<TEntity>().Single(Expression.Lambda<Func<TEntity, bool>>(predicateBody, entity));
        }
    }
}