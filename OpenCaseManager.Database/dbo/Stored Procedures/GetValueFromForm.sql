-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetValueFromForm] 
-- Add the parameters for the stored procedure here
	@FormName VARCHAR(500),
	@EventName VARCHAR(500),
	@InstanceId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here
	
	BEGIN TRY
		SELECT m.c.value('@value', 'nvarchar(1000)')
		FROM   Instance AS i
		       CROSS APPLY i.DCRXML.nodes('//events/event') AS x(t)
		OUTER APPLY x.t.nodes('.//globalStore/variable') AS m(c)
		WHERE  i.Id = @InstanceId
		       AND x.t.value('@id', 'nvarchar(1000)') = @FormName
		       AND m.c.value('@id', 'nvarchar(1000)') = @EventName
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
		    'GetValueFromForm(' + @FormName + ',' + @EventName + ',' + CAST(@InstanceId AS NVARCHAR) + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END