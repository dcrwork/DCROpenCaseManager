-- =============================================
-- Author:               Morten Marquard
-- Create date: May 24th, 2019
-- Description:          Check if a global event can be executed
-- =============================================
CREATE PROCEDURE [dbo].[GetGlobalEvents]
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
	
	BEGIN TRY
		SELECT B.InstanceId,
		       A.Title,
		       C.IsIncluded,
		       C.IsEnabled,
		       a.InternalCaseID,
		       a.CaseNoForeign,
		       a.CaseLink,
		       c.Description,
		       c.Title  AS EventTitle,
		       c.EventId,
		       A.GraphId,
		       A.SimulationId,
		       c.Id     AS TrueEventId
		FROM   Instance A
		       INNER JOIN InstanceExtension B
		            ON  b.InstanceId = a.Id
		       INNER JOIN [event] c
		            ON  c.InstanceId = a.id
		WHERE  B.ChildId = @ChildId
		       AND c.EventId = @EventId
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
		    'GetGlobalEvents(' + CAST(@ChildId AS NVARCHAR) + ',' + @EventId 
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END