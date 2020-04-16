-- =============================================
-- Author:		Muddassar Latif
-- Create date: 20082018
-- Description:	Copy Form items for a new form 
-- from template
-- =============================================
CREATE PROCEDURE [dbo].[CopyFormFromTemplate]
-- Add the parameters for the stored procedure here
	@FormId INT,
	@TemplateId INT
AS
BEGIN
	BEGIN TRY
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
		-- Insert statements for procedure here
		DECLARE @Id INT
		DECLARE @IsGroup BIT
		DECLARE @SequenceNumber INT
		DECLARE @ItemText NVARCHAR(1000)
		DECLARE @NewId INT
		
		
		DECLARE ABC CURSOR  
		FOR
		    SELECT fi.Id,
		           fi.IsGroup,
		           fi.SequenceNumber,
		           fi.ItemText
		    FROM   FormItem AS fi
		    WHERE  fi.FormId = @TemplateId
		           AND fi.ItemId IS NULL
		
		OPEN ABC
		FETCH ABC INTO @Id,@IsGroup,@SequenceNumber,@ItemText
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    INSERT INTO FormItem
		      (
		        -- Id -- this column value is auto-generated
		        FormId,
		        IsGroup,
		        ItemId,
		        SequenceNumber,
		        ItemText
		      )
		    VALUES
		      (
		        @FormId,
		        @IsGroup,
		        NULL,
		        @SequenceNumber,
		        @ItemText
		      )
		    
		    SET @NewId = @@identity
		    
		    INSERT INTO FormItem
		      (
		        -- Id -- this column value is auto-generated
		        FormId,
		        IsGroup,
		        ItemId,
		        SequenceNumber,
		        ItemText
		      )
		    SELECT @FormId,
		           fi.IsGroup,
		           @NewId,
		           fi.SequenceNumber,
		           fi.ItemText
		    FROM   FormItem AS fi
		    WHERE  fi.ItemId = @Id
		    
		    
		    FETCH ABC INTO @Id,@IsGroup,@SequenceNumber,@ItemText
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
		    Exception
		  )
		VALUES
		  (
		    GETDATE(),
		    1,
		    'CopyFormFromTemplate(' + CAST(@FormId AS NVARCHAR) + ',' + CAST(@TemplateId AS NVARCHAR) 
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END