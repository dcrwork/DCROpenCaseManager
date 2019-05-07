-- =============================================
-- Author:		JoeJoe and T-Boi
-- Create date: 27-03-2019
-- Description:	Get history of past executed tasks for an instance
-- =============================================
CREATE FUNCTION [dbo].[JournalHistoryForASingleInstance](@InstanceId INT)
RETURNS TABLE
AS
	RETURN (
	           SELECT [EventDate],
					  [Event].[EventId],
					  [Event].[Id],
	                  [Event].[Title],
	                  [Responsible],
					  [User].[Name] AS "ResponsibleName",
	                  [Due],
	                  [Description],
	                  [IsEnabled],
	                  [IsPending],
	                  [IsIncluded],
	                  [IsExecuted],
	                  [EventType],
	                  [Event].[InstanceId],
	                  [JournalHistory].[Type]
	           FROM   [dbo].[Event], [dbo].[JournalHistory], [dbo].[User]
	           WHERE  [dbo].[Event].[Id] = [dbo].[JournalHistory].[EventId]
					  AND [dbo].[Event].[InstanceId] = [dbo].[JournalHistory].[InstanceId]
					  AND [dbo].[User].[Id] = [dbo].[Event].[Responsible]
					  AND [dbo].[JournalHistory].[InstanceId]=@InstanceId
	       )
GO

