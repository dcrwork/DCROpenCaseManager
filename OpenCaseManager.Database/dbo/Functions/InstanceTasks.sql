-- =============================================
-- Author: Jonathan E. Mogensen, Thais Kure Corneliusen  
-- Create date: 20-04-2018
-- Description:          Get Instances tasks based on responsible can execute or not
-- =============================================
CREATE FUNCTION [dbo].[InstanceTasks]
(
	-- Add the parameters for the function here
	@Responsible NVARCHAR(100)
)
RETURNS TABLE
AS
            RETURN (
                       SELECT [Event].[Id] AS "TrueEventId",
                              [InstanceEvents].[EventId],
                              [EventTitle],
                              [InstanceEvents].[Responsible],
                              [InstanceEvents].[Due],
                              [EventIsOpen],
                              [InstanceIsOpen],
                              [InstanceEvents].[IsEnabled],
                              [InstanceEvents].[IsPending],
                              [InstanceEvents].[IsIncluded],
                              [InstanceEvents].[IsExecuted],
                              [InstanceEvents].[EventType],
                              [InstanceEvents].[InstanceId],
                              [SimulationId],
                              [GraphId],
                              [Name]  AS [ResponsibleName],
                              CASE [InstanceEvents].Responsible
                                   WHEN @Responsible THEN CASE 
                                                               WHEN (
                                                                        [InstanceEvents].[IsEnabled] 
                                                                        = 1
                                                                        AND 
                                                                            [InstanceEvents].[IsIncluded] 
                                                                            = 1
                                                                    ) THEN 1
                                                               ELSE 0
                                                          END
                                   ELSE 0
                              END     AS CanExecute,
                              [InstanceEvents].[Description],
                              [Case],
                              [CaseLink],
                              [CaseTitle],
                              [IsUIEvent],
                              [UIEventValue],
                              [UIEventCssClass],
                              [InstanceEvents].[Type],
                              CASE [InstanceEvents].Responsible
                                   WHEN @Responsible THEN 1
                                   ELSE 0
                              END     AS [MyTask],
                              [IsOverDue],
                              [DaysPassedDue],
                              [InstanceEvents].[Modified] AS Modified,
                              [InstanceEvents].[NotApplicable] AS NotApplicable,
                              [dbo].[InstanceEvents].ActualIsPending,
                              [dbo].[InstanceEvents].ActualIsEnabled,
                              [dbo].[InstanceEvents].ActualIsExecuted,
                              [dbo].[InstanceEvents].ParentId,
							  [dbo].[InstanceEvents].Roles
                       FROM   [dbo].[InstanceEvents],
                              [dbo].[Event]
                       WHERE  [Event].[InstanceId] = [InstanceEvents].[InstanceId]
                              AND [Event].[EventId] = [InstanceEvents].[EventId]
                              AND [InstanceEvents].IsIncluded = 1
                              AND (
                                      [InstanceEvents].IsEnabled = 1
                                      OR [InstanceEvents].IsPending = 1
                                  )
                   )
