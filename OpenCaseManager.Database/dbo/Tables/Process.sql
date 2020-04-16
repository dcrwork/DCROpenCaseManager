CREATE TABLE [dbo].[Process] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [GraphId]            INT              NOT NULL,
    [Title]              NVARCHAR (500)   NOT NULL,
    [ForeignIntegration] NVARCHAR (1000)  NULL,
    [DCRXML]             XML              NULL,
    [Status]             BIT              CONSTRAINT [DF_Process_Status] DEFAULT ((1)) NOT NULL,
    [Created]            DATETIME         CONSTRAINT [DF_Process_Created] DEFAULT (getdate()) NOT NULL,
    [Modified]           DATETIME         NULL,
    [OnFrontPage]        BIT              CONSTRAINT [DF_Process_OnFrontPage] DEFAULT ((1)) NOT NULL,
    [MajorVersionId]     INT              NULL,
    [MajorVersionTitle]  NVARCHAR (1000)  NULL,
    [MajorVerisonDate]   DATETIME         NULL,
    [ReleaseDate]        DATETIME         NULL,
    [Owner]              INT              NULL,
    [Guid]               UNIQUEIDENTIFIER NULL,
    [InstanceGuid]       UNIQUEIDENTIFIER NULL,
    [CreateInstance]     BIT              NULL,
    [EventId]            NVARCHAR (500)   NULL,
    CONSTRAINT [PK_Process] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [IX_Process] UNIQUE NONCLUSTERED ([GraphId] ASC)
);














GO


