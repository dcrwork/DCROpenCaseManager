-- =============================================
-- Author: Thais Kure Corneliusen  
-- Create date: 17-04-2019
-- Description:	Get pending activities
-- =============================================
CREATE FUNCTION [dbo].[PendingActivities](@UserId INT)
RETURNS TABLE
AS
	RETURN (
	           SELECT E.InstanceId, E.Title, IE.ChildId
				FROM [dbo].[Event] AS E LEFT JOIN [dbo].[InstanceExtension] AS IE
					ON E.InstanceId = IE.InstanceId
				WHERE E.IsPending = 1
					AND E.IsEnabled = 1
					AND E.isOpen = 1
					AND E.Responsible = @UserId
			)