-- =============================================
-- Author:		Ahmed Mazher
-- Create date: 14052019
-- Description:	Get List Of Menu Items for Populating Menu Dynamically on the basis of DepartmentId or AlwaysVisible
-- =============================================
CREATE PROCEDURE [dbo].[GetMenuItems]
	@UserID INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY
		DECLARE @DepartmentID INT;
		SELECT @DepartmentID = [DepartmentId]
		FROM   [dbo].[User]
		WHERE  Id = @UserID
		
		SELECT MI.[Id],
		       MI.[SequenceNo],
		       MI.[DisplayTitle],
		       MI.[RelativeURL],
		       MI.[AlwaysVisible]
		FROM   [dbo].MenuItem MI
		       LEFT JOIN MenuItemAccess MIA
		            ON  MI.Id = MIA.MenuItemId
		WHERE  MIA.DepartmentId = (
		           SELECT @DepartmentID
		       )
		       OR  MI.AlwaysVisible = 1
		ORDER BY
		       MI.[SequenceNo] DESC
	END TRY
	BEGIN CATCH
		INSERT INTO LOG
		  (
		    Logged,
		    [LEVEL],
		    [MESSAGE],
		    Exception
		  )
		VALUES
		  (
		    GETDATE(),
		    1,
		    'GetMenuItems(' + CAST(@UserID AS NVARCHAR) + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END
GO
