/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
Schema Changes for Governance of Process
--------------------------------------------------------------------------------------
*/

USE OpenCaseManager;


GO
PRINT N'Altering [dbo].[Instance]...';


GO
ALTER TABLE [dbo].[Instance]
    ADD [MajorVersionId] INT           NULL,
        [Created]        DATETIME2 (7) CONSTRAINT [DF_Instance_Created] DEFAULT (getdate()) NULL;


GO
PRINT N'Altering [dbo].[Process]...';


GO
ALTER TABLE [dbo].[Process]
    ADD [MajorVersionId]    INT             NULL,
        [MajorVersionTitle] NVARCHAR (1000) NULL,
        [MajorVerisonDate]  DATETIME        NULL,
        [ReleaseDate]       DATETIME        NULL,
        [Owner]             INT             NULL;


GO
PRINT N'Creating [dbo].[ProcessHistory]...';


GO
CREATE TABLE [dbo].[ProcessHistory] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [GraphId]            INT              NOT NULL,
    [Title]              NVARCHAR (500)   NOT NULL,
    [ForeignIntegration] NVARCHAR (1000)  NULL,
    [DCRXML]             XML              NULL,
    [Status]             BIT              NOT NULL,
    [Created]            DATETIME         NOT NULL,
    [Modified]           DATETIME         NULL,
    [OnFrontPage]        BIT              NOT NULL,
    [Guid]               UNIQUEIDENTIFIER NULL,
    [CreateInstance]     BIT              NULL,
    [EventId]            NVARCHAR (500)   NULL,
    [InstanceGuid]       UNIQUEIDENTIFIER NULL,
    [MajorVersionId]     INT              NULL,
    [MajorVersionTitle]  NVARCHAR (1000)  NULL,
    [MajorVerisonDate]   DATETIME         NULL,
    [ReleaseDate]        DATETIME         NULL,
    [Owner]              INT              NULL,
    [InstanceId]         INT              NULL,
    [State]              INT              NULL,
    CONSTRAINT [PK_ProcessHistory] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
PRINT N'Creating [dbo].[DF_ProcessHistory_Status]...';


GO
ALTER TABLE [dbo].[ProcessHistory]
    ADD CONSTRAINT [DF_ProcessHistory_Status] DEFAULT ((1)) FOR [Status];


GO
PRINT N'Creating [dbo].[DF_ProcessHistory_Created]...';


GO
ALTER TABLE [dbo].[ProcessHistory]
    ADD CONSTRAINT [DF_ProcessHistory_Created] DEFAULT (getdate()) FOR [Created];


GO
PRINT N'Creating [dbo].[DF_ProcessHistory_OnFrontPage]...';


GO
ALTER TABLE [dbo].[ProcessHistory]
    ADD CONSTRAINT [DF_ProcessHistory_OnFrontPage] DEFAULT ((1)) FOR [OnFrontPage];


GO
PRINT N'Refreshing [dbo].[AllInstances]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[AllInstances]';


GO
PRINT N'Refreshing [dbo].[CaseMemoBridge]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[CaseMemoBridge]';


GO
PRINT N'Refreshing [dbo].[InstanceAutomaticEvents]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[InstanceAutomaticEvents]';


GO
PRINT N'Refreshing [dbo].[InstanceEventHistory]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[InstanceEventHistory]';


GO
PRINT N'Refreshing [dbo].[InstanceEvents]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[InstanceEvents]';


GO
PRINT N'Refreshing [dbo].[InstancePhases]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[InstancePhases]';


GO
PRINT N'Refreshing [dbo].[MUS]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[MUS]';


GO
PRINT N'Refreshing [dbo].[MyInstances]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[MyInstances]';


GO
PRINT N'Refreshing [dbo].[PhaseInstances]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[PhaseInstances]';


GO
PRINT N'Refreshing [dbo].[ResponsibleInstancesCount]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[ResponsibleInstancesCount]';


GO
PRINT N'Refreshing [dbo].[Timeline]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[Timeline]';


GO
PRINT N'Refreshing [dbo].[MUSTasks]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[MUSTasks]';


GO
PRINT N'Creating [dbo].[Processes]...';


GO
CREATE VIEW dbo.Processes
AS
SELECT        ph.Id, ph.GraphId, ISNULL(p.Title, ph.Title) AS Title, ISNULL(p.DCRXML, ph.DCRXML) AS DCRXML, p.MajorVersionId, p.MajorVersionTitle, p.MajorVerisonDate, ph.State AS ProcessApprovalState, p.OnFrontPage, ISNULL
                             ((SELECT        Name
                                 FROM            dbo.[User] AS u
                                 WHERE        (Id = ISNULL(p.Owner, ph.Owner))), '') AS ProcessOwner, ph.ReleaseDate, ph.InstanceId, p.Id AS ProcessId
FROM            dbo.ProcessHistory AS ph LEFT OUTER JOIN
                         dbo.Process AS p ON p.GraphId = ph.GraphId
WHERE        (ph.GraphId IN
                             (SELECT        GraphId
                               FROM            dbo.ProcessHistory AS ph
                               GROUP BY GraphId)) AND (ph.MajorVersionId IS NULL OR
                         ph.MajorVersionId IN
                             (SELECT        MAX(MajorVersionId) AS MaxMajorRevisionId
                               FROM            dbo.ProcessHistory AS ph
                               GROUP BY GraphId)) AND (ph.State <> - 1) AND (ph.Id IN
                             (SELECT        MAX(Id) AS MaxId
                               FROM            dbo.ProcessHistory AS ph2
                               GROUP BY GraphId))
GO
PRINT N'Refreshing [dbo].[ChildInstances]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[ChildInstances]';


GO
PRINT N'Refreshing [dbo].[PendingActivities]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[PendingActivities]';


GO
PRINT N'Refreshing [dbo].[GetDataForChildren]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[GetDataForChildren]';


GO
PRINT N'Refreshing [dbo].[InstanceTasks]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[InstanceTasks]';


GO
PRINT N'Refreshing [dbo].[InstanceTasksAllEnabled]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[InstanceTasksAllEnabled]';


GO
PRINT N'Creating [dbo].[GetDCRXMLLog]...';


GO
-- =============================================
-- Author:		Morten Marquard
-- Create date: July 4th, 2019
-- Description:	Returns DCR XML Logs for
-- =============================================
CREATE PROCEDURE [GetDCRXMLLog]
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
	DECLARE @title AS NVARCHAR(400)
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
	    SET @trace.modify('insert attribute Id {sql:variable("@Id")} into (/*)[1]')
	    SET @trace.modify(
	            'insert attribute created {sql:variable("@created")} into (/*)[1]'
	        )
	    
	    SET @xml.modify('insert sql:variable("@trace") as first into (/log)[1]')
	    FETCH abc INTO @id,@created,@trace
	END
	CLOSE abc
	DEALLOCATE abc
	SELECT @xml AS DCRXML
END
GO
PRINT N'Creating [dbo].[ReleaseProcessInstance]...';


GO
-- =============================================
-- Author:          Muddassar Latif
-- Create date:		01-11-2019
-- Description:     Release a process after approval
-- =============================================
CREATE PROCEDURE [dbo].[ReleaseProcessInstance]
-- Add the parameters for the stored procedure here
	@instanceId INT,
	@ProcessPhaseXML XML = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	BEGIN TRY
		DECLARE @GraphId INT
		
		SELECT @GraphId = ph.GraphId
		FROM   ProcessHistory AS ph
		WHERE  ph.InstanceId = @instanceId
		
		IF NOT EXISTS(
		       SELECT 1
		       FROM   Process AS p
		       WHERE  p.GraphId = @GraphId
		   )
		BEGIN
		    INSERT INTO Process
		      (
		        -- Id -- this column value is auto-generated
		        GraphId,
		        Title,
		        ForeignIntegration,
		        DCRXML,
		        [Status],
		        Created,
		        Modified,
		        OnFrontPage,
		        [Guid],
		        CreateInstance,
		        EventId,
		        InstanceGuid,
		        MajorVersionId,
		        MajorVersionTitle,
		        MajorVerisonDate,
		        ReleaseDate,
		        [Owner]
		      )
		    SELECT -- Id -- this column value is auto-generated
		           GraphId,
		           Title,
		           ForeignIntegration,
		           DCRXML,
		           [Status],
		           Created,
		           Modified,
		           OnFrontPage,
		           [Guid],
		           CreateInstance,
		           EventId,
		           InstanceGuid,
		           MajorVersionId,
		           MajorVersionTitle,
		           MajorVerisonDate,
		           GETDATE(),
		           [Owner]
		    FROM   ProcessHistory AS ph
		    WHERE  ph.InstanceId = @instanceId
		END
		ELSE
		BEGIN
		    UPDATE Process
		    SET    DCRXML                = ph.DCRXML,
		           Modified              = GETDATE(),
		           MajorVersionId        = ph.MajorVersionId,
		           MajorVersionTitle     = ph.MajorVersionTitle,
		           MajorVerisonDate      = ph.MajorVerisonDate,
		           ReleaseDate           = GETDATE()
		    FROM   ProcessHistory AS ph
		    WHERE  Process.GraphId = @GraphId
		           AND ph.InstanceId = @instanceId
		END
		
		UPDATE ProcessHistory
		SET    [State] = -1
		WHERE  GraphId = @GraphId
		       AND (InstanceId IS NULL OR InstanceId <> @InstanceId)
		
		UPDATE ProcessHistory
		SET    ReleaseDate = GETDATE(),
		       [State] = 1
		WHERE  InstanceId = @instanceId
		
		DECLARE @ProcessId INT
		SELECT @ProcessId = Id
		FROM   Process AS p
		WHERE  p.GraphId = @GraphId
		
		-- add/update process phases
		EXEC AddProcessPhases
		     @ProcessId = @ProcessId,
		     @PhaseXml = @ProcessPhaseXML
	END TRY
	BEGIN CATCH
		SELECT ERROR_NUMBER()     AS ErrorNumber,
		       ERROR_SEVERITY()   AS ErrorSeverity,
		       ERROR_STATE()      AS ErrorState,
		       ERROR_PROCEDURE()  AS ErrorProcedure,
		       ERROR_LINE()       AS ErrorLine,
		       ERROR_MESSAGE()    AS ErrorMessage
	END CATCH
END
GO
PRINT N'Refreshing [dbo].[AddEventTypeData]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[AddEventTypeData]';


GO
PRINT N'Refreshing [dbo].[AddInstanceDescription]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[AddInstanceDescription]';


GO
PRINT N'Refreshing [dbo].[AdvanceTime]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[AdvanceTime]';


GO
PRINT N'Refreshing [dbo].[CanExecuteGlobalEvent]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[CanExecuteGlobalEvent]';


GO
PRINT N'Refreshing [dbo].[ChangeResponsibleOfChild]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[ChangeResponsibleOfChild]';


GO
PRINT N'Refreshing [dbo].[GetGlobalEvents]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[GetGlobalEvents]';


GO
PRINT N'Refreshing [dbo].[GetValueFromForm]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[GetValueFromForm]';


GO
PRINT N'Refreshing [dbo].[SetCurrentPhase]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[SetCurrentPhase]';


GO
PRINT N'Refreshing [dbo].[SyncEvents]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[SyncEvents]';


GO
PRINT N'Refreshing [dbo].[UpdateEventLogInstance]...';


GO
EXECUTE sp_refreshsqlmodule N'[dbo].[UpdateEventLogInstance]';


GO
PRINT N'Update complete.';


GO
