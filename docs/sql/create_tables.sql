-- ============================================================
-- ImageBatchGenerator テーブル定義SQL
-- 対象DB: SQL Server 2025 Developer Edition
-- 文字コード: NVARCHAR（Unicode対応）
-- GUID: UNIQUEIDENTIFIER + NEWID()
-- タイムスタンプ: DATETIMEOFFSET（タイムゾーン付き）
-- ============================================================

USE [image_batch_db];
GO

-- ============================================================
-- 1. Templates（テンプレートマスタ）
-- ============================================================
CREATE TABLE [dbo].[Templates] (
    [Id]              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name]            NVARCHAR(255)    NOT NULL,
    [Description]     NVARCHAR(MAX),
    [FilePath]        NVARCHAR(1024)   NOT NULL,
    [ThumbnailPath]   NVARCHAR(1024),
    [Width]           INT              NOT NULL CHECK ([Width] > 0),
    [Height]          INT              NOT NULL CHECK ([Height] > 0),
    [LayerConfigJson] NVARCHAR(MAX)    NOT NULL,
    [Version]         INT              NOT NULL DEFAULT 1,
    [IsActive]        BIT              NOT NULL DEFAULT 1,
    [CreatedAt]       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [UpdatedAt]       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [DeletedAt]       DATETIMEOFFSET,
    CONSTRAINT [PK_Templates] PRIMARY KEY ([Id])
);
GO

-- ============================================================
-- 2. Assets（素材ライブラリ）
-- ============================================================
CREATE TABLE [dbo].[Assets] (
    [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name]          NVARCHAR(255)    NOT NULL,
    [Description]   NVARCHAR(MAX),
    [StoragePath]   NVARCHAR(1024)   NOT NULL,
    [FileName]      NVARCHAR(512)    NOT NULL,
    [ContentType]   NVARCHAR(128)    NOT NULL,
    [FileSizeBytes] BIGINT           NOT NULL CHECK ([FileSizeBytes] > 0),
    [Category]      NVARCHAR(64)     NOT NULL,  -- ProductImage / Background
    [IsActive]      BIT              NOT NULL DEFAULT 1,
    [CreatedAt]     DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [DeletedAt]     DATETIMEOFFSET,
    CONSTRAINT [PK_Assets] PRIMARY KEY ([Id])
);
GO

-- ============================================================
-- 3. Jobs（バッチジョブ管理）
-- ============================================================
CREATE TABLE [dbo].[Jobs] (
    [Id]                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name]                NVARCHAR(255)    NOT NULL,
    [TemplateId]          UNIQUEIDENTIFIER NOT NULL,
    [Status]              NVARCHAR(32)     NOT NULL DEFAULT 'Created',
    -- Created / Queued / Running / Completed / CompletedWithWarning / Failed / Cancelled
    [MappingRulesJson]    NVARCHAR(MAX)    NOT NULL,
    [OutputSettingsJson]  NVARCHAR(MAX)    NOT NULL,
    [CsvOriginalFileName] NVARCHAR(255),
    [CsvStoragePath]      NVARCHAR(1024),
    [TotalCount]          INT              NOT NULL DEFAULT 0,
    [SuccessCount]        INT              NOT NULL DEFAULT 0,
    [WarningCount]        INT              NOT NULL DEFAULT 0,
    [ErrorCount]          INT              NOT NULL DEFAULT 0,
    [SkippedCount]        INT              NOT NULL DEFAULT 0,
    [LastProcessedIndex]  INT              NOT NULL DEFAULT 0,   -- チェックポイント
    [RetryCount]          INT              NOT NULL DEFAULT 0,
    [OriginalJobId]       UNIQUEIDENTIFIER,                      -- 再実行元ジョブID
    [CreatedAt]           DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    [QueuedAt]            DATETIMEOFFSET,
    [StartedAt]           DATETIMEOFFSET,
    [CompletedAt]         DATETIMEOFFSET,
    [CreatedBy]           NVARCHAR(255),
    CONSTRAINT [PK_Jobs]             PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Jobs_Templates]   FOREIGN KEY ([TemplateId])   REFERENCES [Templates]([Id]),
    CONSTRAINT [FK_Jobs_OriginalJob] FOREIGN KEY ([OriginalJobId]) REFERENCES [Jobs]([Id])
);
GO

-- ============================================================
-- 4. JobItems（ジョブ明細：CSV1行 = 1件）
-- ============================================================
CREATE TABLE [dbo].[JobItems] (
    [Id]             UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [JobId]          UNIQUEIDENTIFIER NOT NULL,
    [RowIndex]       INT              NOT NULL CHECK ([RowIndex] >= 0),
    [Status]         NVARCHAR(32)     NOT NULL DEFAULT 'Pending',
    -- Pending / Running / Success / Warning / Error / Skipped
    [InputDataJson]  NVARCHAR(MAX),   -- CSV行データのスナップショット（再実行用）
    [WarningMessage] NVARCHAR(MAX),
    [RetryCount]     INT              NOT NULL DEFAULT 0,
    [ProcessedAt]    DATETIMEOFFSET,
    [ProcessingMs]   INT,
    CONSTRAINT [PK_JobItems]         PRIMARY KEY ([Id]),
    CONSTRAINT [FK_JobItems_Jobs]    FOREIGN KEY ([JobId]) REFERENCES [Jobs]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_JobItems_Job_Row] UNIQUE ([JobId], [RowIndex])
);
GO

-- ============================================================
-- 5. GeneratedImages（生成画像メタ情報）
-- ============================================================
CREATE TABLE [dbo].[GeneratedImages] (
    [Id]            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [JobItemId]     UNIQUEIDENTIFIER NOT NULL,
    [JobId]         UNIQUEIDENTIFIER NOT NULL,
    [FileName]      NVARCHAR(512)    NOT NULL,
    [StoragePath]   NVARCHAR(1024)   NOT NULL,
    [FileSizeBytes] BIGINT           NOT NULL CHECK ([FileSizeBytes] > 0),
    [Format]        NVARCHAR(16)     NOT NULL,   -- PNG / JPEG / WEBP
    [Width]         INT              NOT NULL CHECK ([Width] > 0),
    [Height]        INT              NOT NULL CHECK ([Height] > 0),
    [IsAvailable]   BIT              NOT NULL DEFAULT 1,
    [ExpiresAt]     DATETIMEOFFSET,
    [GeneratedAt]   DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_GeneratedImages]           PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GeneratedImages_JobItems]  FOREIGN KEY ([JobItemId]) REFERENCES [JobItems]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GeneratedImages_Jobs]      FOREIGN KEY ([JobId])     REFERENCES [Jobs]([Id]),
    CONSTRAINT [UQ_GeneratedImages_JobItemId] UNIQUE ([JobItemId])
);
GO

-- ============================================================
-- 6. ErrorLogs（エラーログ）
-- ============================================================
CREATE TABLE [dbo].[ErrorLogs] (
    [Id]          BIGINT           NOT NULL IDENTITY(1,1),
    [JobId]       UNIQUEIDENTIFIER NOT NULL,
    [JobItemId]   UNIQUEIDENTIFIER,
    [Level]       NVARCHAR(16)     NOT NULL,   -- Warning / Error / Critical
    [ErrorCode]   NVARCHAR(64),
    [Category]    NVARCHAR(64)     NOT NULL,
    [Message]     NVARCHAR(MAX)    NOT NULL,
    [StackTrace]  NVARCHAR(MAX),
    [ContextJson] NVARCHAR(MAX),
    [OccurredAt]  DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    CONSTRAINT [PK_ErrorLogs]          PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ErrorLogs_Jobs]     FOREIGN KEY ([JobId])     REFERENCES [Jobs]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ErrorLogs_JobItems] FOREIGN KEY ([JobItemId]) REFERENCES [JobItems]([Id])
);
GO

-- ============================================================
-- インデックス
-- ============================================================

-- Jobs
CREATE INDEX [IX_Jobs_Status_CreatedAt]  ON [dbo].[Jobs] ([Status], [CreatedAt] DESC);
CREATE INDEX [IX_Jobs_Status_QueuedAt]   ON [dbo].[Jobs] ([Status], [QueuedAt]) WHERE [Status] = 'Queued';
CREATE INDEX [IX_Jobs_TemplateId]        ON [dbo].[Jobs] ([TemplateId], [CreatedAt] DESC);

-- JobItems
CREATE INDEX [IX_JobItems_JobId_Status_RowIndex] ON [dbo].[JobItems] ([JobId], [Status], [RowIndex]);
CREATE INDEX [IX_JobItems_JobId_Error]           ON [dbo].[JobItems] ([JobId], [RowIndex]) WHERE [Status] = 'Error';

-- GeneratedImages
CREATE INDEX [IX_GeneratedImages_JobId]      ON [dbo].[GeneratedImages] ([JobId]) WHERE [IsAvailable] = 1;
CREATE INDEX [IX_GeneratedImages_ExpiresAt]  ON [dbo].[GeneratedImages] ([ExpiresAt]) WHERE [ExpiresAt] IS NOT NULL;

-- ErrorLogs
CREATE INDEX [IX_ErrorLogs_JobId_OccurredAt] ON [dbo].[ErrorLogs] ([JobId], [OccurredAt] DESC);
CREATE INDEX [IX_ErrorLogs_Level_OccurredAt] ON [dbo].[ErrorLogs] ([Level], [OccurredAt] DESC);
GO
