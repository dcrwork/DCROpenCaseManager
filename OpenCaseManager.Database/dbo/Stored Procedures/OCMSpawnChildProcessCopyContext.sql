-- =============================================
-- Author:               Morten Marquard
-- Create date: May 22nd, 2019
-- Description:          Copies the context of the to the new instance process
-- =============================================
CREATE PROCEDURE [dbo].[OCMSpawnChildProcessCopyContext]
-- Add the parameters for the stored procedure here
	@InstanceId
AS
	INT,
	@ParentInstanceId AS INT
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here
	
	BEGIN TRY
		INSERT INTO InstanceExtension
		  (
		    InstanceId,
		    [Year],
		    [Employee],
		    Childid
		  )
		SELECT @InstanceId,
		       [Year],
		       [Employee],
		       ChildId
		FROM   InstanceExtension
		WHERE  InstanceId = @ParentInstanceId
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
		    'OCMSpawnChildProcessCopyContext(' + CAST(@InstanceId AS NVARCHAR) + ',' + CAST(@ParentInstanceId AS NVARCHAR) 
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END