namespace Tutorial.Introduction
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Contracts;

    using Tutorial.Resources;

    public class Contact
    {
        [Required(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NameRequired))]
        [StringLength(maximumLength: 50, MinimumLength = 1, ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.NameInvalid))]
        public string Name { get; set; }

        [EmailAddress(ErrorMessageResourceType = typeof(Resources), ErrorMessageResourceName = nameof(Resources.EmailInvalid))]
        public string Email { get; set; }
    }

    public class Model
    {
        private readonly string name;

        private readonly int weight;

        public Model(string name, int weight)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Requires<ArgumentOutOfRangeException>(weight > 0);

            this.name = name;
            this.weight = weight;
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

        public int Weight
        {
            [Pure]
            get
            {
                Contract.Ensures(Contract.Result<int>() > 0);

                return this.weight;
            }
        }
    }
}
