﻿CREATE TABLE [dbo].[UserReferenceKey]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [Type] VARCHAR(20) NULL
)
GO

CREATE UNIQUE INDEX [IX_UserReferenceKey_Type] ON [dbo].[UserReferenceKey] ([Type])
GO
CREATE TABLE [dbo].[UserPropertyKey]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1), 
    [Type] VARCHAR(20) NULL
)
GO

CREATE UNIQUE INDEX [IX_UserPropertyKey_Type] ON [dbo].[UserPropertyKey] ([Type])
GO
CREATE TABLE[dbo].[User]
(
     [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1)
	,[ExternalId] UNIQUEIDENTIFIER NOT NULL
    ,[VersionId] UNIQUEIDENTIFIER NULL 
    ,[UserIdAudit] UNIQUEIDENTIFIER NULL 
	,[DateCreated] DATETIME NOT NULL DEFAULT(GETUTCDATE())
	,[DateUpdated] DATETIME NULL
	,[Sig] VARCHAR(256) NULL
	,[Body] NVARCHAR(MAX)
)
GO
CREATE UNIQUE INDEX[IX_User_ExternalId] ON [dbo].[User] ([ExternalId]) INCLUDE ([VersionId])

GO
CREATE TABLE[dbo].[UserHistory]
(
     [Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1)
    ,[EntityId] BIGINT NOT NULL 
	,[ExternalId] UNIQUEIDENTIFIER NOT NULL
    ,[VersionId] UNIQUEIDENTIFIER NULL 
    ,[UserIdAudit] UNIQUEIDENTIFIER NULL 
	,[DateCreated] DATETIME NOT NULL 
	,[DateUpdated] DATETIME NULL
	,[TimeStamp] DATETIME NOT NULL DEFAULT(GETUTCDATE())
	,[Sig] VARCHAR(256) NULL
	,[Body] NVARCHAR(MAX) NULL
)

GO
CREATE TABLE [dbo].[UserProperty]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1), 
	[EntityId] BIGINT NOT NULL,
    [KeyId] INT NOT NULL, 
    [Value] NVARCHAR(250) NOT NULL, 
    CONSTRAINT [FK_UserProperty_Id] FOREIGN KEY ([EntityId]) REFERENCES [dbo].[User]([Id]), 
    CONSTRAINT [FK_UserProperty_KeyId] FOREIGN KEY ([KeyId]) REFERENCES [dbo].[UserPropertyKey]([Id])
)
GO
CREATE INDEX [IX_UserProperty_EntityId] ON [dbo].[UserProperty] ([EntityId]) INCLUDE ([Id])
GO
CREATE INDEX [IX_UserProperty_KeyId] ON [dbo].[UserProperty] ([KeyId],[EntityId]) INCLUDE ([Value])

GO
CREATE TABLE [dbo].[UserReference]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1), 
	[EntityId] BIGINT NOT NULL,
    [KeyId] INT NOT NULL, 
    [Value] NVARCHAR(250) NOT NULL, 
    CONSTRAINT [FK_UserReference_Id] FOREIGN KEY ([EntityId]) REFERENCES [dbo].[User]([Id]), 
    CONSTRAINT [FK_UserReference_KeyId] FOREIGN KEY ([KeyId]) REFERENCES [dbo].[UserReferenceKey]([Id])
)
GO
CREATE UNIQUE INDEX [IX_UserReference_TypeReference] ON [dbo].[UserReference] ([KeyId],[Value]) INCLUDE ([EntityId])
GO
CREATE INDEX [IX_UserReference_EntityId] ON [dbo].[UserReference] ([EntityId]) INCLUDE ([Id])
GO
CREATE INDEX [IX_UserReference_KeyId] ON [dbo].[UserReference] ([KeyId],[EntityId]) INCLUDE ([Value])

GO

CREATE PROCEDURE [dbo].[spUser_UpsertRP]
	@EntityId BIGINT,
	@References [External].[KvpTableType] READONLY,
	@Properties [External].[KvpTableType] READONLY
AS
	BEGIN TRY

		--Process the reference first.
		INSERT INTO [dbo].[UserReferenceKey] WITH (UPDLOCK) ([Type])
		SELECT DISTINCT [RefType] FROM @References
		EXCEPT
		SELECT [Type] FROM [dbo].[UserReferenceKey]

		--Remove old records
		DELETE FROM [dbo].[UserReference] WITH (UPDLOCK)
		WHERE [EntityId] = @EntityId

		--Add the new records.
		INSERT INTO [dbo].[UserReference] WITH (UPDLOCK)
		(
		      [EntityId]
			, [KeyId]
			, [Value]
		)
		SELECT @EntityId, K.[Id], R.RefValue
		FROM @References AS R 
		INNER JOIN [dbo].[UserReferenceKey] AS K ON R.[RefType] = K.[Type]
	
		--Now process the properties.
		INSERT INTO [dbo].[UserPropertyKey] WITH (UPDLOCK) ([Type])
		SELECT DISTINCT [RefType] FROM @Properties
		EXCEPT
		SELECT [Type] FROM [dbo].[UserPropertyKey]

		--Remove old records
		DELETE FROM [dbo].[UserProperty] WITH (UPDLOCK)
		WHERE [EntityId] = @EntityId

		--Add the new records.
		INSERT INTO [dbo].[UserProperty] WITH (UPDLOCK)
		(
			  [EntityId]
			, [KeyId]
			, [Value]
		)
		SELECT @EntityId, K.[Id], P.RefValue
		FROM @Properties AS P
		INNER JOIN [dbo].[UserPropertyKey] AS K ON P.[RefType] = K.[Type]

		RETURN 200;
	END TRY
	BEGIN CATCH
		IF (ERROR_NUMBER() = 2601)
			RETURN 409; --Conflict - duplicate reference key to other transaction.
		ELSE
			THROW;
	END CATCH

GO
CREATE PROCEDURE [External].[spUser_Create]
	 @ExternalId UNIQUEIDENTIFIER
	,@VersionId UNIQUEIDENTIFIER = NULL
	,@VersionIdNew UNIQUEIDENTIFIER = NULL
    ,@UserIdAudit UNIQUEIDENTIFIER = NULL 
	,@Body NVARCHAR (MAX)
	,@DateCreated DATETIME = NULL
	,@DateUpdated DATETIME = NULL
	,@References [External].[KvpTableType] READONLY 
	,@Properties [External].[KvpTableType] READONLY 
	,@Sig VARCHAR(256) = NULL
AS
	BEGIN TRY
		BEGIN TRAN

		-- Insert record into DB and get its identity
		INSERT INTO [dbo].[User] 
		(
			  ExternalId
			, VersionId
			, UserIdAudit
			, DateCreated
			, DateUpdated
			, Sig
			, Body
		)
		VALUES 
		(
			  @ExternalId
			, ISNULL(@VersionIdNew, NEWID())
			, @UserIdAudit
			, ISNULL(@DateCreated, GETUTCDATE())
			, @DateUpdated
			, @Sig
			, @Body
		)
		DECLARE @Id BIGINT = SCOPE_IDENTITY();
	
		-- Create references and properties
		DECLARE @RPResponse INT;
		EXEC @RPResponse = [dbo].[spUser_UpsertRP] @Id, @References, @Properties
		IF (@RPResponse = 409)
		BEGIN
			ROLLBACK TRAN;
			RETURN 409; --Conflict with other entities.
		END

		COMMIT TRAN;

		RETURN 201;
	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage; 
		ROLLBACK TRAN;
		RETURN 500;
	END CATCH

GO
CREATE PROCEDURE [External].[spUser_Delete]
	@ExternalId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
	BEGIN TRY
	
		BEGIN TRAN

		DECLARE @Id BIGINT = (SELECT [Id] FROM [dbo].[User] WHERE [ExternalId] = @ExternalId)

		IF (@Id IS NULL)
		BEGIN
			ROLLBACK TRAN
			RETURN 404;
		END

		DELETE FROM [dbo].[UserProperty]
		WHERE [EntityId] = @Id

		DELETE FROM [dbo].[UserReference]
		WHERE [EntityId] = @Id

		DELETE FROM [dbo].[User]
		WHERE [Id] = @Id

		--Not found.
		COMMIT TRAN

		RETURN 200;

	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
		ROLLBACK TRAN;
		RETURN 500;
	END CATCH
GO
CREATE PROCEDURE [External].[spUser_DeleteByRef]
	@RefType VARCHAR(30),
	@RefValue NVARCHAR(250) 
AS
SET NOCOUNT ON;
	BEGIN TRY
		
		BEGIN TRAN

		DECLARE @Id BIGINT = (
			SELECT E.[Id] FROM [dbo].[User] E
			INNER JOIN [dbo].[UserReference] R ON E.Id = R.EntityId
			INNER JOIN [dbo].[UserReferenceKey] RK ON R.KeyId = RK.Id
			WHERE RK.[Type] = @RefType AND R.[Value] = @RefValue
		)

		IF (@Id IS NULL)
		BEGIN
			ROLLBACK TRAN
			RETURN 404;
		END

		DELETE FROM [dbo].[UserProperty]
		WHERE [EntityId] = @Id

		DELETE FROM [dbo].[UserReference]
		WHERE [EntityId] = @Id

		DELETE FROM [dbo].[User]
		WHERE [Id] = @Id

		--Not found.
		COMMIT TRAN

		RETURN 200;

	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
		ROLLBACK TRAN
		RETURN 500
	END CATCH
GO
CREATE PROCEDURE [External].[spUser_Read]
	@ExternalId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
	BEGIN TRY
	
		SELECT * 
		FROM [dbo].[User]
		WHERE [ExternalId] = @ExternalId

		IF (@@ROWCOUNT>0)
			RETURN 200;
	
		--Not found.
		RETURN 404;

	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
		RETURN 500
	END CATCH
GO
CREATE PROCEDURE [External].[spUser_ReadByRef]
	@RefType VARCHAR(30),
	@RefValue NVARCHAR(250) 
AS
SET NOCOUNT ON;
	BEGIN TRY
	
		SELECT E.* 
		FROM [dbo].[User] E
		INNER JOIN [dbo].[UserReference] R ON E.Id = R.EntityId
		INNER JOIN [dbo].[UserReferenceKey] RK ON R.KeyId = RK.Id
		WHERE RK.[Type] = @RefType AND R.[Value] = @RefValue

		IF (@@ROWCOUNT>0)
			RETURN 200;
	
		--Not found.
		RETURN 404;

	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
		RETURN 500
	END CATCH
GO
CREATE PROCEDURE [External].[spUser_Update]
	 @ExternalId UNIQUEIDENTIFIER
	,@VersionId UNIQUEIDENTIFIER = NULL
	,@VersionIdNew UNIQUEIDENTIFIER = NULL
    ,@UserIdAudit UNIQUEIDENTIFIER = NULL 
	,@Body NVARCHAR (MAX)
	,@DateCreated DATETIME = NULL
	,@DateUpdated DATETIME = NULL
	,@References [External].[KvpTableType] READONLY
	,@Properties [External].[KvpTableType] READONLY
	,@Sig VARCHAR(256) = NULL
AS
	BEGIN TRY
		BEGIN TRAN

		DECLARE @Id BIGINT

		IF (@VersionId IS NOT NULL)
		BEGIN
			DECLARE @ExistingVersion UNIQUEIDENTIFIER;
			SELECT @Id = [Id], @VersionId = [VersionId] FROM [dbo].[User] WHERE [ExternalId] = @ExternalId
			IF (@Id IS NOT NULL AND @ExistingVersion != @VersionId)
			BEGIN
				ROLLBACK TRAN;
				RETURN 409; --Conflict
			END
		END
		ELSE
		BEGIN
			SELECT @Id = [Id] FROM [dbo].[User] WHERE [ExternalId] = @ExternalId
		END

		--Check we can find the entity
		IF (@Id IS NULL)
		BEGIN
			ROLLBACK TRAN;
			RETURN 404; --Not found
		END

		-- Insert record into DB and get its identity
		UPDATE [dbo].[User]
		SET   [VersionId] = @VersionIdNew
			, [UserIdAudit] = @UserIdAudit
			, [DateUpdated] = ISNULL(@DateUpdated, GETUTCDATE())
			, [Sig] = @Sig
			, [Body] = @Body
		WHERE Id = @Id

		IF (@@ROWCOUNT=0)
		BEGIN
			ROLLBACK TRAN;
			RETURN 409; -- Update failure
		END
	
		-- Create references and properties
		DECLARE @RPResponse INT;
		EXEC @RPResponse = [dbo].[spUser_UpsertRP] @Id, @References, @Properties
		IF (@RPResponse = 409)
		BEGIN
			ROLLBACK TRAN;
			RETURN 409; --Not found
		END

		COMMIT TRAN;

		RETURN 201;
	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage; 
		ROLLBACK TRAN;

		RETURN 500;
	END CATCH

GO
CREATE PROCEDURE [External].[spUser_Version]
	@ExternalId UNIQUEIDENTIFIER
AS
SET NOCOUNT ON;
	BEGIN TRY
	
		SELECT [ExternalId], [VersionId]
		FROM [dbo].[User]
		WHERE [ExternalId] = @ExternalId

		IF (@@ROWCOUNT>0)
			RETURN 200;
	
		--Not found.
		RETURN 404;

	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
		RETURN 500
	END CATCH
GO
CREATE PROCEDURE [External].[spUser_VersionByRef]
	@RefType VARCHAR(30),
	@RefValue NVARCHAR(250) 
AS
SET NOCOUNT ON;
	BEGIN TRY
	
		SELECT E.[ExternalId], E.[VersionId]
		FROM [dbo].[User] E
		INNER JOIN [dbo].[UserReference] R ON E.Id = R.EntityId
		INNER JOIN [dbo].[UserReferenceKey] RK ON R.KeyId = RK.Id
		WHERE RK.[Type] = @RefType AND R.[Value] = @RefValue

		IF (@@ROWCOUNT>0)
			RETURN 200;
	
		--Not found.
		RETURN 404;

	END TRY
	BEGIN CATCH
		SELECT  ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
		RETURN 500
	END CATCH
GO

