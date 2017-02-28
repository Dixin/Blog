namespace Tutorial.Tests.Introduction
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    using Tutorial.Introduction;
    using Tutorial.Resources;

    using EnumerableAssert = Tutorial.LinqToObjects.EnumerableAssert;

    [TestClass]
    public class ContactTests
    {
        [TestMethod]
        public void NameValidationTest()
        {
            Contact contact1 = new Contact();
            ValidationContext validationContext1 = new ValidationContext(contact1);
            List<ValidationResult> results1 = new List<ValidationResult>();
            Assert.IsFalse(Validator.TryValidateObject(contact1, validationContext1, results1, true));
            Assert.AreEqual(nameof(Contact.Name), results1.Single().MemberNames.Single());
            Assert.AreEqual(Resources.NameRequired, results1.Single().ErrorMessage);

            Contact contact2 = new Contact() { Name = string.Empty };
            ValidationContext validationContext2 = new ValidationContext(contact2);
            List<ValidationResult> results2 = new List<ValidationResult>();
            Assert.IsFalse(Validator.TryValidateObject(contact2, validationContext2, results2, true));
            Assert.AreEqual(nameof(Contact.Name), results2.Single().MemberNames.Single());
            Assert.AreEqual(Resources.NameRequired, results2.Single().ErrorMessage);

            Contact contact3 = new Contact() { Name = nameof(Contact.Name) };
            ValidationContext validationContext3 = new ValidationContext(contact3);
            List<ValidationResult> results3 = new List<ValidationResult>();
            Assert.IsTrue(Validator.TryValidateObject(contact3, validationContext3, results3, true));
            EnumerableAssert.IsEmpty(results3);

            Contact contact4 = new Contact() { Name = new string('A', 100) };
            ValidationContext validationContext4 = new ValidationContext(contact4);
            List<ValidationResult> results4 = new List<ValidationResult>();
            Assert.IsFalse(Validator.TryValidateObject(contact4, validationContext4, results4, true));
            Assert.AreEqual(nameof(Contact.Name), results4.Single().MemberNames.Single());
            Assert.AreEqual(Resources.NameInvalid, results4.Single().ErrorMessage);
        }

        [TestMethod]
        public void EmailValidationTest()
        {
            Contact contact1 = new Contact() { Name = nameof(Contact.Name), Email = "user@host.com" };
            ValidationContext validationContext1 = new ValidationContext(contact1);
            List<ValidationResult> results1 = new List<ValidationResult>();
            Assert.IsTrue(Validator.TryValidateObject(contact1, validationContext1, results1, true));
            EnumerableAssert.IsEmpty(results1);

            Contact contact2 = new Contact() { Name = nameof(Contact.Name), Email = "user" };
            ValidationContext validationContext2 = new ValidationContext(contact2);
            List<ValidationResult> results2 = new List<ValidationResult>();
            Assert.IsFalse(Validator.TryValidateObject(contact2, validationContext2, results2, true));
            Assert.AreEqual(nameof(Contact.Email), results2.Single().MemberNames.Single());
            Assert.AreEqual(Resources.EmailInvalid, results2.Single().ErrorMessage);
        }
    }
}
