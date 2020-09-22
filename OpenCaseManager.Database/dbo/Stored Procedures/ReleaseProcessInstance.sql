-- =============================================
-- Author:          Muddassar Latif
-- Create date:		01-11-2019
-- Description:     Release a process after approval
-- =============================================
CREATE PROCEDURE [dbo].[ReleaseProcessInstance]
-- Add the parameters for the stored procedure here
	@instanceId INT,
	@ProcessPhaseXML XML = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY
		DECLARE @GraphId INT
		
		SELECT @GraphId = ph.GraphId
		FROM   ProcessHistory AS ph
		WHERE  ph.InstanceId = @instanceId
		
		IF NOT EXISTS(
		       SELECT 1
		       FROM   Process AS p
		       WHERE  p.GraphId = @GraphId
		   )
		BEGIN
		    INSERT INTO Process
		      (
		        -- Id -- this column value is auto-generated
		        GraphId,
		        Title,
		        ForeignIntegration,
		        DCRXML,
		        [Status],
		        Created,
		        Modified,
		        OnFrontPage,
		        [Guid],
		        CreateInstance,
		        EventId,
		        InstanceGuid,
		        MajorVersionId,
		        MajorVersionTitle,
		        MajorVerisonDate,
		        ReleaseDate,
		        [Owner]
		      )
		    SELECT -- Id -- this column value is auto-generated
		           GraphId,
		           Title,
		           ForeignIntegration,
		           DCRXML,
		           [Status],
		           Created,
		           Modified,
		           OnFrontPage,
		           [Guid],
		           CreateInstance,
		           EventId,
		           InstanceGuid,
		           MajorVersionId,
		           MajorVersionTitle,
		           MajorVerisonDate,
		           GETDATE(),
		           [Owner]
		    FROM   ProcessHistory AS ph
		    WHERE  ph.InstanceId = @instanceId
		END
		ELSE
		BEGIN
		    UPDATE Process
		    SET    DCRXML                = ph.DCRXML,
		           Modified              = GETDATE(),
		           MajorVersionId        = ph.MajorVersionId,
		           MajorVersionTitle     = ph.MajorVersionTitle,
		           MajorVerisonDate      = ph.MajorVerisonDate,
		           ReleaseDate           = GETDATE()
		    FROM   ProcessHistory AS ph
		    WHERE  Process.GraphId = @GraphId
		           AND ph.InstanceId = @instanceId
		END
		
		UPDATE ProcessHistory
		SET    [State] = -1
		WHERE  GraphId = @GraphId
		       AND (InstanceId IS NULL OR InstanceId <> @InstanceId)
		
		UPDATE ProcessHistory
		SET    ReleaseDate = GETDATE(),
		       [State] = 1
		WHERE  InstanceId = @instanceId
		
		DECLARE @ProcessId INT
		SELECT @ProcessId = Id
		FROM   Process AS p
		WHERE  p.GraphId = @GraphId
		
		-- add/update process phases
		EXEC AddProcessPhases
		     @ProcessId = @ProcessId,
		     @PhaseXml = @ProcessPhaseXML
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
		    'ReleaseProcessInstance(' + CAST(@InstanceId AS NVARCHAR) + 
		    ', <xml>' + ')',
		    ERROR_MESSAGE(),
		    @ProcessPhaseXML
		  )
	END CATCH
END