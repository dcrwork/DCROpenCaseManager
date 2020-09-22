-- =============================================
-- Author:		Muddassar Latif
-- Create date: 19082018
-- Description:	Set sequence and item Id
-- =============================================
CREATE PROCEDURE [dbo].[SetFormItemSequence]
-- Add the parameters for the stored procedure here
	@Source INT,
	@Target INT,
	@Position INT
AS
BEGIN
	BEGIN TRY
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
		-- Insert statements for procedure here
		
		DECLARE @SourceItemId INT
		DECLARE @SourceSequenceNumber INT
		DECLARE @TargetItemId INT
		DECLARE @TargetSequenceNumber INT
		
		-- get item and sequence number of source and target id
		SELECT @SourceItemId = fi.ItemId,
		       @SourceSequenceNumber = fi.SequenceNumber
		FROM   FormItem AS fi
		WHERE  fi.Id = @Source 
		
		-- change sequence numbers at source branch
		UPDATE FormItem
		SET    SequenceNumber = (SequenceNumber - 1)
		WHERE  ISNULL(ItemId, 0) = ISNULL(@SourceItemId, 0)
		       AND SequenceNumber > @SourceSequenceNumber
		
		SELECT @TargetItemId = fi.ItemId,
		       @TargetSequenceNumber = fi.SequenceNumber
		FROM   FormItem AS fi
		WHERE  fi.Id = @Target 
		
		-- if source is moved in a parent, move source at last sequence in that parent
		IF @Position = 1
		BEGIN
		    UPDATE FormItem
		    SET    ItemId             = @Target,
		           SequenceNumber     = (
		               SELECT ISNULL(MAX(fi.SequenceNumber), 0) + 1
		               FROM   FormItem AS fi
		               WHERE  ISNULL(fi.ItemId, 0) = ISNULL(@Target, 0)
		           )
		    WHERE  Id = @Source
		END-- if source is moved before a child
		ELSE 
		IF @Position = 0
		BEGIN
		    UPDATE FormItem
		    SET    ItemId = @TargetItemId,
		           SequenceNumber = @TargetSequenceNumber
		    WHERE  Id = @Source
		    
		    UPDATE FormItem
		    SET    SequenceNumber = (SequenceNumber + 1)
		    WHERE  ISNULL(ItemId, 0) = ISNULL(@TargetItemId, 0)
		           AND SequenceNumber >= @TargetSequenceNumber
		           AND id <> @Source
		END-- if source is moved after a child
		ELSE
		BEGIN
		    UPDATE FormItem
		    SET    ItemId = @TargetItemId,
		           SequenceNumber = @TargetSequenceNumber + 1
		    WHERE  Id = @Source
		    
		    UPDATE FormItem
		    SET    SequenceNumber = (SequenceNumber + 1)
		    WHERE  ISNULL(ItemId, 0) = ISNULL(@TargetItemId, 0)
		           AND SequenceNumber > @TargetSequenceNumber
		           AND id <> @Source
		END
		
		-- set Is Group
		UPDATE FormItem
		SET    IsGroup = 1
		WHERE  ItemId IS NULL
		
		UPDATE FormItem
		SET    IsGroup = 0
		WHERE  ItemId IS NOT NULL
		
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
		    'SetFormItemSequence(' + CAST(@Source AS NVARCHAR) + ',' + CAST(@Target AS NVARCHAR)  + ',' + CAST(@Position AS NVARCHAR)
		    + ')',
		    ERROR_MESSAGE()
		  )
	END CATCH
END