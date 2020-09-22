CREATE TABLE [dbo].[MenuItem] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [SequenceNo]    INT            NULL,
    [DisplayTitle]  NVARCHAR (100) NULL,
    [RelativeURL]   NVARCHAR (100) NULL,
    [AlwaysVisible] BIT            NULL,
    CONSTRAINT [PK__MenuItem__3214EC0782E54BE2] PRIMARY KEY CLUSTERED ([Id] ASC)
);


