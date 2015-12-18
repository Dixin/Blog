namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Reflection;

    public partial class Imperative
    {
        public static List<Person> FilterAndOrderByAge(IEnumerable<Person> source)
        {
            List<Person> results = new List<Person>();
            foreach (Person person in source)
            {
                if (person.Age >= 18)
                {
                    results.Add(person);
                }
            }

            Comparison<Person> personComparison = delegate (Person a, Person b)
            {
                int ageComparison = 0 - a.Age.CompareTo(b.Age);
                return ageComparison != 0
                    ? ageComparison
                    : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            };
            results.Sort(personComparison);

            return results;
        }
    }

    public static partial class Imperative
    {
        public static IEnumerable<Type> GetTypes<TType>(Assembly assembly, bool isPublicOnly = false)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if ((!isPublicOnly || type.IsPublic) && typeof(TType).IsAssignableFrom(type))
                {
                    yield return type;
                }
            }
        }
    }

    public static partial class Imperative
    {
        public static IEnumerable<int> Positive(IEnumerable<int> source)
        {
            foreach (int value in source)
            {
                if (value > 0)
                {
                    yield return value;
                }
            }
        }
    }

    public static partial class Imperative
    {
        public static List<int> GetPositive(IEnumerable<int> source)
        {
            List<int> resullt = new List<int>();
            foreach (int value in source)
            {
                if (value > 0)
                {
                    resullt.Add(value);
                }
            }

            return resullt;
        }
    }

    public static partial class Imperative
    {
        public static List<string> ProductNames(string categoryName)
        {
            using (SqlConnection connection = new SqlConnection(
                @"Data Source=localhost;Initial Catalog=Northwind;Integrated Security=True"))
            using (SqlCommand command = new SqlCommand(
                @"SELECT [Products].[ProductName]
            FROM [Products]
            LEFT OUTER JOIN [Categories] ON [Categories].[CategoryID] = [Products].[CategoryID]
            WHERE [Categories].[CategoryName] = @categoryName", connection))
            {
                List<string> result = new List<string>();
                command.Parameters.AddWithValue("@categoryName", categoryName);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string productName = (string)reader["ProductName"];
                        result.Add(productName);
                    }

                    return result;
                }
            }
        }
    }
}
