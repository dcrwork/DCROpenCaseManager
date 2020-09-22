-- =============================================
-- Author:		Muddassar Latif
-- Create date: 05/12/2018
-- Description:	Add description from process to instances
-- =============================================
CREATE PROCEDURE [dbo].[AddInstanceDescription] 
-- Add the parameters for the stored procedure here
	@GraphId INT,
	@InstanceId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	-- Insert statements for procedure here
	SET NOCOUNT ON;
	
	BEGIN TRY
		DECLARE @description VARCHAR(MAX)
		
		SELECT @description = r.value('(graphDetails)[1]', 'varchar(max)')
		FROM   Process AS p
		       CROSS APPLY DCRXML.nodes('//specification/resources/custom') AS x(r)
		WHERE  p.GraphId = @GraphId
		
		SELECT @description = REPLACE(
		           REPLACE(@description, CHAR(13), '</br>'),
		           CHAR(10),
		           '</br>'
		       )
		
		UPDATE Instance
		SET    [Description]     = @description
		WHERE  Id                = @InstanceId
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
		    'AddInstanceDescription(' + CAST(@GraphId AS NVARCHAR) + ',' + CAST(@InstanceId AS NVARCHAR) 
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END