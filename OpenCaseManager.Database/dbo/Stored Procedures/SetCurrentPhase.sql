-- =============================================
-- Author:               Muddassar Latif
-- Create date: 23-07-2018
-- Description:          Set Current phase of an instance from events
-- =============================================
CREATE PROCEDURE [dbo].[SetCurrentPhase]
-- Add the parameters for the stored procedure here
	@instanceId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY
		DECLARE @phaseSequence INT = NULL
		DECLARE @phaseId INT
		-- Insert statements for procedure here
		IF EXISTS(
		       SELECT *
		       FROM   [Event] AS e
		       WHERE  e.InstanceId = @instanceId
		              AND e.IsPending = 1
		              AND e.IsEnabled = 1
		              AND e.PhaseId <> ''
		   )
		BEGIN
		    SELECT @phaseSequence = MIN(CAST(a.value AS INT))
		    FROM   [Event] AS e
		           OUTER APPLY dbo.fn_split(phaseId, ',') AS a
		    WHERE  e.InstanceId = @instanceId
		           AND e.IsPending = 1
		           AND e.IsEnabled = 1
		           AND e.PhaseId <> ''
		           AND ISNUMERIC(a.value) = 1
		               --print 'min ' +cast(@phaseSequence as nvarchar)
		END
		ELSE
		BEGIN
		    SELECT @phaseSequence = MAX(CAST(a.value AS INT))
		    FROM   [Event] AS e
		           OUTER APPLY dbo.fn_split(phaseId, ',') AS a
		    WHERE  e.InstanceId = @instanceId
		           AND e.IsExecuted = 1
		           AND e.PhaseId <> ''
		           AND ISNUMERIC(a.value) = 1
		               --print 'max ' +cast(@phaseSequence as nvarchar)
		END
		IF @phaseSequence IS NOT NULL
		BEGIN
		    SELECT @phaseId = pp.Id
		    FROM   ProcessPhase         AS pp
		           INNER JOIN Process   AS p
		                ON  pp.ProcessId = p.Id
		           INNER JOIN Instance  AS i
		                ON  i.GraphId = p.GraphId
		    WHERE  i.Id = @instanceId
		           AND pp.sequenceNumber = @phaseSequence
		    
		    UPDATE Instance
		    SET    CurrentPhaseNo     = @phaseId
		    WHERE  Id                 = @instanceId
		END
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
		    'SetCurrentPhase(' + CAST(@InstanceId AS NVARCHAR) + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END