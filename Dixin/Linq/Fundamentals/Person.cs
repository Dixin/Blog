namespace Dixin.Linq.Fundamentals
{
    internal partial class Person
    {
        internal string Name { get; set; }

        internal int Age { get; set; }
    }

    internal partial class Person
    {
        internal string PlaceOfBirth { get; set; }
    }
}

namespace Dixin.Linq.Fundamentals.DataAnnotation
{
    using System.ComponentModel.DataAnnotations;

    internal class Person
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NameRequired))]
        [StringLength(1, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.InvalidName))]
        internal string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.AgeRequired))]
        [Range(0, 123, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.InvalidAge))] // https://en.wikipedia.org/wiki/Oldest_people
        internal int Age { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.InvalidEmail))]
        internal string Email { get; set; }
    }
}

#if DEMO
namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Diagnostics.Contracts;

    internal class Person
    {
        private readonly string name;

        private readonly int age;

        internal Person(string name, int age)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Requires<ArgumentOutOfRangeException>(age >= 0);

            this.name = name;
            this.age = age;
        }

        internal string Name
        {
            [Pure]
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return this.name;
            }
        }

        internal int Age
        {
            [Pure]
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);

                return this.age;
            }
        }
    }
}
#endif
