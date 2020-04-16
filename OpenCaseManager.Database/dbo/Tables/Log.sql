CREATE TABLE [dbo].[Log] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Logged]     DATETIME       CONSTRAINT [DF_Log_Logged] DEFAULT (getdate()) NOT NULL,
    [Level]      NVARCHAR (50)  NOT NULL,
    [UserName]   NVARCHAR (250) NULL,
    [ServerName] NVARCHAR (MAX) NULL,
    [Port]       NVARCHAR (MAX) NULL,
    [Url]        NVARCHAR (MAX) NULL,
    [Https]      BIT            NULL,
    [Message]    NVARCHAR (MAX) NOT NULL,
    [Exception]  NVARCHAR (MAX) NULL,
    [XML]        XML            NULL,
    CONSTRAINT [PK_dbo.Log] PRIMARY KEY CLUSTERED ([Id] ASC)
);





