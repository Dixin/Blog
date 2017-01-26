CREATE EVENT SESSION [Queries] ON DATABASE -- ON SERVER for SQL Server on-premise database.
ADD EVENT sqlserver.begin_tran_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.commit_tran_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.error_reported(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.rollback_tran_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.rpc_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.sp_statement_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.sql_batch_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text)), 
ADD EVENT sqlserver.sql_statement_completed(
    ACTION(sqlserver.client_app_name, sqlserver.client_connection_id, sqlserver.client_hostname, sqlserver.client_pid, sqlserver.database_name, sqlserver.request_id, sqlserver.session_id, sqlserver.sql_text))
ADD TARGET package0.ring_buffer(SET max_events_limit = (100)) -- Most recent 100 events.
WITH (STARTUP_STATE = OFF)
GO

ALTER EVENT SESSION [Queries] ON DATABASE -- ON SERVER for SQL Server on-premise database.
    STATE = START;
GO

DECLARE @target_data XML = 
(SELECT CONVERT(XML, [targets].[target_data])
FROM sys.dm_xe_database_session_targets AS [targets] -- sys.dm_xe_session_targets for SQL Server on-premise database.
INNER JOIN sys.dm_xe_database_sessions AS [sessions] -- sys.dm_xe_sessions for SQL Server on-premise database.
    ON [sessions].[address] = [targets].[event_session_address]
WHERE [sessions].[name] = N'Queries');

SELECT
    @target_data.value('(RingBufferTarget/@truncated)[1]', 'bigint') AS [truncated],
    @target_data.value('(RingBufferTarget/@processingTime)[1]', 'bigint') AS [processingTime],
    @target_data.value('(RingBufferTarget/@totalEventsProcessed)[1]', 'bigint') AS [totalEventsProcessed],
    @target_data.value('(RingBufferTarget/@eventCount)[1]', 'bigint') AS [eventCount],
    @target_data.value('(RingBufferTarget/@droppedCount)[1]', 'bigint') AS [droppedCount],
    @target_data.value('(RingBufferTarget/@memoryUsed)[1]', 'bigint') AS [memoryUsed];

SELECT
	[event].value('@timestamp[1]', 'datetime') AS [timestamp],
	[event].value('(action[@name="client_hostname"]/value)[1]', 'nvarchar(MAX)') AS [client_hostname],
	[event].value('(action[@name="client_pid"]/value)[1]', 'bigint') AS [client_pid],
	[event].value('(action[@name="client_connection_id"]/value)[1]', 'uniqueidentifier') AS [client_connection_id],
	[event].value('(action[@name="session_id"]/value)[1]', 'bigint') AS [session_id],
	[event].value('(action[@name="request_id"]/value)[1]', 'bigint') AS [request_id],
	[event].value('(action[@name="database_name"]/value)[1]', 'nvarchar(MAX)') AS [database_name],
	[event].value('@name[1]', 'nvarchar(MAX)') AS [name],
	[event].value('(data[@name="duration"]/value)[1]', 'bigint') AS [duration],
	[event].value('(data[@name="result"]/text)[1]', 'nvarchar(MAX)') AS [result],
	[event].value('(data[@name="row_count"]/value)[1]', 'bigint') AS [row_count],
	[event].value('(data[@name="cpu_time"]/value)[1]', 'bigint') as [cpu_time],
	[event].value('(data[@name="logical_reads"]/value)[1]', 'bigint') as [logical_reads],
	[event].value('(data[@name="physical_reads"]/value)[1]', 'bigint') as [physical_reads],
	[event].value('(data[@name="writes"]/value)[1]', 'bigint') as [writes],
	[event].value('(action[@name="sql_text"]/value)[1]', 'nvarchar(MAX)') AS [sql_text],
	[event].value('(data[@name="statement"]/value)[1]', 'nvarchar(MAX)') AS [statement],
	[event].value('(data[@name="error_number"]/value)[1]', 'bigint') AS [error_number],
	[event].value('(data[@name="message"]/value)[1]', 'nvarchar(MAX)') AS [message]
FROM @target_data.nodes('//RingBufferTarget/event') AS [Rows]([event])
WHERE [event].value('(action[@name="client_app_name"]/value)[1]', 'nvarchar(MAX)') = N'Core .Net SqlClient Data Provider' -- N'.Net SqlClient Data Provider' for .NET Framework.
ORDER BY [timestamp];
