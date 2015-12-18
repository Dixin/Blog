namespace Dixin.Linq.Fundamentals
{
    public partial class Person
    {
        public string Name { get; set; }

        public int Age { get; set; }
    }

    public partial class Person
    {
        public string PlaceOfBirth { get; set; }
    }
}

namespace Dixin.Linq.Fundamentals.DataAnnotation
{
    using System.ComponentModel.DataAnnotations;

    public class Person
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NameRequired))]
        [StringLength(1, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.InvalidName))]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.AgeRequired))]
        [Range(0, 123, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.InvalidAge))] // https://en.wikipedia.org/wiki/Oldest_people
        public int Age { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.InvalidEmail))]
        public string Email { get; set; }
    }
}

namespace Dixin.Linq.Fundamentals.Contracts
{
    using System;
    using System.Diagnostics.Contracts;

    public class Person
    {
        private readonly string name;

        private readonly int age;

        public Person(string name, int age)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Requires<ArgumentOutOfRangeException>(age >= 0);

            this.name = name;
            this.age = age;
        }

        public string Name
        {
            [Pure]
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return this.name;
            }
        }

        public int Age
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
