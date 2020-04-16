CREATE TABLE [dbo].[AcadreLog] (
    [Id]              INT             IDENTITY (1, 1) NOT NULL,
    [Method]          NVARCHAR (500)  NOT NULL,
    [Parameters]      NVARCHAR (MAX)  NULL,
    [IsSuccess]       BIT             NOT NULL,
    [Result]          NVARCHAR (MAX)  NULL,
    [ErrorStatement]  NVARCHAR (1000) NULL,
    [ErrorStackTrace] NVARCHAR (MAX)  NULL,
    [InstanceId]      INT             NULL,
    [Created]         DATETIME        CONSTRAINT [DF_AcadreLog_Created] DEFAULT (getdate()) NULL,
    CONSTRAINT [PK_AcadreLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);





