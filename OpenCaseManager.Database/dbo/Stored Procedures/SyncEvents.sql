-- =============================================
-- Author:                MUDDASSAR LATIF
-- Create date: 17-04-2018
-- Description:          This Stored Procedure will sync events
--                                                  with tasks
-- =============================================
CREATE PROCEDURE [dbo].[SyncEvents]
-- Add the parameters for the stored procedure here
	@InstanceId INT,
	@EventXML XML,
	@LoginUser INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	
	SET NOCOUNT ON;
	DECLARE @Modified AS DATETIME
	SET @Modified = GETUTCDATE()
	
	DECLARE @Id                NVARCHAR(100),
	        @Title             NVARCHAR(500),
	        @Included          BIT,
	        @Enabled           BIT,
	        @Pending           BIT,
	        @Executed          BIT,
	        @eventType         NVARCHAR(500),
	        @groups            NVARCHAR(500),
	        @isAccepting       NVARCHAR(50),
	        @nextDeadline      NVARCHAR(100),
	        @nextDelay         NVARCHAR(100),
	        @currentTime       NVARCHAR(100),
	        @role              NVARCHAR(250),
	        @phases            NVARCHAR(250),
	        @description       NVARCHAR(MAX),
	        @eventDeadline     NVARCHAR(100),
	        @type              NVARCHAR(100),
	        @ancestor          NVARCHAR(100),
	        @ParentEvent       INT
	
	BEGIN TRY
		IF ISNULL(@LoginUser, 0) = 0
		BEGIN
		    SELECT @LoginUser = i.Responsible
		    FROM   Instance AS i
		    WHERE  i.Id = @InstanceId
		END
		
		DECLARE ABC CURSOR  
		FOR
		    SELECT c.p.value('@id', 'varchar(100)') Id,
		           c.p.value('@label', 'varchar(500)') Title,
		           c.p.value('@included', 'varchar(50)') Included,
		           c.p.value('@enabled', 'varchar(50)') ENABLED,
		           c.p.value('@pending', 'varchar(50)') Pending,
		           c.p.value('@executed', 'varchar(50)') Executed,
		           c.p.value('@eventType', 'varchar(500)') eventType,
		           c.p.value('@groups', 'varchar(500)') groups,
		           c.p.value('@roles', 'varchar(250)') [role],
		           c.p.value('@phases', 'varchar(250)') phases,
		           c.p.value('@description', 'varchar(max)') [description],
		           c.p.value('@deadline', 'varchar(100)') [eventdeadline],
		           c.p.value('@type', 'varchar(100)') [type],
		           c.p.value('@ancestors', 'varchar(100)') [ancestors]
		    FROM   @EventXML.nodes('//events/event') AS c(p)
		    ORDER BY
		           [ancestors],
		           [type],
		           [Title]
		
		OPEN ABC
		FETCH ABC INTO @Id,@Title,@Included,@Enabled,@Pending,@Executed,@eventType,
		@groups,@role,@phases,@description,@eventDeadline,@type,@ancestor
		WHILE @@FETCH_STATUS = 0
		BEGIN
		    SET @ParentEvent = NULL
		    IF @ancestor IS NOT NULL
		    BEGIN
		        SELECT @ParentEvent = e.Id
		        FROM   [Event] AS e
		        WHERE  @InstanceId = e.InstanceId
		               AND e.EventId = @ancestor
		    END
		    
		    IF EXISTS(
		           SELECT id
		           FROM   [Event] AS e
		           WHERE  e.EventId = @Id
		                  AND e.InstanceId = @instanceId
		       )
		        UPDATE [Event]
		        SET    IsEnabled         = CASE @enabled
		                                WHEN 'true' THEN 1
		                                ELSE 0
		                           END,
		               IsPending         = CASE @Pending
		                                WHEN 'true' THEN 1
		                                ELSE 0
		                           END,
		               IsIncluded        = CASE @Included
		                                 WHEN 'true' THEN 1
		                                 ELSE 0
		                            END,
		               IsExecuted        = CASE @Executed
		                                 WHEN 'true' THEN 1
		                                 ELSE 0
		                            END,
		               [Description]     = @description,
		               PhaseId           = @phases,
		               Due               = CASE @eventDeadline
		                          WHEN '' THEN NULL
		                          ELSE DATEADD(
		                                   mi,
		                                   DATEDIFF(mi, GETUTCDATE(), GETDATE()),
		                                   CAST(@eventDeadline AS DATETIME2)
		                               )
		                     END
		        WHERE  InstanceId        = @instanceId
		               AND EventId       = @Id
		    ELSE 
		    IF @Included = 'true'
		        INSERT INTO [Event]
		          (
		            InstanceId,
		            EventId,
		            Title,
		            Responsible,
		            Due,
		            PhaseId,
		            IsEnabled,
		            IsPending,
		            IsIncluded,
		            IsExecuted,
		            EventType,
		            [Description],
		            [TYPE],
		            ParentId
		          )
		        VALUES
		          (
		            @InstanceId,
		            @Id,
		            @Title,
		            CASE 
		                 WHEN @role IN (SELECT [role]
		                                FROM   InstanceRole AS ir
		                                WHERE  instanceId = @InstanceId) THEN (
		                          SELECT userId
		                          FROM   InstanceRole AS ir
		                          WHERE  instanceId = @InstanceId
		                                 AND [ROLE] = @role
		                      )
		                 WHEN LOWER(@role) = 'robot' THEN -1
		                 WHEN LOWER(@role) = 'automatic' THEN -1
		                 ELSE @LoginUser
		            END,
		            CASE @eventDeadline
		                 WHEN '' THEN NULL
		                 ELSE DATEADD(
		                          mi,
		                          DATEDIFF(mi, GETUTCDATE(), GETDATE()),
		                          CAST(@eventDeadline AS DATETIME2)
		                      )
		            END,
		            @phases,
		            @enabled,
		            @pending,
		            @Included,
		            @executed,
		            CASE ISNULL(@eventType, '')
		                 WHEN '' THEN 'Tasks'
		                 ELSE @eventType
		            END,
		            @description,
		            @type,
		            @ParentEvent
		          )
		    
		    FETCH ABC INTO @Id,@Title,@Included,@Enabled,@Pending,@Executed,@eventType,
		    @groups,@role,@phases,@description,@eventDeadline,@type,@ancestor
		END 
		
		CLOSE ABC
		DEALLOCATE ABC
		
		
		UPDATE [event]
		SET    IsIncluded = 0,
		       IsEnabled = 0
		WHERE  instanceid = @instanceid
		       AND NOT eventid IN (SELECT c.p.value('@id', 'varchar(100)') Id
		                           FROM   @EventXML.nodes('//events/event') AS c(p))
		
		SET @isAccepting = @EventXML.value('(/events/@isAccepting)[1]', 'nvarchar(10)')
		SET @nextDeadline = @EventXML.value('(/events/@nextDeadline)[1]', 'nvarchar(100)')
		SET @nextDelay = @EventXML.value('(/events/@nextDelay)[1]', 'nvarchar(100)')
		SET @currentTime = @EventXML.value('(/events/@currentTime)[1]', 'nvarchar(100)')
		SET @phases = @EventXML.value('(/events/@currentPhase)[1]', 'nvarchar(10)')              
		
		UPDATE Instance
		SET    IsAccepting = CASE @isAccepting
		                          WHEN 'True' THEN 1
		                          ELSE 0
		                     END,
		       CurrentPhaseNo = (
		           SELECT a.id
		           FROM   processphase a
		                  INNER JOIN process b
		                       ON  b.id = a.processid
		           WHERE  b.graphid = instance.graphid
		                  AND a.sequencenumber = @phases
		       ),
		       NextDelay = CASE @nextDelay
		                        WHEN 'None' THEN NULL
		                        ELSE CAST(@nextDelay AS DATETIME2)
		                   END,
		       NextDeadline = CASE @nextDeadline
		                           WHEN 'None' THEN NULL
		                           ELSE CAST(@nextDeadline AS DATETIME2)
		                      END,
		       currentTime = CASE @currentTime
		                          WHEN 'None' THEN NULL
		                          ELSE CAST(@currentTime AS DATETIME2)
		                     END,
		       Modified = @Modified
		WHERE  Id = @InstanceId
		
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
		    'SyncEvents(' + CAST(@InstanceId AS NVARCHAR) + ', <xml> , ' + CAST(@LoginUser AS NVARCHAR) 
		    + ')',
		    ERROR_MESSAGE(),
		    @EventXML
		  )
	END CATCH
END