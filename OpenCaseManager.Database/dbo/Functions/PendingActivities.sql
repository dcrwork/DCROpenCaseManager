-- =============================================
-- Author: Thais Kure Corneliusen  
-- Create date: 17-04-2019
-- Description:	Get pending child activities
-- 10-05-2019 - Morten Marquard - adjusted to work with child
-- =============================================
CREATE FUNCTION [dbo].[PendingActivities]
(
	@UserId INT
)
RETURNS TABLE
AS

	RETURN (
	           SELECT E.InstanceId,
	                  I.Title  AS InstanceTitle,
	                  E.Title  AS EventTitle,
	                  IE.ChildId,
	                  ISNULL(NULL, 'Har ikke navn på barn') AS NAME
	           FROM   [dbo].[Instance] AS I
	                  INNER JOIN [dbo].[Event] AS E
	                       ON  I.Id = E.InstanceId
	                  INNER JOIN [dbo].[InstanceExtension] AS IE
	                       ON  E.InstanceId = IE.InstanceId
	           WHERE  E.IsPending = 1
	                  AND E.IsEnabled = 1
	                  AND I.isOpen = 1
	                  AND E.Responsible = @UserId
	       )
GO


