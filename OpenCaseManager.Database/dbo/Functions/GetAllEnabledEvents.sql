-- =============================================
-- =============================================
-- Author: Daniel Guldberg Aaes, Louise Kahl Skafte
-- Create date: 24-04-2018
-- Description: Returns a sum of all enabled acitivites there is bound
--				to a child.
-- =============================================
CREATE FUNCTION [dbo].[GetAllEnabledEvents]
(
	-- Add the parameters for the function here
	@childId NVARCHAR(100),
	@userId NVARCHAR(100),
	@instanceId NVARCHAR(100)

)
RETURNS TABLE
AS

	RETURN (
			SELECT (COUNT(DISTINCT E.Id)) as "Activities"
			FROM dbo.Event as E, dbo.InstanceExtension as IE
			WHERE E.IsEnabled = 1
				AND E.InstanceId = @instanceId
				AND E.Responsible = @userId
				AND IE.ChildId = @childId
	       )
		   

