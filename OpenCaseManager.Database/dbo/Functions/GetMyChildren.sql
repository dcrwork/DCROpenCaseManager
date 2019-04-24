-- =============================================
-- =============================================
-- Author: Daniel Guldberg Aaes, Louise Kahl Skafte
-- Create date: 20-04-2018
-- Description:	Get Instances tasks based on responsible can execute or not
-- =============================================
CREATE FUNCTION [dbo].[GetMyChildren]
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
					  ) AS "NextDeadline"
					  
			   FROM [dbo].[Child] AS C INNER JOIN [dbo].[User] AS U ON C.Responsible = U.Id


			   WHERE C.[Responsible] = @userId
	       )
