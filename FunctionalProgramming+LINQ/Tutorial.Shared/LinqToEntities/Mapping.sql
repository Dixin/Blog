-- Query all schema.
SELECT
    [UnionAll1].[Ordinal] AS [C1],
    [Extent1].[CatalogName] AS [CatalogName],
    [Extent1].[SchemaName] AS [SchemaName],
    [Extent1].[Name] AS [Name],
    [UnionAll1].[Name] AS [C2],
    [UnionAll1].[IsNullable] AS [C3],
    [UnionAll1].[TypeName] AS [C4],
    [UnionAll1].[MaxLength] AS [C5],
    [UnionAll1].[Precision] AS [C6],
    [UnionAll1].[DateTimePrecision] AS [C7],
    [UnionAll1].[Scale] AS [C8],
    [UnionAll1].[IsIdentity] AS [C9],
    [UnionAll1].[IsStoreGenerated] AS [C10],
    CASE
        WHEN ([Project5].[C2] IS NULL) THEN CAST(0 AS bit)
        ELSE [Project5].[C2]
    END AS [C11]
FROM (
    SELECT
        QUOTENAME(TABLE_SCHEMA) + QUOTENAME(TABLE_NAME) [Id],
        TABLE_CATALOG [CatalogName],
        TABLE_SCHEMA [SchemaName],
        TABLE_NAME [Name]
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE') AS [Extent1]
INNER JOIN (
    SELECT
        [Extent2].[Id] AS [Id],
        [Extent2].[Name] AS [Name],
        [Extent2].[Ordinal] AS [Ordinal],
        [Extent2].[IsNullable] AS [IsNullable],
        [Extent2].[TypeName] AS [TypeName],
        [Extent2].[MaxLength] AS [MaxLength],
        [Extent2].[Precision] AS [Precision],
        [Extent2].[DateTimePrecision] AS [DateTimePrecision],
        [Extent2].[Scale] AS [Scale],
        [Extent2].[IsIdentity] AS [IsIdentity],
        [Extent2].[IsStoreGenerated] AS [IsStoreGenerated],
        0 AS [C1],
        [Extent2].[ParentId] AS [ParentId]
    FROM (
        SELECT
            QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) + QUOTENAME(c.COLUMN_NAME) [Id],
            QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) [ParentId],
            c.COLUMN_NAME [Name],
            c.ORDINAL_POSITION [Ordinal],
            CAST(CASE c.IS_NULLABLE
                WHEN 'YES' THEN 1
                WHEN 'NO' THEN 0
                ELSE 0
            END AS bit) [IsNullable],
            CASE
                WHEN c.DATA_TYPE IN ('varchar', 'nvarchar', 'varbinary') AND
                    c.CHARACTER_MAXIMUM_LENGTH = -1 THEN c.DATA_TYPE + '(max)'
                ELSE c.DATA_TYPE
            END
            AS [TypeName],
            c.CHARACTER_MAXIMUM_LENGTH [MaxLength],
            CAST(c.NUMERIC_PRECISION AS integer) [Precision],
            CAST(c.DATETIME_PRECISION AS integer) [DateTimePrecision],
            CAST(c.NUMERIC_SCALE AS integer) [Scale],
            c.COLLATION_CATALOG [CollationCatalog],
            c.COLLATION_SCHEMA [CollationSchema],
            c.COLLATION_NAME [CollationName],
            c.CHARACTER_SET_CATALOG [CharacterSetCatalog],
            c.CHARACTER_SET_SCHEMA [CharacterSetSchema],
            c.CHARACTER_SET_NAME [CharacterSetName],
            CAST(0 AS bit) AS [IsMultiSet],
            CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') AS bit) AS [IsIdentity],
            CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed') | CASE
                WHEN c.DATA_TYPE = 'timestamp' THEN 1
                ELSE 0
            END AS bit) AS [IsStoreGenerated],
            c.COLUMN_DEFAULT AS [Default]
        FROM INFORMATION_SCHEMA.COLUMNS c
        INNER JOIN INFORMATION_SCHEMA.TABLES t
            ON c.TABLE_CATALOG = t.TABLE_CATALOG
            AND c.TABLE_SCHEMA = t.TABLE_SCHEMA
            AND c.TABLE_NAME = t.TABLE_NAME
            AND t.TABLE_TYPE = 'BASE TABLE') AS [Extent2]
    UNION ALL
    SELECT
        [Extent3].[Id] AS [Id],
        [Extent3].[Name] AS [Name],
        [Extent3].[Ordinal] AS [Ordinal],
        [Extent3].[IsNullable] AS [IsNullable],
        [Extent3].[TypeName] AS [TypeName],
        [Extent3].[MaxLength] AS [MaxLength],
        [Extent3].[Precision] AS [Precision],
        [Extent3].[DateTimePrecision] AS [DateTimePrecision],
        [Extent3].[Scale] AS [Scale],
        [Extent3].[IsIdentity] AS [IsIdentity],
        [Extent3].[IsStoreGenerated] AS [IsStoreGenerated],
        6 AS [C1],
        [Extent3].[ParentId] AS [ParentId]
    FROM (
        SELECT
            QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) + QUOTENAME(c.COLUMN_NAME) [Id],
            QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) [ParentId],
            c.COLUMN_NAME [Name],
            c.ORDINAL_POSITION [Ordinal],
            CAST(CASE c.IS_NULLABLE
                WHEN 'YES' THEN 1
                WHEN 'NO' THEN 0
                ELSE 0
            END AS bit) [IsNullable],
            CASE
                WHEN c.DATA_TYPE IN ('varchar', 'nvarchar', 'varbinary') AND
                    c.CHARACTER_MAXIMUM_LENGTH = -1 THEN c.DATA_TYPE + '(max)'
                ELSE c.DATA_TYPE
            END
            AS [TypeName],
            c.CHARACTER_MAXIMUM_LENGTH [MaxLength],
            CAST(c.NUMERIC_PRECISION AS integer) [Precision],
            CAST(c.DATETIME_PRECISION AS integer) AS [DateTimePrecision],
            CAST(c.NUMERIC_SCALE AS integer) [Scale],
            c.COLLATION_CATALOG [CollationCatalog],
            c.COLLATION_SCHEMA [CollationSchema],
            c.COLLATION_NAME [CollationName],
            c.CHARACTER_SET_CATALOG [CharacterSetCatalog],
            c.CHARACTER_SET_SCHEMA [CharacterSetSchema],
            c.CHARACTER_SET_NAME [CharacterSetName],
            CAST(0 AS bit) AS [IsMultiSet],
            CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') AS bit) AS [IsIdentity],
            CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed') | CASE
                WHEN c.DATA_TYPE = 'timestamp' THEN 1
                ELSE 0
            END AS bit) AS [IsStoreGenerated],
            c.COLUMN_DEFAULT [Default]
        FROM INFORMATION_SCHEMA.COLUMNS c
        INNER JOIN INFORMATION_SCHEMA.VIEWS v
            ON c.TABLE_CATALOG = v.TABLE_CATALOG
            AND c.TABLE_SCHEMA = v.TABLE_SCHEMA
            AND c.TABLE_NAME = v.TABLE_NAME
        WHERE NOT (v.TABLE_SCHEMA = 'dbo'
        AND v.TABLE_NAME IN ('syssegments', 'sysconstraints')
        AND SUBSTRING(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 1, 1) = 8)) AS [Extent3]) AS [UnionAll1]
    ON (0 = [UnionAll1].[C1])
    AND ([Extent1].[Id] = [UnionAll1].[ParentId])
LEFT OUTER JOIN (
    SELECT
        [UnionAll2].[Id] AS [C1],
        CAST(1 AS bit) AS [C2]

    FROM (
        SELECT
            QUOTENAME(tc.CONSTRAINT_SCHEMA) + QUOTENAME(tc.CONSTRAINT_NAME) [Id],
            QUOTENAME(tc.TABLE_SCHEMA) + QUOTENAME(tc.TABLE_NAME) [ParentId],
            tc.CONSTRAINT_NAME [Name],
            tc.CONSTRAINT_TYPE [ConstraintType],
            CAST(CASE tc.IS_DEFERRABLE
                WHEN 'NO' THEN 0
                ELSE 1
            END AS bit) [IsDeferrable],
            CAST(CASE tc.INITIALLY_DEFERRED
                WHEN 'NO' THEN 0
                ELSE 1
            END AS bit) [IsInitiallyDeferred]
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        WHERE tc.TABLE_NAME IS NOT NULL) AS [Extent4]
    INNER JOIN (
        SELECT
            7 AS [C1],
            [Extent5].[ConstraintId] AS [ConstraintId],
            [Extent6].[Id] AS [Id]
        FROM (
            SELECT
                QUOTENAME(CONSTRAINT_SCHEMA) + QUOTENAME(CONSTRAINT_NAME) [ConstraintId],
                QUOTENAME(TABLE_SCHEMA) + QUOTENAME(TABLE_NAME) + QUOTENAME(COLUMN_NAME) [ColumnId]
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE) AS [Extent5]
        INNER JOIN (
            SELECT
                QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) + QUOTENAME(c.COLUMN_NAME) [Id],
                QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) [ParentId],
                c.COLUMN_NAME [Name],
                c.ORDINAL_POSITION [Ordinal],
                CAST(CASE c.IS_NULLABLE
                    WHEN 'YES' THEN 1
                    WHEN 'NO' THEN 0
                    ELSE 0
                END AS bit) [IsNullable],
                CASE
                    WHEN c.DATA_TYPE IN ('varchar', 'nvarchar', 'varbinary') AND
                        c.CHARACTER_MAXIMUM_LENGTH = -1 THEN c.DATA_TYPE + '(max)'
                    ELSE c.DATA_TYPE
                END
                AS [TypeName],
                c.CHARACTER_MAXIMUM_LENGTH [MaxLength],
                CAST(c.NUMERIC_PRECISION AS integer) [Precision],
                CAST(c.DATETIME_PRECISION AS integer) [DateTimePrecision],
                CAST(c.NUMERIC_SCALE AS integer) [Scale],
                c.COLLATION_CATALOG [CollationCatalog],
                c.COLLATION_SCHEMA [CollationSchema],
                c.COLLATION_NAME [CollationName],
                c.CHARACTER_SET_CATALOG [CharacterSetCatalog],
                c.CHARACTER_SET_SCHEMA [CharacterSetSchema],
                c.CHARACTER_SET_NAME [CharacterSetName],
                CAST(0 AS bit) AS [IsMultiSet],
                CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') AS bit) AS [IsIdentity],
                CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed') | CASE
                    WHEN c.DATA_TYPE = 'timestamp' THEN 1
                    ELSE 0
                END AS bit) AS [IsStoreGenerated],
                c.COLUMN_DEFAULT AS [Default]
            FROM INFORMATION_SCHEMA.COLUMNS c
            INNER JOIN INFORMATION_SCHEMA.TABLES t
                ON c.TABLE_CATALOG = t.TABLE_CATALOG
                AND c.TABLE_SCHEMA = t.TABLE_SCHEMA
                AND c.TABLE_NAME = t.TABLE_NAME
                AND t.TABLE_TYPE = 'BASE TABLE') AS [Extent6]
            ON [Extent6].[Id] = [Extent5].[ColumnId]
        UNION ALL
        SELECT
            11 AS [C1],
            [Extent7].[ConstraintId] AS [ConstraintId],
            [Extent8].[Id] AS [Id]
        FROM (
            SELECT
                CAST(NULL AS nvarchar(1)) [ConstraintId],
                CAST(NULL AS nvarchar(max)) [ColumnId]
            WHERE 1 = 2) AS [Extent7]
        INNER JOIN (
            SELECT
                QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) + QUOTENAME(c.COLUMN_NAME) [Id],
                QUOTENAME(c.TABLE_SCHEMA) + QUOTENAME(c.TABLE_NAME) [ParentId],
                c.COLUMN_NAME [Name],
                c.ORDINAL_POSITION [Ordinal],
                CAST(CASE c.IS_NULLABLE
                    WHEN 'YES' THEN 1
                    WHEN 'NO' THEN 0
                    ELSE 0
                END AS bit) [IsNullable],
                CASE
                    WHEN c.DATA_TYPE IN ('varchar', 'nvarchar', 'varbinary') AND
                        c.CHARACTER_MAXIMUM_LENGTH = -1 THEN c.DATA_TYPE + '(max)'
                    ELSE c.DATA_TYPE
                END
                AS [TypeName],
                c.CHARACTER_MAXIMUM_LENGTH [MaxLength],
                CAST(c.NUMERIC_PRECISION AS integer) [Precision],
                CAST(c.DATETIME_PRECISION AS integer) AS [DateTimePrecision],
                CAST(c.NUMERIC_SCALE AS integer) [Scale],
                c.COLLATION_CATALOG [CollationCatalog],
                c.COLLATION_SCHEMA [CollationSchema],
                c.COLLATION_NAME [CollationName],
                c.CHARACTER_SET_CATALOG [CharacterSetCatalog],
                c.CHARACTER_SET_SCHEMA [CharacterSetSchema],
                c.CHARACTER_SET_NAME [CharacterSetName],
                CAST(0 AS bit) AS [IsMultiSet],
                CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity') AS bit) AS [IsIdentity],
                CAST(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed') | CASE
                    WHEN c.DATA_TYPE = 'timestamp' THEN 1
                    ELSE 0
                END AS bit) AS [IsStoreGenerated],
                c.COLUMN_DEFAULT [Default]
            FROM INFORMATION_SCHEMA.COLUMNS c
            INNER JOIN INFORMATION_SCHEMA.VIEWS v
                ON c.TABLE_CATALOG = v.TABLE_CATALOG
                AND c.TABLE_SCHEMA = v.TABLE_SCHEMA
                AND c.TABLE_NAME = v.TABLE_NAME
            WHERE NOT (v.TABLE_SCHEMA = 'dbo'
            AND v.TABLE_NAME IN ('syssegments', 'sysconstraints')
            AND SUBSTRING(CAST(SERVERPROPERTY('productversion') AS varchar(20)), 1, 1) = 8)) AS [Extent8]
            ON [Extent8].[Id] = [Extent7].[ColumnId]) AS [UnionAll2]
        ON (7 = [UnionAll2].[C1])
        AND ([Extent4].[Id] = [UnionAll2].[ConstraintId])
    WHERE [Extent4].[ConstraintType] = N'PRIMARY KEY') AS [Project5]
    ON [UnionAll1].[Id] = [Project5].[C1]
WHERE [Extent1].[Name] LIKE N'%'
