CREATE TABLE [dbo].[Document] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [Title]       NVARCHAR (1000) NOT NULL,
    [Type]        NVARCHAR (100)  NOT NULL,
    [Link]        NVARCHAR (MAX)  NOT NULL,
    [Responsible] NVARCHAR (500)  NULL,
    [InstanceId]  INT             NULL,
    [IsActive]    BIT             CONSTRAINT [DF_Document_IsActive] DEFAULT ((1)) NOT NULL,
    [UploadDate]  DATETIME        NULL,
    [isLocked]    BIT             NULL,
    [isDraft]     BIT             NULL,
    [ChildId]     INT             NULL,
    CONSTRAINT [PK_Document] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Document_Instance] FOREIGN KEY ([InstanceId]) REFERENCES [dbo].[Instance] ([Id])
);



