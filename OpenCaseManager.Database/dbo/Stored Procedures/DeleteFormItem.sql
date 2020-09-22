-- =============================================
-- Author:		Muddassar Latif
-- Create date: 19082018
-- Description:	Delete form item
-- =============================================
CREATE PROCEDURE [dbo].[DeleteFormItem]
-- Add the parameters for the stored procedure here
	@FormItemId INT
AS
BEGIN
	BEGIN TRY
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
		-- Insert statements for procedure here
		DECLARE @FormItemParentId INT
		DECLARE @SequenceNumber INT
		
		-- get item and sequence number of source and target id
		SELECT @FormItemParentId = fi.ItemId,
		       @SequenceNumber = fi.SequenceNumber
		FROM   FormItem AS fi
		WHERE  fi.Id = @FormItemId 
		
		-- change sequence numbers at source branch
		UPDATE FormItem
		SET    SequenceNumber = (SequenceNumber - 1)
		WHERE  ISNULL(ItemId, 0) = ISNULL(@FormItemParentId, 0)
		       AND SequenceNumber > @SequenceNumber
		
		DELETE 
		FROM   FormItem
		WHERE  ItemId = @FormItemId
		
		DELETE 
		FROM   FormItem
		WHERE  Id = @FormItemId
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
		    'DeleteFormItem(' + CAST(@FormItemId AS NVARCHAR)+ ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END