-- =============================================
-- Author: Morten Marquard
-- Create date: 11-05-2018
-- Description:          Get data about children.
-- =============================================
CREATE FUNCTION [dbo].[GetDataForChildren]
(
	-- Add the parameters for the function here
	@childList NVARCHAR(1000)
)
RETURNS TABLE
AS
	RETURN (
	    SELECT B.ChildId,
	           MAX(a.nextdeadline)   AS NextDeadline,
	           COUNT(DISTINCT c.id)  AS SumOfEvents
	    FROM   [Instance] A
	           INNER JOIN [InstanceExtension] B
	                ON  A.Id = B.InstanceId
	           LEFT OUTER JOIN EVENT c
	                ON  c.InstanceId = a.id
	                AND c.IsEnabled = 1
	                AND a.IsOpen = 1
	                AND c.IsPending = 1
	    WHERE  B.[ChildId] IN (SELECT o.value
	                           FROM   dbo.fn_Split(@childList, ',') AS o)
	    GROUP BY
	           B.ChildId
	)