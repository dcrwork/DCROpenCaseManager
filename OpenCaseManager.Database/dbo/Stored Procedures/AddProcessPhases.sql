-- =============================================
-- Author:		Muddassar Latif
-- Create date: 13-07-2018
-- Description:	Add Process Phases
-- =============================================
CREATE PROCEDURE [dbo].[AddProcessPhases]
-- Add the parameters for the stored procedure here
	@ProcessId INT,
	@PhaseXml XML
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here
	
	BEGIN TRY
		DECLARE @SequenceNumber VARCHAR(100)
		DECLARE @Title VARCHAR(100)
		
		DECLARE ABC CURSOR  
		FOR
		    SELECT c.p.value('@sequence', 'nvarchar(100)') SequenceNumber,
		           c.p.value('.', 'nvarchar(100)') Title
		    FROM   @PhaseXml.nodes('//phases/phase') AS c(p)
		
		OPEN ABC
		FETCH ABC INTO @SequenceNumber,@Title
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    IF EXISTS(
		           SELECT *
		           FROM   ProcessPhase AS pp
		           WHERE  pp.ProcessId = @ProcessId
		                  AND pp.SequenceNumber = @SequenceNumber
		       )
		    BEGIN
		        UPDATE ProcessPhase
		        SET    Title         = @title
		        WHERE  ProcessId     = @ProcessId
		               AND SequenceNumber = @SequenceNumber
		    END
		    ELSE
		    BEGIN
		        INSERT INTO ProcessPhase
		          (
		            -- Id -- this column value is auto-generated
		            ProcessId,
		            SequenceNumber,
		            Title
		          )
		        VALUES
		          (
		            @processId,
		            @sequenceNumber,
		            @title
		          )
		    END
		    
		    FETCH ABC INTO @SequenceNumber,@Title
		END
		CLOSE ABC
		DEALLOCATE ABC
		
		SELECT 1
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
		    'AddProcessPhases(' + CAST(@ProcessId AS NVARCHAR) + ', <xml>' + ')',
		    ERROR_MESSAGE(),
		    @PhaseXml
		  )
	END CATCH
END