-- =============================================
-- Author: Thais Kure Corneliusen  
-- Create date: 22-04-2019
-- Description:	Lock all documents when older than 24 hours
-- =============================================
CREATE PROC [dbo].[LockDocuments]
AS
	BEGIN
		UPDATE dbo.Document
		SET isLocked = 1
		WHERE isLocked = 0
			AND DATEADD(hh, +24, UploadDate) < DATEADD(hh, +2, GETDATE())
	END