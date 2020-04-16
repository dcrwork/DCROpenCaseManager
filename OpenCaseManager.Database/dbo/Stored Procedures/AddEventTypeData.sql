-- =============================================
-- Author:                Muddassar Latif
-- Create date: 10-08-2018
-- Description:          Get parametes from graph xml and update events
-- =============================================
CREATE PROCEDURE [dbo].[AddEventTypeData]
-- Add the parameters for the stored procedure here
	@InstanceId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY
		-- Insert statements for procedure here
		DECLARE @EventXML XML
		DECLARE @Id NVARCHAR(100)
		DECLARE @EventTypeData XML
		
		SELECT @EventXML = p.DCRXML
		FROM   Process AS p
		WHERE  p.GraphId = (
		           SELECT i.GraphId
		           FROM   Instance AS i
		           WHERE  i.Id = @InstanceId
		       )
		
		DECLARE ABC CURSOR  
		FOR
		    SELECT *
		    FROM   (
		               SELECT c.p.value('@id', 'varchar(100)') AS Id,
		                      c.p.query('./custom/eventTypeData/parameter')
		                      eventTypedata
		               FROM   @EventXML.nodes('//resources//event') AS c(p)
		           ) A
		    WHERE  eventTypedata.exist('*') > 0
		
		OPEN ABC
		FETCH ABC INTO @Id,@EventTypeData
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    UPDATE [Event]
		    SET    EventTypeData = @EventTypeData
		    WHERE  InstanceId = @InstanceId
		           AND (EventId = @Id OR EventId LIKE '%].' + @Id ESCAPE ']')
		    
		    
		    FETCH ABC INTO @Id,@EventTypeData
		END 
		
		CLOSE ABC
		DEALLOCATE ABC
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
		    'AddEventTypeData(' + CAST(@InstanceId AS NVARCHAR) + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END