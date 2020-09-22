CREATE TABLE [dbo].[Instance] (
    [Id]             INT             IDENTITY (1, 1) NOT NULL,
    [GraphId]        INT             NOT NULL,
    [SimulationId]   INT             NULL,
    [Title]          NVARCHAR (500)  NOT NULL,
    [CaseNoForeign]  NVARCHAR (500)  NULL,
    [CaseLink]       NVARCHAR (1000) NULL,
    [Responsible]    NVARCHAR (1000) NULL,
    [IsAccepting]    BIT             NULL,
    [CurrentPhaseNo] NVARCHAR (50)   NULL,
    [NextDelay]      DATETIME2 (7)   NULL,
    [NextDeadline]   DATETIME2 (7)   NULL,
    [IsOpen]         AS              (case [isAccepting] when (1) then (0) else (1) end),
    [DCRXML]         XML             NULL,
    [CaseStatus]     INT             NULL,
    [InternalCaseID] NVARCHAR (500)  NULL,
    [Description]    NVARCHAR (MAX)  NULL,
    [Modified]       DATETIME2 (7)   CONSTRAINT [DF_Instance_Modified] DEFAULT (getdate()) NULL,
    [CurrentTime]    DATETIME2 (7)   NULL,
    [MajorVersionId] INT             NULL,
    [Created]        DATETIME2 (7)   CONSTRAINT [DF_Instance_Created] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_Instance] PRIMARY KEY CLUSTERED ([Id] ASC)
);













