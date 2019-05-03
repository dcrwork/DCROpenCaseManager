-- =============================================
-- Author: Thais Kure Corneliusen  
-- Create date: 17-04-2019
-- Description:	Get pending activities
-- =============================================
CREATE FUNCTION [dbo].[PendingActivities](@UserId INT)
RETURNS TABLE
AS
	RETURN (
	           SELECT E.InstanceId, I.Title AS InstanceTitle, E.Title AS EventTitle, IE.ChildId, S.Navn
				FROM [dbo].[Instance] AS I, [dbo].[Event] AS E LEFT JOIN [dbo].[InstanceExtension] AS IE
					ON E.InstanceId = IE.InstanceId LEFT JOIN [dbo].[Child] AS C
					ON C.Id = IE.ChildId, [dbo].[StamdataChild] AS S

				WHERE I.Id = E.InstanceId
					AND E.IsPending = 1
					AND E.IsEnabled = 1
					AND E.isOpen = 1
					AND E.Responsible = @UserId
					AND C.Id = S.ChildId
			)