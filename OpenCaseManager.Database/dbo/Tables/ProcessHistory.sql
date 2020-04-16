CREATE TABLE [dbo].[ProcessHistory] (
    [Id]                 INT              IDENTITY (1, 1) NOT NULL,
    [GraphId]            INT              NOT NULL,
    [Title]              NVARCHAR (500)   NOT NULL,
    [ForeignIntegration] NVARCHAR (1000)  NULL,
    [DCRXML]             XML              NULL,
    [Status]             BIT              CONSTRAINT [DF_ProcessHistory_Status] DEFAULT ((1)) NOT NULL,
    [Created]            DATETIME         CONSTRAINT [DF_ProcessHistory_Created] DEFAULT (getdate()) NOT NULL,
    [Modified]           DATETIME         NULL,
    [OnFrontPage]        BIT              CONSTRAINT [DF_ProcessHistory_OnFrontPage] DEFAULT ((1)) NOT NULL,
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

