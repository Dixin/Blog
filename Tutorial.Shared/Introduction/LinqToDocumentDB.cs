namespace Tutorial.Introduction
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    using Microsoft.Azure.Documents.Client;

    using Newtonsoft.Json;

    public class Store
    {
        [JsonProperty(PropertyName = "id")]
        public string Id;

        public string Name;

        public Address Address;
    }

    public class Address
    {
        public string AddressType;

        public string AddressLine1;

        public Location Location;

        public string PostalCode;

        public string CountryRegionName;
    }

    public class Location
    {
        public string City;

        public string StateProvinceName;
    }

    internal static partial class LinqToDocumentDB
    {
        internal static void QueryExpression(string key)
        {
            using (DocumentClient client = new DocumentClient(
                new Uri("https://dixin.documents.azure.com:443/"), key))
            {
                IOrderedQueryable<Store> source = client.CreateDocumentQuery<Store>(
                    UriFactory.CreateDocumentCollectionUri("dixin", "Store")); // Get source.
                IQueryable<string> query = from store in source
                                           where store.Address.Location.City == "Seattle"
                                           orderby store.Name
                                           select store.Name; // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }

    internal static partial class LinqToDocumentDB
    {
        internal static void QueryMethods(string key)
        {
            using (DocumentClient client = new DocumentClient(
                new Uri("https://dixin.documents.azure.com:443/"), key))
            {
                IOrderedQueryable<Store> source = client.CreateDocumentQuery<Store>(
                    UriFactory.CreateDocumentCollectionUri("dixin", "Store")); // Get source.
                IQueryable<string> query = source
                    .Where(store => store.Address.CountryRegionName == "United States")
                    .OrderBy(store => store.Address.PostalCode)
                    .Select(store => store.Name); // Define query.
                foreach (string result in query) // Execute query.
                {
                    Trace.WriteLine(result);
                }
            }
        }
    }
}