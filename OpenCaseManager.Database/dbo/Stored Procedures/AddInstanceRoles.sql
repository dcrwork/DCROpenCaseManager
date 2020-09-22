-- =============================================
-- Author:		Muddassar Latif
-- Create date: 10-05-2018
-- Description:	Add Instance Roles
-- =============================================
CREATE PROCEDURE [dbo].[AddInstanceRoles]
-- Add the parameters for the stored procedure here
	@InstanceId INT,
	@UserRoles XML
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for procedure here
	-- 	
	BEGIN TRY
		DECLARE @Id NVARCHAR(MAX)
		DECLARE @role NVARCHAR(MAX)
		DECLARE @roles XML 
		
		DECLARE ABC CURSOR  
		FOR
		    SELECT c.p.value('@Id', 'nvarchar(MAX)') Id,
		           c.p.query('./Role') AS roles
		    FROM   @UserRoles.nodes('//UserRoles/User') AS c(p)
		
		OPEN ABC
		FETCH ABC INTO @Id,@roles
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    DECLARE xyz CURSOR  
		    FOR
		        SELECT d.p.value('.', 'nvarchar(MAX)') Id
		        FROM   @roles.nodes('/Role') AS d(p)
		    
		    OPEN xyz
		    FETCH xyz INTO @role
		    WHILE @@FETCH_STATUS = 0
		    BEGIN
		        INSERT INTO InstanceRole
		        VALUES
		          (
		            @InstanceId,
		            @role,
		            @Id
		          )
		        
		        
		        FETCH xyz INTO @role
		    END
		    CLOSE xyz
		    DEALLOCATE xyz 
		    
		    FETCH ABC INTO @Id,@roles
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
		    'AddInstanceRoles(' + CAST(@InstanceId AS NVARCHAR) + ',<xml>' + ')',
		    ERROR_MESSAGE(),
		    @UserRoles
		  )
	END CATCH
END