namespace Examples.Sql;

using System.Data.SqlClient;
using Examples.Common;

internal static class Warehouses
{
    internal static void QQ(int qq)
    {
        ConcurrentQueue<int> groups = new();
        using SqlConnection connection = new("Server=.;Database=master;Trusted_Connection=True;MultipleActiveResultSets=true");
        connection.Open();
        Enumerable
            .Range(1, 11)
            .Select(databaseIndex => (databaseIndex, $"GroupData{databaseIndex}"))
            .ForEach(databaseName => Enumerable
                .Range(1, 100)
                .Select(tableIndex => $"Group{(databaseName.Item1 - 1) * 100 + tableIndex}")
                .ForEach(tableName =>
                {
                    Console.WriteLine($"{databaseName} => {tableName}");
                    using SqlCommand command = connection.CreateCommand();
                    command.CommandText = $"select QunNum from {databaseName.Item2}.dbo.{tableName} where QQNum = @QQ";
                    command.Parameters.AddWithValue("@QQ", qq);
                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int group = (int)reader[0];
                        groups.Enqueue(@group);
                        Trace.WriteLine(@group);
                    }
                }));
        groups.ForEach(group =>
        {
            using SqlCommand command = connection.CreateCommand();
            command.CommandText = $"select Title, QunText from QunInfo{@group / 10000000 + 1}.dbo.QunList{@group / 1000000 + 1} where qunnum = @QQ";
            command.Parameters.AddWithValue("@QQ", @group);
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine($"{reader["Title"]} {reader["QunText"]}");
            }
            else
            {
                Console.WriteLine(@group);
            }
        });
    }

    internal static void FormatQQ()
    {

        Enumerable
            .Range(1, 11)
            .Select(databaseIndex => $"GroupData{databaseIndex}")
            .ForEach((databaseName, databaseIndex) =>
            {
                if (databaseIndex == 0)
                {
                    return;
                }

                using SqlConnection connection = new($"Server=.;Database={databaseName};Trusted_Connection=True;");
                connection.Open();

                List<(string, string)> primaryKeys = new();
                using SqlCommand command1 = connection.CreateCommand();
                command1.CommandText = $"select name, OBJECT_NAME(OBJECT_ID) from sys.indexes where is_primary_key = 1 and OBJECT_NAME(OBJECT_ID) like 'Group%'";
                using SqlDataReader reader1 = command1.ExecuteReader();
                while (reader1.Read())
                {
                    primaryKeys.Add(((string)reader1[0], (string)reader1[1]));
                }

                primaryKeys
                    .Select(key => $"ALTER TABLE {key.Item2} DROP {key.Item1}")
                    .ForEach(drop =>
                    {
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = drop;
                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                    });

                List<(string, string)> indexes = new();
                using SqlCommand command2 = connection.CreateCommand();
                command2.CommandText = $"select name, OBJECT_NAME(object_ID) from sys.indexes where OBJECT_NAME(object_ID) like 'Group%' and name is not null;";
                using SqlDataReader reader2 = command2.ExecuteReader();
                while (reader2.Read())
                {
                    indexes.Add(((string)reader2[0], (string)reader2[1]));
                }

                indexes
                    .Select(key => $"drop index {key.Item1} on {key.Item2}")
                    .ForEach(drop =>
                    {
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = drop;
                        Console.WriteLine(command.CommandText);
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (SqlException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    });

                Enumerable
                    .Range(1, 100)
                    .Select(tableIndex => $"Group{databaseIndex * 100 + tableIndex}")
                    .ForEach(tableName =>
                    {
                        Console.WriteLine($"{databaseName} => {tableName}");
                        using SqlCommand command1 = connection.CreateCommand();
                        command1.CommandText = $"WITH CTE AS(SELECT *, RN = ROW_NUMBER() OVER(PARTITION BY qqnum, qunnum ORDER BY id) FROM dbo.{tableName}) delete from CTE WHERE RN > 1;";
                        Console.WriteLine(command1.CommandText);
                        Console.WriteLine(command1.ExecuteNonQuery());
                        using SqlCommand command2 = connection.CreateCommand();
                        command2.CommandText = $"ALTER TABLE {tableName} add constraint PK_{tableName}_QQNum_QunNum primary key clustered (qqnum, qunnum);";
                        Console.WriteLine(command2.CommandText);
                        Console.WriteLine(command2.ExecuteNonQuery());
                        using SqlCommand command3 = connection.CreateCommand();
                        command3.CommandText = $"alter table {tableName} drop column ID;";
                        Console.WriteLine(command3.CommandText);
                        Console.WriteLine(command3.ExecuteNonQuery());
                    });
            });
    }

    internal static void FormatQQ2()
    {

        Enumerable
            .Range(1, 11)
            .Select(databaseIndex => $"quninfo{databaseIndex}")
            .ForEach((databaseName, databaseIndex) =>
            {
                if (databaseIndex == 0)
                {
                    return;
                }

                using SqlConnection connection = new($"Server=.;Database={databaseName};Trusted_Connection=True;");
                connection.Open();

                List<(string, string)> primaryKeys = new();
                using SqlCommand command1 = connection.CreateCommand();
                command1.CommandText = $"select name, OBJECT_NAME(OBJECT_ID) from sys.indexes where is_primary_key = 1 and OBJECT_NAME(OBJECT_ID) like 'qunlist%'";
                using SqlDataReader reader1 = command1.ExecuteReader();
                while (reader1.Read())
                {
                    primaryKeys.Add(((string)reader1[0], (string)reader1[1]));
                }

                primaryKeys
                    .Select(key => $"ALTER TABLE {key.Item2} DROP {key.Item1}")
                    .ForEach(drop =>
                    {
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = drop;
                        Console.WriteLine(command.CommandText);
                        command.ExecuteNonQuery();
                    });

                List<(string, string)> indexes = new();
                using SqlCommand command2 = connection.CreateCommand();
                command2.CommandText = $"select name, OBJECT_NAME(object_ID) from sys.indexes where OBJECT_NAME(object_ID) like 'qunlist%' and name is not null;";
                using SqlDataReader reader2 = command2.ExecuteReader();
                while (reader2.Read())
                {
                    indexes.Add(((string)reader2[0], (string)reader2[1]));
                }

                indexes
                    .Select(key => $"drop index {key.Item1} on {key.Item2}")
                    .ForEach(drop =>
                    {
                        using SqlCommand command = connection.CreateCommand();
                        command.CommandText = drop;
                        Console.WriteLine(command.CommandText);
                        try
                        {
                            command.ExecuteNonQuery();
                        }
                        catch (SqlException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    });

                Enumerable
                    .Range(1, 10)
                    .Select(tableIndex => $"QunList{databaseIndex * 10 + tableIndex}")
                    .ForEach(tableName =>
                    {
                        Console.WriteLine($"{databaseName} => {tableName}");
                        using SqlCommand command1 = connection.CreateCommand();
                        command1.CommandText = $"WITH CTE AS(SELECT *, RN = ROW_NUMBER() OVER(PARTITION BY qunnum ORDER BY id) FROM dbo.{tableName}) delete from CTE WHERE RN > 1;";
                        Console.WriteLine(command1.CommandText);
                        Console.WriteLine(command1.ExecuteNonQuery());
                        using SqlCommand command2 = connection.CreateCommand();
                        command2.CommandText = $"ALTER TABLE {tableName} add constraint PK_{tableName}_QunNum primary key clustered (qunnum);";
                        Console.WriteLine(command2.CommandText);
                        Console.WriteLine(command2.ExecuteNonQuery());
                        using SqlCommand command3 = connection.CreateCommand();
                        command3.CommandText = $"alter table {tableName} drop column Id;";
                        Console.WriteLine(command3.CommandText);
                        Console.WriteLine(command3.ExecuteNonQuery());
                    });
            });
    }

    internal static void Aipai()
    {
        using SqlConnection connection = new("");
        using StreamWriter writer = new(File.OpenWrite(@"D:\Sql\Aipai.txt"), Encoding.Unicode);
        using StreamWriter writer2 = new(File.OpenWrite(@"D:\Sql\Aipai2.txt"), Encoding.Unicode);
        Directory.EnumerateFiles(@"D:\Sql\Aipai").ForEach(file =>
        {
            using StreamReader reader = new(File.OpenRead(file), Encoding.GetEncoding("GB18030"));
            string? line;
            while ((line = reader.ReadLine()) is not null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    writer2.WriteLine(line);
                    continue;
                }
                char first = line[0];
                if (first == '\t' || first == '<')
                {
                    writer2.WriteLine(line);
                    continue;
                }
                Match match = Regex.Match(line, @"(.*)[ ]+([0-9a-z]{32})[ ]+(.*)");
                GroupCollection groups = match.Groups;
                if (groups.Count == 4)
                {
                    string last = groups[3].Value;
                    if (last.ContainsOrdinal("@"))
                    {
                        string newLine = string.Join("\t", new[] { groups[1].Value.TrimEnd(), groups[2].Value, last });
                        writer.WriteLine(newLine);
                    }
                    else
                    {
                        string newLine = string.Join("\t", new[] { groups[1].Value.TrimEnd(), groups[2].Value, string.Empty });
                        writer.WriteLine(newLine);
                    }
                }
                else
                {
                    string[] fields = Regex.Split(line, "[ ]{2,}");
                    string lastField = fields.Last();
                    if (lastField.ContainsOrdinal("@"))
                    {
                        string firstField = line.Substring(0, line.Length - lastField.Length).TrimEnd();
                        string newLine = string.Join("\t", new[] { firstField, lastField, string.Empty });
                        writer.WriteLine(newLine);
                    }
                    else if (lastField == fields[0])
                    {
                        string newLine = string.Join("\t", new[] { lastField, string.Empty, string.Empty });
                        writer.WriteLine(newLine);
                    }
                    else
                    {
                        string newLine = string.Join("\t", new[] { line.TrimEnd(), string.Empty, string.Empty });
                        writer.WriteLine(newLine);
                    }
                }
                //string[] fields = Regex.Split(line, "[ ]{2,}");
                //int fieldsLength = fields.Length;
                //if (fieldsLength > 2 && !fields[2].Contains("@"))
                //{
                //    fields[2] = null;
                //}
                //string firstField = fields[0];
                //int firstFieldLength = firstField.Length;
                //if (firstFieldLength > 32 && firstField[firstFieldLength - 33] == ' ')
                //{
                //    Array.Resize(ref fields, 3);
                //    fields[2] = fields[1];
                //    fields[0] = firstField.Substring(0, firstFieldLength - 33);
                //    fields[1] = firstField.Substring(firstFieldLength - 32);
                //}
                //if(fields[1].Length != 32)
                //{
                //    if (fields[1].Contains("@"))
                //    {
                //        Array.Resize(ref fields, 3);
                //        fields[2] = fields[1];
                //        fields[1] = null;
                //    }
                //    else if (string.IsNullOrWhiteSpace(fields[1]) || fields[0] == fields[1])
                //    {
                //        fields[1] = null;
                //    }
                //    else
                //    {
                //        Debugger.Break();
                //    }
                //}
                //string formattedLine = string.Join("\t", fields);
                //if (fieldsLength == 2 || fieldsLength == 3)
                //{
                //    writer.WriteLine(formattedLine);
                //}
                //else
                //{
                //    writer2.WriteLine(formattedLine);
                //}
            }
        });
    }

    internal static void Tianya()
    {
        using StreamWriter writer = new(File.OpenWrite(@"D:\Sql\Tianya.txt"), Encoding.Unicode);
        using StreamWriter writer2 = new(File.OpenWrite(@"D:\Sql\Tianya2.txt"), Encoding.Unicode);
        string? previousFileLastLine = null;
        Directory.EnumerateFiles(@"D:\Sql\Tianya").ForEach(file =>
        {
            using StreamReader reader = new(File.OpenRead(file), Encoding.GetEncoding("GB18030"));
            string? line = string.Concat(previousFileLastLine, reader.ReadLine());
            do
            {
                Match match = Regex.Match(line!, @"([^ ]+)[ ]+([^ ]+)[ ]+(.*)");
                GroupCollection groups = match.Groups;
                if (groups.Count == 4)
                {
                    string last = groups[3].Value;
                    if (last.ContainsOrdinal("@"))
                    {
                        string newLine = string.Join("\t", new[] { groups[1].Value.TrimEnd(), groups[2].Value, last });
                        writer.WriteLine(newLine);
                    }
                    else
                    {
                        string newLine = string.Join("\t", new[] { groups[1].Value.TrimEnd(), groups[2].Value, string.Empty });
                        writer.WriteLine(newLine);
                    }
                }
                else
                {
                    writer2.WriteLine(line);
                }

            } while ((previousFileLastLine = line = reader.ReadLine()) is not null);
        });
    }

    internal static void Tianya2()
    {
        using StreamReader reader = new(File.OpenRead(@"d:\sql\t1.txt"), Encoding.GetEncoding("GB18030"));
        using StreamWriter writer = new(File.OpenWrite(@"d:\sql\t11.txt"), Encoding.Unicode);
        using StreamWriter writer2 = new(File.OpenWrite(@"d:\sql\t12.txt"), Encoding.Unicode);
        using StreamWriter writer3 = new(File.OpenWrite(@"d:\sql\t13.txt"), Encoding.Unicode);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.TrimStart();
            Match match = Regex.Match(line, @"([^\t]+)\t([^\t]+)\t([^\t]*)");
            GroupCollection groups = match.Groups;
            if (groups.Count != 4)
            {
                writer2.WriteLine(line); continue;
            }
            else
            {
                if (groups[1].Length > 100)
                {
                    writer3.WriteLine(line); continue;
                }
                if (groups[2].Length > 100)
                {
                    writer3.WriteLine(line); continue;
                }
                if (groups[3].Length > 100 || groups[3].Value.ContainsOrdinal(" "))
                {
                    writer3.WriteLine(line); continue;
                }
                writer.WriteLine(line);
            }
        }
    }

    internal static void Tianya3()
    {
        using StreamReader reader = new(File.OpenRead(@"d:\sql\t131.txt"), Encoding.GetEncoding("GB18030"));
        using StreamWriter writer = new(File.OpenWrite(@"d:\sql\t1311.txt"), Encoding.Unicode);
        using StreamWriter writer2 = new(File.OpenWrite(@"d:\sql\t1312.txt"), Encoding.Unicode);
        using StreamWriter writer3 = new(File.OpenWrite(@"d:\sql\t1313.txt"), Encoding.Unicode);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.TrimStart();
            Match match = Regex.Match(line, @"([^\t]+)\t([^\t]+)\t([^\t]*)");
            GroupCollection groups = match.Groups;
            if (groups.Count != 4)
            {
                writer2.WriteLine(line); continue;
            }
            else
            {
                if (groups[1].Length > 100)
                {
                    writer3.WriteLine(line); continue;
                }
                if (groups[2].Length > 100)
                {
                    writer3.WriteLine(line); continue;
                }
                if (groups[3].Length > 100 || groups[3].Value.EndsWith(" "))
                {
                    writer3.WriteLine(line); continue;
                }
                writer.WriteLine(line);
            }
        }
    }
}