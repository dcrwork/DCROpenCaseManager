CREATE TABLE [dbo].[InstanceExtension] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Year]       INT            NULL,
    [Employee]   NVARCHAR (500) NULL,
    [InstanceId] INT            NOT NULL,
    [ChildId]    INT            NULL,
    CONSTRAINT [PK_InstanceExtension] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_InstanceExtension_Instance] FOREIGN KEY ([InstanceId]) REFERENCES [dbo].[Instance] ([Id]) ON DELETE CASCADE
);



