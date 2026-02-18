using ImageBatchGenerator.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ImageBatchGenerator.Infrastructure.Storage;

/// <summary>
/// ローカルファイルシステムストレージサービス実装
/// appsettings.jsonのStorage設定から出力先を取得する
/// </summary>
public class LocalStorageService : IStorageService
{
    // TODO: Phase 2でIConfigurationから設定を注入
    private readonly string _outputBasePath;
    private readonly string _csvUploadPath;
    private readonly string _assetPath;

    public LocalStorageService(IConfiguration configuration)
    {
        // TODO: Phase 2で実装
        // _outputBasePath = configuration["Storage:OutputBasePath"] ?? "C:\\ImageBatch\\Output";
        // _csvUploadPath  = configuration["Storage:CsvUploadPath"]  ?? "C:\\ImageBatch\\Csv";
        // _assetPath      = configuration["Storage:AssetPath"]      ?? "C:\\ImageBatch\\Assets";
        _outputBasePath = string.Empty;
        _csvUploadPath = string.Empty;
        _assetPath = string.Empty;
    }

    public async Task<string> SaveAsync(
        Stream content,
        string fileName,
        string subDirectory,
        CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        // 1. サブディレクトリを作成する
        // 2. ファイル名の重複を防ぐためGUID+元ファイル名で保存
        // 3. ストレージパス（相対パス）を返す
        throw new NotImplementedException();
    }

    public async Task<Stream> ReadAsync(string storagePath, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
    {
        // TODO: Phase 2で実装
        throw new NotImplementedException();
    }

    public async Task<Stream> CreateZipArchiveAsync(Guid jobId, CancellationToken ct = default)
    {
        // TODO: Phase 3で実装
        // System.IO.Compression.ZipArchiveを使用して生成画像をZIPにまとめる
        throw new NotImplementedException();
    }
}
