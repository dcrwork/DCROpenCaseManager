-- =============================================
-- Author:               Muddassar Latif
-- Create date: 20-04-2018
-- Description:          Get Instances tasks based on responsible can execute or not
-- =============================================
CREATE FUNCTION [dbo].[InstanceTasksAllEnabled]
(
	-- Add the parameters for the function here
	@Responsible NVARCHAR(100)
)
RETURNS TABLE
AS

            RETURN (
                       SELECT [EventId],
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
                              [InstanceId],
                              [SimulationId],
                              [GraphId],
                              [Name]  AS [ResponsibleName],
                              CASE Responsible
                                   WHEN @Responsible THEN CASE 
                                                               WHEN ([IsEnabled] = 1 AND [IsIncluded] = 1) THEN 
                                                                    1
                                                               ELSE 0
                                                          END
                                   ELSE 0
                              END     AS CanExecute,
                              [Description],
                              [Case],
                              [CaseLink],
                              [CaseTitle],
                              [IsUIEvent],
                              [UIEventValue],
                              [UIEventCssClass],
                              [Type],
                              [IsOverDue],
                              [DaysPassedDue],
                              [Modified],
                              [NotApplicable],
                              ActualIsPending,
                              ActualIsEnabled,
                              ActualIsExecuted,
                              ParentId
                       FROM   [dbo].[InstanceEvents]
                       WHERE  IsIncluded = 1
                              AND IsEnabled = 1
                   )