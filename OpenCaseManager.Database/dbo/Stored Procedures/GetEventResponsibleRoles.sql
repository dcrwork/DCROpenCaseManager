-- =============================================
-- Author:		Muddassar Latif
-- Create date: 10-01-2020
-- Description:	Add Events Role
-- =============================================
CREATE PROCEDURE [dbo].[GetEventResponsibleRoles] 
-- Add the parameters for the stored procedure here
	@InstanceId INT,
	@EventId INT,
	@ResponsibleId INT,
	@EventsXML XML
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @Id             NVARCHAR(1000),
	        @role           NVARCHAR(1000),
	        @DCREventId     NVARCHAR(1000)
	
	BEGIN TRY
		SELECT @DCREventId = e.EventId,
		       @role = e.Roles
		FROM   [Event] AS e
		WHERE  e.InstanceId = @InstanceId
		       AND e.Id = @EventId
		
		IF (@role IS NULL)
		BEGIN
		    DECLARE ABC CURSOR  
		    FOR
		        SELECT *
		        FROM   (
		                   SELECT c.p.value('@id', 'varchar(1000)') AS [Id],
		                          c.p.value('@roles', 'varchar(1000)') [role]
		                   FROM   @EventsXML.nodes('//events/event') AS c(p)
		               ) A
		        WHERE  [Id] = @DCREventId
		    
		    OPEN ABC
		    FETCH ABC INTO @Id,@role
		    WHILE @@FETCH_STATUS = 0
		    BEGIN
		        UPDATE [Event]
		        SET    Roles = @role
		        WHERE  Id = @EventId
		        
		        FETCH ABC INTO @Id,@role
		    END 
		    
		    CLOSE ABC
		    DEALLOCATE ABC
		END
		
		SELECT ir.[Role]
		FROM   InstanceRole AS ir
		WHERE  ir.InstanceId = @InstanceId
		       AND ir.UserId = @ResponsibleId
		       AND ir.[Role] IN (SELECT value FROM dbo.fn_Split(@role,',') AS fs)
	END TRY
	BEGIN CATCH
		INSERT INTO LOG
		  (
		    Logged,
		    [LEVEL],
		    [MESSAGE],
		    Exception,
		    [XML]
		  )
		VALUES
		  (
		    GETDATE(),
		    1,
		    'AddEventRoles(' + CAST(@InstanceId AS NVARCHAR) + CAST(@EventId AS NVARCHAR) 
		    + ', <xml> ' 
		    + ')',
		    ERROR_MESSAGE(),
		    @EventsXML
		  )
	END CATCH
END