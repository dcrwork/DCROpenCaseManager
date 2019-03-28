-- =============================================
-- Author:		JoeJoe and T-Boi
-- Create date: 27-03-2019
-- Description:	Get history of past executed tasks for an instance
-- =============================================
CREATE FUNCTION [dbo].[JournalHistoryForASingleInstance](@InstanceId NVARCHAR(100))
RETURNS TABLE
AS
	RETURN (
	           SELECT [CreationDate],
					  [InstanceEvents].[EventId],
	                  [EventTitle],
	                  [Responsible],
	                  [Due],
	                  [EventIsOpen],
	                  [InstanceIsOpen],
	                  [IsEnabled],
	                  [IsPending],
	                  [IsIncluded],
	                  [IsExecuted],
	                  [EventType],
	                  [InstanceEvents].[InstanceId],
	                  [SimulationId],
	                  [GraphId],
	                  [Name]  AS [ResponsibleName],
	                  [Description],
	                  [Case],
	                  [CaseLink],
	                  [CaseTitle],
	                  [IsUIEvent],
	                  [UIEventValue],
	                  [UIEventCssClass],
	                  [InstanceEvents].[Type]
	           FROM   [dbo].[InstanceEvents], [dbo].[JournalHistory]
	           WHERE  [dbo].[InstanceEvents].[EventId] = [dbo].[JournalHistory].[EventId]
					  AND [dbo].[InstanceEvents].[InstanceId] = [dbo].[JournalHistory].[InstanceId]
					  AND IsIncluded = 1
	                  AND (IsEnabled = 1 OR IsPending = 1)
					  AND [dbo].[InstanceEvents].[InstanceId]=@InstanceId
	       )