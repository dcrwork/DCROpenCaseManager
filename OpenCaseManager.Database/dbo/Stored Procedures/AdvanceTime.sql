-- =============================================
-- Author:               Muddassar Latif
-- Create date: 14092018
-- Description:          Get time for instances whose time
--                                               should advance
-- =============================================
CREATE PROCEDURE [dbo].[AdvanceTime]
-- Add the parameters for the stored procedure here
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here	
	BEGIN TRY
		DECLARE @Now AS DATETIME2(7)
		DECLARE @InstanceId INT
		DECLARE @InstanceXML XML
		DECLARE @GraphId INT
		DECLARE @SimId INT
		DECLARE @NextDeadline AS DATETIME2(7)
		DECLARE @NextDelay AS DATETIME2(7)
		DECLARE @NextTime AS DATETIME2(7)
		
		SET @Now = GETUTCDATE()		
		
		CREATE TABLE #InstancesTime
		(
			Id           INT NOT NULL,
			DCRXML       XML,
			NextTime     DATETIME2(7),
			GraphId      INT,
			SimId        INT
		) 
		
		DECLARE TimeCursor CURSOR  
		FOR
		    SELECT i.Id,
		           i.DCRXML,
		           i.NextDelay,
		           i.NextDeadline,
		           i.GraphId,
		           i.SimulationId
		    FROM   Instance AS i
		    WHERE  (
		               NextDelay < @now
		               OR NextDeadline < @Now
		               AND NextDeadline > CurrentTime
		           )
		
		OPEN TimeCursor
		FETCH TimeCursor INTO @InstanceId,@InstanceXML,@NextDelay,@NextDeadline,
		@GraphId,
		@SimId
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    --IF @NextDelay IS NOT NULL 
		    BEGIN
		    	IF @NextDelay IS NULL
		    	    SET @NextDelay = @Now
		    	
		    	IF @NextDelay < @Now
		    	    SET @NextTime = @NextDelay
		    	ELSE
		    	    SET @NextTime = @Now
		    	
		    	INSERT INTO #InstancesTime
		    	  (
		    	    Id,
		    	    DCRXML,
		    	    NextTime,
		    	    GraphId,
		    	    SimId
		    	  )
		    	VALUES
		    	  (
		    	    @InstanceId,
		    	    @InstanceXML,
		    	    @NextTime,
		    	    @GraphId,
		    	    @SimId
		    	  )
		    END
		    
		    FETCH TimeCursor INTO @InstanceId,@InstanceXML,@NextDelay,@NextDeadline,
		    @GraphId,
		    @SimId
		END
		CLOSE TimeCursor
		DEALLOCATE TimeCursor
		
		SELECT *
		FROM   #InstancesTime
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
		    'AdvanceTime()',
		    ERROR_MESSAGE()
		  )
	END CATCH
END