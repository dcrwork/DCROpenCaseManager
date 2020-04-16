CREATE TABLE [dbo].[Event] (
    [Id]            INT              IDENTITY (1, 1) NOT NULL,
    [InstanceId]    INT              NOT NULL,
    [EventId]       NVARCHAR (100)   NULL,
    [Title]         NVARCHAR (500)   NOT NULL,
    [Responsible]   NVARCHAR (50)    NOT NULL,
    [Due]           DATETIME         NULL,
    [PhaseId]       NVARCHAR (500)   NULL,
    [IsEnabled]     BIT              NOT NULL,
    [IsPending]     BIT              NOT NULL,
    [IsIncluded]    BIT              NOT NULL,
    [IsExecuted]    BIT              NOT NULL,
    [EventType]     NVARCHAR (50)    NOT NULL,
    [isOpen]        AS               (case [isincluded] when (1) then case when [isenabled]=(1) then (1) else [ispending] end else (0) end),
    [Description]   NVARCHAR (MAX)   NULL,
    [EventTypeData] XML              NULL,
    [Type]          NVARCHAR (100)   NULL,
    [Token]         UNIQUEIDENTIFIER CONSTRAINT [DF_Event_Token] DEFAULT (newid()) NULL,
    [Note]          NVARCHAR (MAX)   NULL,
    [NotApplicable] BIT              CONSTRAINT [DF_Event_NotApplicable] DEFAULT ((0)) NULL,
    [NoteIsHtml]    BIT              CONSTRAINT [DF_Event_NoteIsHtml] DEFAULT ((0)) NULL,
    [ParentId]      INT              NULL,
    [Roles]         NVARCHAR (1000)  NULL,
    CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Event_Instance] FOREIGN KEY ([InstanceId]) REFERENCES [dbo].[Instance] ([Id])
);


















GO
CREATE NONCLUSTERED INDEX [IDX_Event1]
    ON [dbo].[Event]([InstanceId] ASC, [IsPending] ASC);


GO
CREATE TRIGGER UPDATEEvent
ON dbo.Event
   AFTER UPDATE
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	-- Insert statements for trigger here
	UPDATE [Event]
	SET    NotApplicable = 0
	FROM   INSERTED
	       INNER JOIN DELETED
	            ON  INSERTED.id = DELETED.id
	WHERE  INSERTED.id = [Event].id
	       AND INSERTED.IsPending = 1
	       AND ISNULL(DELETED.IsPending, 0) = 0
	       AND ISNULL(INSERTED.notapplicable, 0) = 1
END