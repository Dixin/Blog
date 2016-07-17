DBCC FREEPROCCACHE

-- Shows query plans including itself.
SELECT 
	sys.syscacheobjects.cacheobjtype, 
	sys.dm_exec_cached_plans.usecounts, 
	sys.syscacheobjects.[sql],
	sys.dm_exec_query_plan.query_plan
FROM sys.syscacheobjects
INNER JOIN sys.dm_exec_cached_plans ON sys.syscacheobjects.bucketid = sys.dm_exec_cached_plans.bucketid
CROSS APPLY sys.dm_exec_query_plan(sys.dm_exec_cached_plans.plan_handle)

-- Shows query plans excluding itself.
SELECT
	sys.syscacheobjects.cacheobjtype, 
	sys.dm_exec_query_stats.execution_count,
	sys.syscacheobjects.sql,
	sys.dm_exec_query_plan.query_plan
FROM sys.dm_exec_query_stats
INNER JOIN sys.dm_exec_cached_plans ON sys.dm_exec_query_stats.plan_handle = sys.dm_exec_cached_plans.plan_handle
INNER JOIN sys.syscacheobjects ON sys.syscacheobjects.bucketid = sys.dm_exec_cached_plans.bucketid
CROSS APPLY sys.dm_exec_query_plan(sys.dm_exec_query_stats.plan_handle)

-- Equivalent to:
SELECT
	sys.syscacheobjects.cacheobjtype, 
	sys.dm_exec_query_stats.execution_count,
	sys.dm_exec_sql_text.text,
	sys.dm_exec_query_plan.query_plan
FROM sys.dm_exec_query_stats
INNER JOIN sys.dm_exec_cached_plans ON sys.dm_exec_query_stats.plan_handle = sys.dm_exec_cached_plans.plan_handle
INNER JOIN sys.syscacheobjects ON sys.syscacheobjects.bucketid = sys.dm_exec_cached_plans.bucketid
CROSS APPLY sys.dm_exec_query_plan(sys.dm_exec_query_stats.plan_handle)
CROSS APPLY sys.dm_exec_sql_text(sys.dm_exec_query_stats.sql_handle)

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE (LEN([Extent1].[Name])) >= 1

SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE (LEN([Extent1].[Name])) >= 10

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE (LEN([Extent1].[Name])) >= @p__linq__0',N'@p__linq__0 int',@p__linq__0=1

exec sp_executesql N'SELECT 
    [Extent1].[ProductCategoryID] AS [ProductCategoryID], 
    [Extent1].[Name] AS [Name]
    FROM [Production].[ProductCategory] AS [Extent1]
    WHERE (LEN([Extent1].[Name])) >= @p__linq__0',N'@p__linq__0 int',@p__linq__0=10
