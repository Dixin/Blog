namespace Dixin.Linq.LinqToSql
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq;

    using Dixin.Reflection;

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

        public static void Associate<TThis, TOther, TKey>(
            this TThis @this, 
            Action<TKey> setThisKey, 
            EntityRef<TOther> thisEntityRef, 
            TOther other, 
            Func<TKey> getOtherKey, 
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
                    setThisKey(getOtherKey());
                }
                else
                {
                    setThisKey(default(TKey));
                }
            }
        }

        public static void SetForeignKey<TThis, TOther, TKey>(
            this TThis @this, TKey value, string key, string entity)
            where TOther : class
        {
            if (!EqualityComparer<TKey>.Default.Equals(@this.GetField<TKey>(key), value))
            {
                if (@this.GetField<EntityRef<TOther>>(entity).HasLoadedOrAssignedValue)
                {
                    throw new ForeignKeyReferenceAlreadyHasValueException();
                }

                @this.SetField(key, value);
            }
        }

        public static void Associate<TThis, TOther, TColumn>(
            this TThis @this, TOther other, string entity, string entitySet, string thisKey, string otherKey)
            where TOther : class
            where TThis : class
        {
            EntityRef<TOther> entityRef = @this.GetField<EntityRef<TOther>>(entity);
            TOther previousOther = entityRef.Entity;
            if (previousOther != other || !entityRef.HasLoadedOrAssignedValue)
            {
                if (previousOther != null)
                {
                    entityRef.Entity = null;
                    previousOther.GetProperty<EntitySet<TThis>>(entitySet).Remove(@this);
                }

                entityRef.Entity = other;
                if (other != null)
                {
                    other.GetProperty<EntitySet<TThis>>(entitySet).Add(@this);
                    @this.SetProperty(thisKey, other.GetProperty<TColumn>(otherKey));
                }
                else
                {
                    @this.SetProperty(thisKey, default(TColumn));
                }
            }
        }
    }
}