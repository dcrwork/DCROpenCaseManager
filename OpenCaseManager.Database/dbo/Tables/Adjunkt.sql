/****** Object:  Table [dbo].[Adjunkt]    Script Date: 15-04-2020 13:34:38 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Adjunkt](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Responsible] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
	[ObsBoxText] [nvarchar](100) NULL,
 CONSTRAINT [PK_Child] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Adjunkt]  WITH CHECK ADD  CONSTRAINT [FK_Child_Responsible] FOREIGN KEY([Responsible])
REFERENCES [dbo].[User] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Adjunkt] CHECK CONSTRAINT [FK_Child_Responsible]
GO