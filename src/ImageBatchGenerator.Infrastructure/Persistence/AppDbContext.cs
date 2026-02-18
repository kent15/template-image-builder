using ImageBatchGenerator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageBatchGenerator.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext
/// SQL Server 2025 Developer Edition 接続
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs { get; set; } = null!;
    public DbSet<JobItem> JobItems { get; set; } = null!;
    public DbSet<Template> Templates { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<GeneratedImage> GeneratedImages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TODO: Phase 1でエンティティ設定を実装
        // 各エンティティのテーブル名・カラム型・制約・インデックスをDB設計に従い定義する

        ConfigureTemplate(modelBuilder);
        ConfigureJob(modelBuilder);
        ConfigureJobItem(modelBuilder);
        ConfigureGeneratedImage(modelBuilder);
        ConfigureAsset(modelBuilder);
    }

    private static void ConfigureTemplate(ModelBuilder modelBuilder)
    {
        // TODO: Phase 1で実装
        // テーブル名: Templates
        // PK: Id (UNIQUEIDENTIFIER)
        // カラム型: NVARCHAR, INT, BIT, DATETIMEOFFSET
        // インデックス: なし
    }

    private static void ConfigureJob(ModelBuilder modelBuilder)
    {
        // TODO: Phase 1で実装
        // テーブル名: Jobs
        // PK: Id (UNIQUEIDENTIFIER)
        // FK: TemplateId → Templates.Id
        // FK: OriginalJobId → Jobs.Id (自己参照)
        // インデックス: Status+CreatedAt, Status+QueuedAt(Queued限定), TemplateId+CreatedAt
    }

    private static void ConfigureJobItem(ModelBuilder modelBuilder)
    {
        // TODO: Phase 1で実装
        // テーブル名: JobItems
        // PK: Id (UNIQUEIDENTIFIER)
        // FK: JobId → Jobs.Id ON DELETE CASCADE
        // UQ: JobId+RowIndex
        // インデックス: JobId+Status+RowIndex, JobId+RowIndex(Error限定)
    }

    private static void ConfigureGeneratedImage(ModelBuilder modelBuilder)
    {
        // TODO: Phase 1で実装
        // テーブル名: GeneratedImages
        // PK: Id (UNIQUEIDENTIFIER)
        // FK: JobItemId → JobItems.Id ON DELETE CASCADE
        // FK: JobId → Jobs.Id
        // UQ: JobItemId
        // インデックス: JobId(IsAvailable=1限定), ExpiresAt(NotNull限定)
    }

    private static void ConfigureAsset(ModelBuilder modelBuilder)
    {
        // TODO: Phase 2で実装
        // テーブル名: Assets
        // PK: Id (UNIQUEIDENTIFIER)
    }
}
