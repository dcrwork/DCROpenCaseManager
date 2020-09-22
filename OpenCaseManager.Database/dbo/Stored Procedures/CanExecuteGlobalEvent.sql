-- =============================================
-- Author:               Morten Marquard
-- Create date: May 24th, 2019
-- Description:          Check if a global event can be executed
-- =============================================
CREATE PROCEDURE [dbo].[CanExecuteGlobalEvent]
-- Add the parameters for the stored procedure here
	@ChildId
AS
	INT,
	@EventId AS NVARCHAR(100)
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here
	
	BEGIN TRY
		DECLARE @Message AS NVARCHAR(200)
		IF EXISTS (
		       SELECT *
		       FROM   Instance A
		              INNER JOIN InstanceExtension B
		                   ON  b.InstanceId = a.Id
		              INNER JOIN [event] c
		                   ON  c.InstanceId = a.id
		       WHERE  B.ChildId = @ChildId
		              AND A.NextDelay < GETUTCDATE()
		   )
		BEGIN
		    SET @Message = 'Cannot execute event as delay in the past block'
		END
		ELSE
		BEGIN
		    DECLARE @Id AS INT
		    
		    SELECT @Id = MIN(A.Id)
		    FROM   Instance A
		           INNER JOIN InstanceExtension B
		                ON  b.InstanceId = a.Id
		           INNER JOIN [event] c
		                ON  c.InstanceId = a.id
		    WHERE  B.ChildId = @ChildId
		           AND c.EventId = @EventId
		           AND c.IsIncluded = 1
		           AND NOT c.IsEnabled = 1
		    
		    SET @Id = ISNULL(@Id, 0)
		    IF @Id > 0
		    BEGIN
		        SELECT @Message = 'Kan ikke udføre aktiviteten "' + ISNULL(b.Title, b.EventId) 
		               + '" da den er blokeret i indsatsen "' + a.Title + '"' +
		               ISNULL(' ' + a.CaseNoForeign, '')
		        FROM   Instance A
		               INNER JOIN [event] b
		                    ON  b.InstanceId = a.id
		        WHERE  a.Id = @Id
		               AND b.EventId = @Eventid
		    END
		    ELSE
		    BEGIN
		        SET @Message = ''
		    END
		END
		
		SELECT B.InstanceId,
		       A.Title,
		       C.IsIncluded,
		       C.IsEnabled,
		       @Message  AS [Message],
		       a.InternalCaseID,
		       a.CaseNoForeign,
		       a.CaseLink,
		       c.Description,
		       c.Title   AS EventTitle,
		       c.EventId,
		       A.GraphId,
		       A.SimulationId,
		       c.Id      AS TrueEventId
		FROM   Instance A
		       INNER JOIN InstanceExtension B
		            ON  b.InstanceId = a.Id
		       INNER JOIN [event] c
		            ON  c.InstanceId = a.id
		WHERE  B.ChildId = @ChildId
		       AND c.EventId = @EventId
		       AND c.IsIncluded = 1
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
		    'CanExecuteGlobalEvent(' + CAST(@ChildId AS NVARCHAR) + ',' + CAST(@EventId AS NVARCHAR) 
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END