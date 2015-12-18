namespace Dixin.Linq.Fundamentals
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;

    [CompilerGenerated]
    [DebuggerDisplay(@"\{ Name = {Name}, Age = {Age} }", Type = "<Anonymous Type>")]
    internal sealed class AnonymousType<TName, TAge>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TName nameBackingField;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly TAge ageBackingField;

        [DebuggerHidden]
        public AnonymousType(TName name, TAge age)
        {
            this.nameBackingField = name;
            this.ageBackingField = age;
        }

        public TAge Age { get { return this.ageBackingField; } }

        public TName Name { get { return this.nameBackingField; } }

        [DebuggerHidden]
        public override bool Equals(object value)
        {
            AnonymousType<TName, TAge> anonymous = value as AnonymousType<TName, TAge>;
            return anonymous != null
                && EqualityComparer<TName>.Default.Equals(this.nameBackingField, anonymous.nameBackingField)
                && EqualityComparer<TAge>.Default.Equals(this.ageBackingField, anonymous.ageBackingField);
        }

        [DebuggerHidden]
        public override int GetHashCode()
        {
            int num = 0x7d068cce;
            num = (-1521134295 * num) + EqualityComparer<TName>.Default.GetHashCode(this.nameBackingField);
            return (-1521134295 * num) + EqualityComparer<TAge>.Default.GetHashCode(this.ageBackingField);
        }

        [DebuggerHidden]
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{ Name = ");
            builder.Append(this.nameBackingField);
            builder.Append(", Age = ");
            builder.Append(this.ageBackingField);
            builder.Append(" }");
            return builder.ToString();
        }
    }
}
