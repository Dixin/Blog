namespace Examples.Reflection;

using Examples.Common;

public static class FieldInfoExtensions
{
    #region Methods

    [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
    public static void SetValue<T>(this FieldInfo field, ref T obj, object? value)
    {
        field.ThrowIfNull();

        if (typeof(T).IsValueType)
        {
            // Cannot use SetValue, which boxes obj.
            field.SetValueDirect(__makeref(obj), value!);
        }
        else
        {
            field.SetValue(obj, value);
        }
    }

    #endregion
}