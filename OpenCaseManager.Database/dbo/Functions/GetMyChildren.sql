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
			   SELECT *
			   FROM [dbo].[Child]
			   WHERE [Child].[Responsible] = @userId           
	       )
