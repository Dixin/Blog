#nullable enable
namespace Examples.Dynamic;

using System.Dynamic;

using Examples.Common;
using Examples.Reflection;
using Microsoft.CSharp.RuntimeBinder;

public class DynamicWrapper<T> : DynamicObject
{
    #region Constants and Fields

#pragma warning disable 414
    [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
#pragma warning disable IDE0052 // Remove unread private members
    private readonly bool hasValue;
#pragma warning restore IDE0052 // Remove unread private members
#pragma warning restore 414

    private readonly bool isValueType;

    private readonly Type type;

    private T value; // Not readonly, for value type scenarios.

    #endregion

    #region Constructors and Destructors

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public DynamicWrapper() // For static.
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    {
        this.type = typeof(T);
        this.isValueType = this.type.IsValueType;
        this.hasValue = false;
    }

    [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
    public DynamicWrapper(ref T value) // Uses ref in case of 'value' is value type.
    {
        this.value = value.NotNull();
#if NETSTANDARD || NETCOREAPP
        this.type = value.GetType();
#else
        this.type = value!.GetType();
#endif
        this.isValueType = this.type.IsValueType;
        this.hasValue = true;
    }

    #endregion

    #region Public Methods

    public T ToStatic() => this.value;

    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        result = this.value;
        return true;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        if (this.value is Array array && indexes.All(item => item is int || item is long))
        {
            result = array.GetValue(indexes.Select(Convert.ToInt64).ToArray());
            return true;
        }

        PropertyInfo? index = this.type.GetTypeIndex(indexes);
        if (index is not null)
        {
            result = index.GetValue(this.value, indexes);
            return true;
        }

        MethodInfo? method = this.type.GetInterfaceMethod("get_Item", indexes);
        if (method is not null)
        {
            result = method.Invoke(this.value, indexes);
            return true;
        }

        index = this.type.GetBaseIndex(indexes);
        if (index is not null)
        {
            result = index.GetValue(this.value, indexes);
            return true;
        }

        result = null;
        return false;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        // Searches in current type's public and non-public properties.
        PropertyInfo? property = this.type.GetTypeProperty(binder.Name);
        if (property is not null)
        {
            result = property.GetValue(this.value, null).ToDynamic();
            return true;
        }

        // Searches in explicitly implemented properties for interface.
        MethodInfo? method = this.type.GetInterfaceMethod(string.Concat("get_", binder.Name));
        if (method is not null)
        {
            result = method.Invoke(this.value, null).ToDynamic();
            return true;
        }

        // Searches in current type's public and non-public fields.
        FieldInfo? field = this.type.GetTypeField(binder.Name);
        if (field is not null)
        {
            result = field.GetValue(this.value).ToDynamic();
            return true;
        }

        // Searches in base type's public and non-public properties.
        property = this.type.GetBaseProperty(binder.Name);
        if (property is not null)
        {
            result = property.GetValue(this.value, null).ToDynamic();
            return true;
        }

        // Searches in base type's public and non-public fields.
        field = this.type.GetBaseField(binder.Name);
        if (field is not null)
        {
            result = field.GetValue(this.value).ToDynamic();
            return true;
        }

        // The specified member is not found.
        result = null;
        return false;
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
        MethodInfo? method = this.type.GetTypeMethod(binder.Name, args ?? []) ??
            this.type.GetInterfaceMethod(binder.Name, args ?? []) ??
            this.type.GetBaseMethod(binder.Name, args ?? []);
        if (method is not null)
        {
            result = method.Invoke(this.value, args).ToDynamic();
            return true;
        }

        result = null;
        return false;
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
    {
        if (this.isValueType)
        {
            throw new NotSupportedException("Setting index on value type is not supported.");
        }

        if (this.value is Array array && indexes.All(item => item is int || item is long))
        {
            array.SetValue(value, indexes.Select(Convert.ToInt64).ToArray());
            return true;
        }

        PropertyInfo? index = this.type.GetTypeIndex(indexes);
        if (index is not null)
        {
            index.SetValue(this.value, value, indexes);
            return true;
        }

        MethodInfo? method = this.type.GetInterfaceMethod("set_Item", indexes);
        if (method is not null)
        {
            method.Invoke(this.value, indexes.Concat(Enumerable.Repeat(value, 1)).ToArray());
            return true;
        }

        index = this.type.GetBaseIndex(indexes);
        if (index is not null)
        {
            index.SetValue(this.value, value, indexes);
            return true;
        }

        return false;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (!this.isValueType)
        {
            PropertyInfo? property = this.type.GetTypeProperty(binder.Name);
            if (property is not null)
            {
                property.SetValue(this.value, value, null);
                return true;
            }
        }

        FieldInfo? field = this.type.GetTypeField(binder.Name);
        if (field is not null)
        {
            field.SetValue(ref this.value, value);
            return true;
        }

        if (!this.isValueType)
        {
            MethodInfo? method = this.type.GetInterfaceMethod(string.Concat("set_", binder.Name), value);
            method?.Invoke(this.value, [value]);

            PropertyInfo? property = this.type.GetBaseProperty(binder.Name);
            if (property is not null)
            {
                property.SetValue(this.value, value, null);
                return true;
            }
        }

        field = this.type.GetBaseField(binder.Name);
        if (field is not null)
        {
            field.SetValue(ref this.value, value);
            return true;
        }

        if (this.isValueType)
        {
            throw new RuntimeBinderException(
                "The specified field is not found (Setting property is not supported on value type).");
        }

        return false;
    }

    #endregion
}