-- =============================================
-- =============================================
-- Author: Daniel Guldberg Aaes, Louise Kahl Skafte
-- Create date: 24-04-2018
-- Description:	Get data about your children.
-- =============================================
CREATE FUNCTION [dbo].[GetDataForAllMyChildren]
(
	-- Add the parameters for the function here
	@userId NVARCHAR(100)
)
RETURNS TABLE
AS

	RETURN (
			SELECT C.[Name] as "ChildName", 
				   C.[Id] as "ChildId",
				   U.[Name] as "Responsible",

					  (SELECT MAX(NextDeadline)
					   FROM [Instance], 
					        [InstanceExtension]
					   WHERE Instance.Id = InstanceExtension.InstanceId
							AND InstanceExtension.ChildId = C.Id
					  ) AS "NextDeadline",
					  (SELECT (COUNT(DISTINCT E.Id)) as "Activities"
						FROM dbo.Event as E, dbo.InstanceExtension as IE
						WHERE E.IsEnabled = 1
							AND E.InstanceId = IE.InstanceId
							AND IE.ChildId = C.Id
							AND E.Responsible = C.Responsible
							AND IE.ChildId = ChildId
					   ) AS SumOfEvents
					  
			   FROM [dbo].[Child] AS C 
					INNER JOIN [dbo].[User] AS U ON C.Responsible = U.Id



			   WHERE C.[Responsible] = @userId
	       )
