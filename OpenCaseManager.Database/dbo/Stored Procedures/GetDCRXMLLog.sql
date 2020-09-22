-- =============================================
-- Author:              Morten Marquard
-- Create date:			July 4th, 2019
-- Description:         Returns DCR XML Logs for
-- Updated :			"id" attribute is small cap (03-01-2020)
-- =============================================
CREATE PROCEDURE [dbo].[GetDCRXMLLog]
-- Add the parameters for the stored procedure here
	@GraphId
AS
	INT,
	@From AS DATETIME2 = NULL,
	@To AS DATETIME2 = NULL,
	@IsAccepting AS INT = 0
	AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here           
	BEGIN TRY
		DECLARE @title AS NVARCHAR(400)
		DECLARE @initTime AS NVARCHAR(100)
		SELECT @title = TItle
		FROM   process
		WHERE  GraphId = @GraphId
		--print @title
		DECLARE @Log AS NVARCHAR(600)
		SET @log = '<log title="' + REPLACE(
		        REPLACE(
		            REPLACE(REPLACE(REPLACE(@title, '''', ''), '"', ''), '&', '&amp;'),
		            '<',
		            '&lt'
		        ),
		        '>',
		        '<&gt;'
		    ) + '"></log>'
		--print @LOg
		DECLARE @xml AS XML
		SET @xml = CAST(@log AS XML)
		DECLARE @id AS INT
		DECLARE @created AS DATETIME2
		DECLARE @trace AS XML
		DECLARE ABC CURSOR  
		FOR
		    SELECT id,
		           created,
		           T2.Loc.query('.')
		    FROM   instance
		           CROSS APPLY DCRXML.nodes('/dcrgraph/runtime/log/trace') AS T2(Loc)
		    WHERE  graphid = @GraphId
		           AND (@IsAccepting = 0 OR @IsAccepting = 1 AND IsAccepting = 1)
		           AND (
		                   @From IS NULL
		                   AND @To IS NULL
		                   OR created > @From
		                   AND created < @To
		               )
		           AND NOT dcrxml IS NULL
		    ORDER BY
		           1 DESC
		
		OPEN abc
		FETCH abc INTO @id,@created,@trace
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    --print @id
		    SET @initTime = @trace.query('./trace').value('(trace/@init)[1]', 'nvarchar(100)')
		    --print @initTime
		    SET @trace.modify('delete (trace/@init)[1]')
		    SET @trace.modify(
		            'insert attribute initTime {sql:variable("@initTime")} into (/*)[1]'
		        )
		    
		    SET @trace.modify('insert attribute id {sql:variable("@Id")} into (/*)[1]')
		    SET @trace.modify(
		            'insert attribute created {sql:variable("@created")} into (/*)[1]'
		        )
		    
		    SET @xml.modify('insert sql:variable("@trace") as first into (/log)[1]')
		    FETCH abc INTO @id,@created,@trace
		END
		CLOSE abc
		DEALLOCATE abc
		SELECT @xml AS DCRXML
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
		    'GetDCRXMLLog(' + CAST(@GraphId AS NVARCHAR) + ',' + CAST(@From AS NVARCHAR) 
		    + ',' + CAST(@To AS NVARCHAR) + ',' + CAST(@IsAccepting AS NVARCHAR)
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END