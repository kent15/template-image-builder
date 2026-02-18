using System.IO.Compression;
using ImageBatchGenerator.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ImageBatchGenerator.Infrastructure.Storage;

/// <summary>
/// ローカルファイルシステムストレージサービス実装
/// appsettings.json の Storage:OutputBasePath を出力先ルートとして使用する
/// </summary>
public class LocalStorageService : IStorageService
{
    private readonly string _outputBasePath;

    public LocalStorageService(IConfiguration configuration)
    {
        _outputBasePath = configuration["Storage:OutputBasePath"] ?? "data/output";
        Directory.CreateDirectory(_outputBasePath);
    }

    /// <summary>
    /// ファイルを保存し、相対ストレージパスを返す
    /// 保存先: {outputBasePath}/{subDirectory}/{fileName}
    /// </summary>
    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string subDirectory,
        CancellationToken ct = default)
    {
        var dir = Path.Combine(_outputBasePath, subDirectory);
        Directory.CreateDirectory(dir);

        var fullPath = Path.Combine(dir, fileName);
        await using var fs = new FileStream(
            fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true);
        await content.CopyToAsync(fs, ct);

        return Path.Combine(subDirectory, fileName);
    }

    /// <summary>
    /// ストレージパスからファイルを読み込み MemoryStream として返す（呼び出し元で Dispose 不要）
    /// </summary>
    public async Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_outputBasePath, storagePath);
        var ms = new MemoryStream();

        await using var fs = new FileStream(
            fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        await fs.CopyToAsync(ms, ct);

        ms.Position = 0;
        return ms;
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_outputBasePath, storagePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
        => Task.FromResult(File.Exists(Path.Combine(_outputBasePath, storagePath)));

    /// <summary>
    /// ジョブ出力ディレクトリの全ファイルを ZIP アーカイブとして返す
    /// </summary>
    public async Task<Stream> CreateZipArchiveAsync(Guid jobId, CancellationToken ct = default)
    {
        var jobDir = Path.Combine(_outputBasePath, jobId.ToString());
        var ms = new MemoryStream();

        using var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true);

        if (Directory.Exists(jobDir))
        {
            foreach (var filePath in Directory.GetFiles(jobDir).OrderBy(f => f))
            {
                ct.ThrowIfCancellationRequested();

                var entry = archive.CreateEntry(Path.GetFileName(filePath), CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await using var fs = new FileStream(
                    filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                await fs.CopyToAsync(entryStream, ct);
            }
        }

        ms.Position = 0;
        return ms;
    }
}
