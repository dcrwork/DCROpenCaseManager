CREATE TABLE [dbo].[AdjunktUser]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [AdjunktId] INT NOT NULL, 
    [UserId] INT NOT NULL, 
    CONSTRAINT [FK_AdjunktUser_ToUser] FOREIGN KEY ([UserId]) REFERENCES [User]([Id]), 
    CONSTRAINT [FK_AdjunktUser_ToAdjunkt] FOREIGN KEY ([AdjunktId]) REFERENCES [Adjunkt]([Id])
)

GO
