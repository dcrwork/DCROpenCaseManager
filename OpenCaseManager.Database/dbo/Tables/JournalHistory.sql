CREATE TABLE [dbo].[JournalHistory] (
    [Id]				INT             IDENTITY (1, 1) NOT NULL,
    [InstanceId]		INT				NULL,
    [EventId]			INT             NULL,
    [DocumentId]		INT             NULL,
    [Type]				NVARCHAR (100)  NOT NULL,
    [Title]				NVARCHAR (1000) NOT NULL,
    [CreationDate]		DATETIME		NOT NULL,
    [EventDate]			DATETIME        NOT NULL,
    [IsLocked]			BIT				NOT NULL,
    [ChildId]			INT				NULL,
    CONSTRAINT [PK_JournalHistory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_JournalHistory_InstanceId] FOREIGN KEY ([InstanceId]) REFERENCES [dbo].[Instance] ([Id]) ON DELETE CASCADE
);

