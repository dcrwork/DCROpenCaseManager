CREATE FUNCTION [dbo].[AdjunktActivities]
(
	@AdjunktId INT
)
RETURNS TABLE
AS
	RETURN (
	    SELECT 
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