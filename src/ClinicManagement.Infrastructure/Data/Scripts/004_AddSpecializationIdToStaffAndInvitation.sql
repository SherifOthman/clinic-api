-- Add SpecializationId to StaffInvitation table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StaffInvitation]') AND name = 'SpecializationId')
BEGIN
    ALTER TABLE [dbo].[StaffInvitation]
    ADD [SpecializationId] INT NULL;
    
    ALTER TABLE [dbo].[StaffInvitation]
    ADD CONSTRAINT FK_StaffInvitation_Specialization FOREIGN KEY ([SpecializationId])
    REFERENCES [dbo].[Specializations]([Id]);
END
GO
