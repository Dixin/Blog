namespace Dixin.Linq.Fundamentals
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Reflection;

    internal partial class Imperative
    {
        internal static List<Person> FilterAndOrderByAge(IEnumerable<Person> source)
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

    internal static partial class Imperative
    {
        internal static IEnumerable<Type> GetTypes<TType>(Assembly assembly, bool isPublicOnly = false)
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

    internal static partial class Imperative
    {
        internal static IEnumerable<int> Positive(IEnumerable<int> source)
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

    internal static partial class Imperative
    {
        internal static List<int> GetPositive(IEnumerable<int> source)
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

    internal static partial class Imperative
    {
        internal static List<string> ProductNames(string categoryName)
        {
            using (SqlConnection connection = new SqlConnection(
                @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\AdventureWorks_Data.mdf;Integrated Security=True;Connect Timeout=30"))
            using (SqlCommand command = new SqlCommand(
                @"SELECT [Product].[Name]
                FROM [Production].[Product] AS [Product]
                LEFT OUTER JOIN [Production].[ProductSubcategory] AS [Subcategory] 
                    ON [Subcategory].[ProductSubcategoryID] = [Product].[ProductSubcategoryID]
                LEFT OUTER JOIN [Production].[ProductCategory] AS [Category] 
                    ON [Category].[ProductCategoryID] = [Subcategory].[ProductCategoryID]
                WHERE [Category].[Name] = @categoryName
                ORDER BY [Product].[ListPrice]", 
                connection))
            {
                List<string> result = new List<string>();
                command.Parameters.AddWithValue("@categoryName", categoryName);
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string productName = (string)reader["Name"];
                        result.Add(productName);
                    }

                    return result;
                }
            }
        }
    }
}
