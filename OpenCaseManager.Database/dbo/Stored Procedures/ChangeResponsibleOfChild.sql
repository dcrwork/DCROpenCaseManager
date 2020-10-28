-- =============================================
-- Author:               Morten Marquard
-- Create date: June 28th, 2019
-- Description:          Change responsible in OCM database
-- =============================================
CREATE PROCEDURE [dbo].[ChangeResponsibleOfChild]
-- Add the parameters for the stored procedure here
	@ChildId
AS
	INT,
	@InstanceId AS INT = 0, -- instance id in OCM Instance table
	@EventId AS INT = 0, -- internal eventid integer key for event
	@FromInitials AS NVARCHAR(50) = '',
	@ToInitials AS NVARCHAR(50)
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here
	
	BEGIN TRY
		DECLARE @Title AS NVARCHAR(200)
		DECLARE @Message AS NVARCHAR(1000)
		DECLARE @FromInitialsId AS INT
		DECLARE @FromInitialsName AS NVARCHAR(100)
		DECLARE @ToInitialsId AS INT
		DECLARE @ToInitialsName AS NVARCHAR(100)
		IF @EventId <> 0
		   AND @FromInitials = ''
		BEGIN
		    SELECT @FromInitialsId = Responsible
		    FROM   EVENT
		    WHERE  Id = @EventId
		    
		    SELECT @FromInitials = sAMAccountName
		    FROM   [User]
		    WHERE  Id = @FromInitialsId
		END
		
		SELECT @FromInitialsId = Id,
		       @FromInitialsName     = NAME
		FROM   [User]
		WHERE  SamAccountName        = @FromInitials
		
		SET @FromInitialsId = ISNULL(@FromInitialsId, 0)
		SET @FromInitialsName = ISNULL(@FromInitialsName, '')
		SELECT @ToInitialsId = Id,
		       @ToInitialsName     = NAME
		FROM   [User]
		WHERE  SamAccountName      = @ToInitials
		
		DECLARE @Id AS INT
		IF @EventId <> 0
		BEGIN
		    SELECT @Message = 'Ansvarlig for aktiviteten ' + Title +
		           ' ændret fra ' 
		           + @FromInitialsName + ' til ' + @ToInitialsName
		    FROM   EVENT
		    WHERE  Id = @EventId
		    
		    UPDATE EVENT
		    SET    Responsible              = @ToInitialsId
		    WHERE  Id                       = @EventId
		           AND (@FromInitialsId     = 0 OR Responsible = @FromInitialsId)
		END
		ELSE
		BEGIN
		    DECLARE ABC CURSOR  
		    FOR
		        SELECT A.Id
		        FROM   INSTANCE A
		               INNER JOIN InstanceExtension b
		                    ON  b.InstanceId = a.id
		        WHERE  b.ChildId = @ChildId
		               AND (@InstanceId = 0 OR a.id = @InstanceId)
		    
		    OPEN ABC
		    FETCH ABC INTO @Id
		    WHILE @@FETCH_STATUS = 0
		    BEGIN
		        -- Update instance responsible
		        UPDATE Instance
		        SET    Responsible = @ToInitialsId
		        WHERE  Id = @Id
		               AND (Responsible = @FromInitialsId OR @FromInitialsId = 0)
		        -- Update instance role responsible - only if a from is provided
		        UPDATE InstanceRole
		        SET    UserId = @ToInitialsId
		        WHERE  Id = @Id
		               AND UserId = @FromInitialsId
		        -- Update event responsible - only if a from is provided - might be for a specific event
		        UPDATE [Event]
		        SET    Responsible = @ToInitialsId
		        WHERE  InstanceId = @Id
		               AND Responsible = @FromInitialsId
		        
		        FETCH ABC INTO @Id
		    END
		    CLOSE ABC
		    DEALLOCATE ABC
		    IF @InstanceId = 0
		        SET @Message = 'Ansvarlig for barnet ændret fra ' + @FromInitialsName 
		            + ' til ' + @ToInitialsName
		    ELSE
		        SELECT @Message = 'Ansvarlig for indsatsen ' + Title +
		               ' ændret fra ' + @FromInitialsName + ' til ' + @ToInitialsName
		        FROM   Instance
		        WHERE  Id = @InstanceId
		END
		SET @Title = 'Ændr ansvarlig fra ' + @FromInitials + ' til ' + @ToInitials
		SELECT @Title    AS Title,
		       @Message  AS [Message],
		       @ChildId  AS ChildId
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
		    'ChangeResponsibleOfChild(' + CAST(@ChildId AS NVARCHAR) + ',' + 
		    CAST(@InstanceId AS NVARCHAR) + ',' + CAST(@EventId AS NVARCHAR) + 
		    ',' + @FromInitials + ',' + @ToInitials
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END