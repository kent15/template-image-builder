# 画像レイアウト自動生成ツール

テンプレートベースの画像レイアウト自動生成Webアプリケーション。  
テキスト・背景・商品画像を差し替え、500〜3,000件を一括バッチ処理する。

---

## 目次

1. [プロジェクト概要](#1-プロジェクト概要)
2. [技術スタック](#2-技術スタック)
3. [開発環境・実行環境](#3-開発環境実行環境)
4. [ディレクトリ構成](#4-ディレクトリ構成)
5. [開発フェーズ分割方針](#5-開発フェーズ分割方針)
6. [要件定義](#6-要件定義)
7. [UI設計](#7-ui設計)
8. [アーキテクチャ設計](#8-アーキテクチャ設計)
9. [DB設計](#9-db設計)
10. [環境構築手順](#10-環境構築手順)
11. [Claude Codeへの引き継ぎ情報](#11-claude-codeへの引き継ぎ情報)

---

## 1. プロジェクト概要

### 背景・目的

- テンプレート型画像生成の自動化
- テキスト・背景・商品画像の差し替えを一括処理
- 500〜3,000件規模のバッチ処理をWeb UIから操作可能にする

### 主要機能サマリー

| 機能 | 概要 |
|------|------|
| テンプレート管理 | SVG/JSON形式のテンプレート登録・編集・プレビュー |
| 素材ライブラリ | 商品画像・背景のZIP一括アップロード・管理 |
| CSVアップロード | テキスト差し替えデータのCSV投入・マッピング設定 |
| ジョブ作成 | ウィザード形式で4ステップ作成 |
| バッチ処理 | 非同期並列処理・途中停止・再開・エラー分のみ再実行 |
| 進捗確認 | SignalRによるリアルタイム進捗表示 |
| ログ閲覧 | ジョブ・明細単位の構造化ログ検索 |
| 出力 | ZIP一括ダウンロード / ローカルフォルダ出力 |

---

## 2. 技術スタック

| 区分 | 採用技術 | 選定理由 |
|------|---------|---------|
| Webフレームワーク | ASP.NET Core 8 | 既存スタックとの一貫性・LTS |
| 言語 | C# 12 | 本業スタックと統一 |
| ORM | EF Core 8 | Code First・マイグレーション管理 |
| **DB** | **SQL Server 2025 Developer Edition** | **Windowsネイティブ・既存インストール済み・全機能無償** |
| 画像処理 | SkiaSharp（MIT License） | 商用無償・クロスプラットフォーム |
| リアルタイム通知 | SignalR（ASP.NET Core組込） | 追加コストなし |
| キュー（MVP） | System.Threading.Channels | インプロセス・追加依存なし |
| ログ | Serilog | 構造化ログ・Sink拡張性 |
| DI | Microsoft.Extensions.DI（組込） | 標準・追加コストなし |
| テスト | xUnit + Moq | 標準的なC#テスト構成 |
| GUIツール | SSMS（SQL Server Management Studio） | DB管理・デバッグ用 |

### NuGetパッケージ（主要）

```
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.EntityFrameworkCore.Tools
SkiaSharp
SkiaSharp.HarfBuzz
Microsoft.AspNetCore.SignalR
Serilog.AspNetCore
Serilog.Sinks.MSSqlServer
```

---

## 3. 開発環境・実行環境

| 項目 | 内容 |
|------|------|
| OS | Windows 10/11（ネイティブ） |
| IDE | Visual Studio 2022 |
| ランタイム | .NET 8 SDK |
| DB | SQL Server 2025 Developer Edition（localhost・デフォルトインスタンス） |
| 実行形態 | ローカルPC個人利用 |
| コンテナ | 不使用 |

### DB接続文字列

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=image_batch_db;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 注意事項

> ⚠️ **日本語ユーザー名環境での注意**  
> PostgreSQLのWindowsネイティブインストールは日本語ユーザー名によるパスエンコード問題で失敗するため、本プロジェクトではSQL Serverを採用している。

---

## 4. ディレクトリ構成

```
ImageBatchGenerator/
├── README.md
├── ImageBatchGenerator.sln
│
├── src/
│   ├── ImageBatchGenerator.Domain/              # ドメイン層（依存なし）
│   │   ├── Entities/
│   │   │   ├── Job.cs
│   │   │   ├── JobItem.cs
│   │   │   ├── Template.cs
│   │   │   └── Asset.cs
│   │   ├── ValueObjects/
│   │   │   ├── JobStatus.cs
│   │   │   ├── JobItemStatus.cs
│   │   │   ├── OutputFormat.cs
│   │   │   └── MappingRule.cs
│   │   ├── Events/
│   │   │   ├── JobStartedEvent.cs
│   │   │   ├── JobItemCompletedEvent.cs
│   │   │   └── JobCompletedEvent.cs
│   │   └── Interfaces/
│   │       ├── IJobRepository.cs
│   │       ├── IJobItemRepository.cs
│   │       ├── ITemplateRepository.cs
│   │       ├── IAssetRepository.cs
│   │       ├── IImageProcessor.cs
│   │       └── IStorageService.cs
│   │
│   ├── ImageBatchGenerator.Application/         # アプリケーション層
│   │   ├── UseCases/
│   │   │   ├── Jobs/
│   │   │   │   ├── CreateJobUseCase.cs
│   │   │   │   ├── StartJobUseCase.cs
│   │   │   │   ├── CancelJobUseCase.cs
│   │   │   │   ├── RetryJobUseCase.cs
│   │   │   │   └── GetJobProgressUseCase.cs
│   │   │   ├── Templates/
│   │   │   │   └── RegisterTemplateUseCase.cs
│   │   │   └── Assets/
│   │   │       └── UploadAssetUseCase.cs
│   │   ├── DTOs/
│   │   │   ├── JobDto.cs
│   │   │   ├── JobProgressDto.cs
│   │   │   └── CreateJobRequest.cs
│   │   ├── Orchestration/
│   │   │   ├── JobOrchestrator.cs
│   │   │   └── BatchCoordinator.cs
│   │   └── Interfaces/
│   │       ├── IJobQueue.cs
│   │       └── IProgressNotifier.cs
│   │
│   ├── ImageBatchGenerator.Infrastructure/      # インフラ層
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── JobRepository.cs
│   │   │   │   └── JobItemRepository.cs
│   │   │   └── Migrations/
│   │   ├── ImageProcessing/
│   │   │   └── SkiaSharpImageProcessor.cs
│   │   ├── Storage/
│   │   │   └── LocalStorageService.cs
│   │   ├── Queue/
│   │   │   └── InMemoryJobQueue.cs
│   │   └── Notifications/
│   │       └── SignalRProgressNotifier.cs
│   │
│   └── ImageBatchGenerator.Web/                 # プレゼンテーション層
│       ├── Controllers/
│       │   ├── JobsController.cs
│       │   ├── TemplatesController.cs
│       │   └── AssetsController.cs
│       ├── Hubs/
│       │   └── ProgressHub.cs
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── BackgroundServices/
│       │   └── JobQueueWorker.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
├── tests/
│   ├── ImageBatchGenerator.UnitTests/
│   │   ├── Domain/
│   │   └── Application/
│   └── ImageBatchGenerator.IntegrationTests/
│
└── docs/
    ├── wireframes/                              # 画面ワイヤーフレーム
    └── sql/                                     # テーブル定義SQL
        └── create_tables.sql
```

---

## 5. 開発フェーズ分割方針

Claude Codeへの引き渡し・実装作業を効率化するため、以下のフェーズで段階的に開発する。

### フェーズ概要

| フェーズ | 名称 | 主な実装内容 | 完了目安 |
|---------|------|------------|---------|
| Phase 1 | 基盤構築 | プロジェクト作成・DB接続・EFマイグレーション・ヘルスチェックAPI | 1〜2日 |
| Phase 2 | コア機能MVP | テンプレート管理・素材アップロード・CSVアップロード・ジョブ作成API | 3〜5日 |
| Phase 3 | バッチ処理 | 並列処理・途中停止・再開・チェックポイント・SignalR進捗通知 | 3〜5日 |
| Phase 4 | Web UI | ジョブ作成ウィザード・進捗画面・ログビューア | 3〜5日 |
| Phase 5 | 品質向上 | エラーハンドリング強化・ユニットテスト・パフォーマンス検証 | 2〜3日 |

### Phase 1：基盤構築（最初に着手）

```
✅ ゴール: dotnet run でAPIが起動し、SSMSからDBに接続できる状態

実装項目:
- ソリューション・プロジェクト作成（Clean Architecture構成）
- NuGetパッケージ導入
- AppDbContext作成
- 全テーブルのEFエンティティ定義
- Initial Migration実行・DB作成
- ヘルスチェックエンドポイント（GET /health）
- appsettings.json 接続文字列設定
```

### Phase 2：コア機能MVP

```
✅ ゴール: テンプレート登録→CSV投入→ジョブ作成までAPIで完結する状態

実装項目:
- テンプレートCRUD API
- 素材アップロードAPI（ローカルフォルダ保存）
- CSVアップロード・パース・JobItem一括生成
- ジョブ作成API（マッピング設定保存）
- ジョブ一覧・詳細取得API
```

### Phase 3：バッチ処理

```
✅ ゴール: 100件のサンプルCSVで画像生成が完走する状態

実装項目:
- IHostedService（JobQueueWorker）実装
- System.Threading.Channels によるInMemoryキュー
- SkiaSharpImageProcessor（テキスト・画像・背景差し替え）
- 並列処理（Parallel.ForEachAsync・SemaphoreSlim）
- チェックポイント保存（LastProcessedIndex）
- キャンセルAPI（CancellationTokenRegistry）
- SignalR進捗通知（ProgressHub）
```

### Phase 4：Web UI

```
✅ ゴール: ブラウザからジョブ作成〜進捗確認〜DLが完結する状態

実装項目:
- ジョブ作成ウィザード（4ステップ）
- テンプレート選択・素材ライブラリ画面
- リアルタイム進捗バー（SignalR接続）
- ジョブ一覧・ログビューア
- ZIP生成・ダウンロード
```

### Phase 5：品質向上

```
✅ ゴール: 3,000件バッチが安定完走・エラー再実行が動作する状態

実装項目:
- エラー分のみ再実行フロー
- ExceptionHandlingMiddleware
- Serilogによる構造化ログ（SQL Serverシンク）
- ユニットテスト（UseCases・Domain）
- 3,000件負荷テスト・チューニング
```

---

## 6. 要件定義

### 6-1. 機能要件一覧

#### テンプレート管理

| No | 機能 | 説明 |
|----|------|------|
| F-001 | テンプレート登録 | SVG/JSON形式のテンプレートをアップロード・保存 |
| F-002 | テンプレート編集 | レイヤー構成（テキスト・背景・商品画像）の差し替え領域を定義 |
| F-003 | テンプレートプレビュー | 編集内容をリアルタイムプレビュー表示 |
| F-004 | テンプレートバージョン管理 | 変更履歴の保持・ロールバック |

#### 素材管理

| No | 機能 | 説明 |
|----|------|------|
| F-010 | 素材一括アップロード | 商品画像・背景画像のZIP一括投入 |
| F-011 | CSVインポート | テキスト差し替えデータをCSV形式で投入 |
| F-012 | 素材ライブラリ | アップロード済み素材の一覧・検索・再利用 |

#### 生成・処理

| No | 機能 | 説明 |
|----|------|------|
| F-020 | 一括生成実行 | 500〜3,000件を非同期バッチで処理 |
| F-021 | 生成プレビュー（サンプル） | 全件実行前に先頭3件をサンプル生成して確認 |
| F-022 | 差し替えルール設定 | テキスト・画像とCSV列のマッピング定義 |
| F-023 | 生成オプション | 出力サイズ・フォーマット（JPEG/PNG/WebP）・品質設定 |
| F-024 | AI補助機能 | テキストの自動リサイズ・画像の自動トリミング |

#### ジョブ管理

| No | 機能 | 説明 |
|----|------|------|
| F-030 | ジョブ一覧 | 実行中・完了・失敗ジョブの一覧表示 |
| F-031 | 進捗表示 | リアルタイム進捗バー・処理件数表示 |
| F-032 | エラーハンドリング | 失敗件数・エラー詳細レポート・部分再実行 |
| F-033 | キャンセル機能 | 実行中ジョブの途中停止 |

#### 出力・ダウンロード

| No | 機能 | 説明 |
|----|------|------|
| F-040 | ZIP一括ダウンロード | 生成画像をZIPアーカイブでダウンロード |
| F-041 | ローカルフォルダ出力 | 指定フォルダへの自動エクスポート |
| F-042 | ファイル命名規則設定 | CSV項目を組み合わせた動的ファイル名生成 |

### 6-2. 非機能要件

- 1件あたりの画像生成処理：**2秒以内**（標準テンプレート）
- 3,000件バッチ：**30分以内**に完了（並列ワーカー構成前提）
- Web UI初期表示：**3秒以内**
- 稼働率：ローカル運用のため定義なし
- バッチ処理中の障害時：**自動リトライ（最大3回）** および中断ポイントからの再開

### 6-3. 制約条件

| 区分 | 内容 |
|------|------|
| 技術スタック | ASP.NET Core 8 / C# 12 |
| DB | SQL Server 2025 Developer Edition（Windowsネイティブ） |
| 画像処理 | SkiaSharp（MIT License・商用無償） |
| ファイルサイズ上限 | 1素材あたり最大50MB、CSVは最大5MB |
| 同時実行 | バッチジョブの同時実行は最大2ジョブ |
| 対応ブラウザ | Chrome / Edge 最新版 |
| 実行環境 | Windows 10/11 ネイティブ（Dockerなし） |

### 6-4. 想定リスク

| リスク | 影響度 | 対策 |
|--------|--------|------|
| 大量処理時のメモリ枯渇 | 高 | ストリーム処理・チャンク分割でメモリ使用量を制御 |
| テンプレート構造の複雑化による処理遅延 | 中 | テンプレート複雑度の上限定義・事前バリデーション |
| ファイルアップロードによるセキュリティ脆弱性 | 高 | MIMEタイプ検証・拡張子チェック |
| ストレージ容量の急増 | 中 | 生成ファイルの自動TTL削除・容量クォータ設定 |
| CSVデータ不備による大量エラー | 中 | 実行前バリデーション＋サンプルプレビューで事前検出 |

### 6-5. スケーラビリティ方針

本プロジェクトはローカル個人利用を前提とするため、以下の段階的対応とする。

| フェーズ | 処理規模 | 構成 |
|---------|---------|------|
| MVP | 〜500件 | シングルプロセス＋InMemoryキュー |
| 拡張 | 〜3,000件 | 並列ワーカー数調整（appsettingsで制御） |
| 将来 | 3,000件超 | RabbitMQ導入・Workerプロセス分離 |

---

## 7. UI設計

### 7-1. 画面一覧

| 画面ID | 画面名 | 主な役割 |
|--------|--------|---------|
| SCR-001 | ダッシュボード | ジョブ全体の状態サマリー・クイックアクセス |
| SCR-002 | テンプレート管理 | テンプレート一覧・登録・プレビュー |
| SCR-003 | 素材ライブラリ | 画像素材の一覧・アップロード・管理 |
| SCR-004 | ジョブ作成ウィザード | テンプレ選択→素材・CSV紐付け→実行設定→確認 |
| SCR-005 | ジョブ一覧 | 全ジョブの状態確認・操作 |
| SCR-006 | ジョブ詳細／進捗 | リアルタイム進捗・エラー詳細・ログ |
| SCR-007 | ログビューア | 全ジョブの統合ログ・フィルタ検索 |
| SCR-008 | 設定 | 出力先・通知・ユーザー管理 |

### 7-2. 各画面ワイヤー構成

#### SCR-001：ダッシュボード

```
┌─────────────────────────────────────────────────────┐
│ [ロゴ]  ダッシュボード  ジョブ  テンプレート  ログ  設定 │
├─────────────────────────────────────────────────────┤
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌────────┐  │
│  │ 実行中    │ │ 完了(今日)│ │ エラー   │ │ 総生成数│  │
│  │    2     │ │   1,240  │ │    3    │ │ 48,320 │  │
│  └──────────┘ └──────────┘ └──────────┘ └────────┘  │
│  ┌─────────────────────────┐  ┌────────────────────┐ │
│  │ 直近ジョブ               │  │ クイックアクション  │ │
│  │ ● JOB-042  実行中 68%   │  │  [+ 新規ジョブ作成] │ │
│  │ ● JOB-041  完了         │  │  [テンプレート追加] │ │
│  │ ✕ JOB-040  エラー       │  │  [素材アップロード] │ │
│  └─────────────────────────┘  └────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

#### SCR-004：ジョブ作成ウィザード（4ステップ）

```
① テンプレート選択 ━━ ② 素材・CSV設定 ━━ ③ 出力設定 ━━ ④ 確認・実行
```

**STEP 2：素材・CSV設定**

```
┌──────────────────────────────────────────────────────┐
│ CSVアップロード                                        │
│ ┌────────────────────────────────────────────────┐   │
│ │  CSVをドロップ or [ファイルを選択]               │   │
│ │  ※ UTF-8 / BOM付きUTF-8 対応 / 最大5MB          │   │
│ └────────────────────────────────────────────────┘   │
│ ✅ products.csv 読込完了（1,500行検出）                │
│                                                       │
│ マッピング設定                                         │
│ ┌──────────────┬─────────────────────────────────┐   │
│ │ レイヤー      │ CSV列 / 素材                    │   │
│ │ 📝 商品名     │ [product_name 列▼]              │   │
│ │ 🖼 商品画像   │ [素材ライブラリから選択▼]        │   │
│ │ 🎨 背景      │ [固定素材: bg_001 ▼]            │   │
│ └──────────────┴─────────────────────────────────┘   │
│                              [← 戻る]  [次へ →]       │
└──────────────────────────────────────────────────────┘
```

**STEP 4：確認・実行（サンプルプレビュー付き）**

```
┌──────────────────────────────────────────────────────┐
│ サンプルプレビュー（先頭3件）                           │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐             │
│  │[preview] │ │[preview] │ │[preview] │             │
│  └──────────┘ └──────────┘ └──────────┘             │
│  ⚠ item_002：テキストがはみ出しています（要確認）       │
│                  [← 戻る]  [▶ ジョブ実行]             │
└──────────────────────────────────────────────────────┘
```

#### SCR-006：ジョブ詳細／進捗

```
┌──────────────────────────────────────────────────────┐
│ #042 商品春夏2026                         [キャンセル] │
├──────────────────────────────────────────────────────┤
│ ████████████████░░░░░░░░  68%  (1,020 / 1,500件)     │
│ 残り時間: 約 4分32秒  |  処理速度: 3.7件/秒            │
│ 成功: 1,017  ⚠ 警告: 3  ❌ エラー: 0                 │
├──────────────────────────────────────────────────────┤
│ ⚠ [14:35:12] item_0312：テキストオーバーフロー（自動縮小済）│
│ ⚠ [14:34:58] item_0287：画像未登録 → デフォルト差替   │
│                                      [全ログを見る →] │
└──────────────────────────────────────────────────────┘
```

### 7-3. 操作フロー

```
[ダッシュボード]
    ├─→ [テンプレート管理] ─→ テンプレート登録・確認
    ├─→ [素材ライブラリ]   ─→ 素材アップロード・整理
    └─→ [ジョブ作成ウィザード]
            STEP1: テンプレート選択
            STEP2: 素材・CSVアップロード＋マッピング設定
            STEP3: 出力形式・命名規則・出力先設定
            STEP4: サンプルプレビュー確認 → 実行
            ▼
    [ジョブ詳細／進捗] ─→ リアルタイム監視
            ├─ 完了 ─→ ZIPダウンロード
            └─ エラー ─→ [ログ確認] ─→ [エラー分のみ再実行]
```

### 7-4. エラー発生時UX

| 分類 | 例 | 対応 |
|------|-----|------|
| 入力エラー（実行前） | CSV列が未マッピング | ウィザード上でインライン表示・実行ボタン非活性 |
| 警告（処理続行） | テキストオーバーフロー | 自動フォールバック＋警告ログ記録・処理継続 |
| 部分エラー（件単位） | 画像ファイル名不一致 | 該当件スキップ＋エラー集計・正常分は継続 |
| 致命的エラー（停止） | テンプレート破損 | ジョブ停止＋原因・復旧手順をモーダル表示 |

### 7-5. バッチ進捗表示設計

| 要素 | 内容 | 更新頻度 |
|------|------|---------|
| プログレスバー | 視覚的な達成率（アニメーション付き） | 1秒ごと |
| 数値カウンター | 処理済み / 総件数 | 1秒ごと |
| 残り時間 | 移動平均で算出（直近30件の処理速度から推定） | 5秒ごと |
| 状態別カウント | 成功・警告・エラーを色分け表示 | リアルタイム |

更新方式：**SignalRによるリアルタイムプッシュ**。切断時はポーリング（5秒間隔）へ自動フォールバック。

---

## 8. アーキテクチャ設計

### 8-1. 全体構成図

```
┌─────────────────────────────────────────────────────────────┐
│                      クライアント層                           │
│              ブラウザ（Blazor Server / React）                │
│         WebSocket（SignalR）─── REST API（fetch）            │
└────────────────────────┬────────────────────────────────────┘
                         │ HTTPS
┌────────────────────────▼────────────────────────────────────┐
│                   プレゼンテーション層                         │
│                ASP.NET Core Web API                          │
│       Controllers / SignalR Hubs / Middleware                │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    アプリケーション層                          │
│       UseCases / Commands / Queries / DTOs                   │
│            JobOrchestrator / BatchCoordinator                │
└──────┬──────────────────┬──────────────────┬────────────────┘
       │                  │                  │
┌──────▼──────┐  ┌────────▼──────┐  ┌───────▼────────────────┐
│ ドメイン層   │  │  インフラ層    │  │ バックグラウンドサービス  │
│ Entities    │  │ EF Core       │  │ IHostedService          │
│ ValueObjects│  │ SQL Server    │  │ Worker（並列処理）        │
│ DomainEvents│  │ ローカル      │  │ SignalR進捗通知          │
│ Interfaces  │  │ ストレージ    │  │ ジョブキュー監視         │
└─────────────┘  └───────────────┘  └────────────────────────┘
```

### 8-2. レイヤー構成と依存方向

```
Domain ← Application ← Infrastructure
               ↑
          Presentation
```

| レイヤー | 責務 | 外部依存 |
|---------|------|---------|
| Domain | ビジネスルール・エンティティ定義 | なし（純粋なC#） |
| Application | ユースケース調整・フロー制御 | Domainのみ |
| Infrastructure | DB・ストレージ・画像処理の実装 | Application（IFのみ） |
| Presentation | HTTP/WebSocket・DI組み立て | Application |

### 8-3. ジョブ管理方式

#### ジョブのライフサイクル

```
Created → Queued → Running → Completed
                       │         ↑
                       ↓         │ Retry（エラー分のみ）
                   Cancelled  Failed
```

#### チェックポイント方式（途中停止・再開）

```
通常実行:
  Job.LastProcessedIndex を100件ごとにDB更新（チェックポイント）
  └─ 障害発生 → 再起動後に LastProcessedIndex+1 から再開

エラー再実行:
  JobItem.Status = Error のみ抽出して再実行
  └─ Job.Status を Running に戻し、エラー件数をリセット
```

### 8-4. 並列処理設計

```csharp
// BatchCoordinator の基本構造
var channel = Channel.CreateBounded<JobItem>(parallelism * 2);

// Producer：DBからバッチ単位でフェッチ
var producer = ProduceJobItemsAsync(job, channel.Writer, ct);

// Consumer：並列ワーカーでChannel購読・画像生成
var consumers = Enumerable
    .Range(0, parallelism)
    .Select(_ => ConsumeAsync(channel.Reader, job, ct))
    .ToArray();

await Task.WhenAll(producer);
channel.Writer.Complete();
await Task.WhenAll(consumers);
```

#### 並列数の推奨設定

| 環境 | MaxDegreeOfParallelism |
|------|----------------------|
| 開発（ローカル） | 2 |
| 本番（標準） | 4 |

### 8-5. キュー方式（InMemory・MVP構成）

```csharp
public interface IJobQueue
{
    Task EnqueueAsync(Guid jobId, CancellationToken ct = default);
    Task<Guid?> DequeueAsync(CancellationToken ct = default);
    Task<int> GetQueueLengthAsync();
}
// MVP実装: System.Threading.Channels
// 将来拡張: RabbitMqJobQueue（IJobQueueの差し替えのみで対応可能）
```

### 8-6. ログ設計

**フレームワーク：Serilog**（構造化ログ・JSON形式）

```json
{
  "Timestamp": "2026-02-18T14:35:12.441+09:00",
  "Level": "Warning",
  "Properties": {
    "JobId": "3fa85f64-...",
    "RowIndex": 312,
    "WarningType": "TextOverflow",
    "Detail": "テキストを92%に自動縮小しました"
  }
}
```

| レベル | 用途 |
|-------|------|
| Information | ジョブ開始・件単位完了・ジョブ完了 |
| Warning | テキストオーバーフロー・画像未登録 |
| Error | 件単位の処理失敗 |
| Critical | ジョブ停止を伴う致命的障害 |

---

## 9. DB設計

### 9-1. ER構造

```
Templates
  │ 1:N
  ▼
Jobs ──────────────────────────────┐
  │ 1:N                          1:N
  ▼                               ▼
JobItems                      ErrorLogs
  │ 1:1（成功時のみ）
  ▼
GeneratedImages
```

### 9-2. テーブル定義

#### Templates（テンプレートマスタ）

```sql
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
```

#### Jobs（バッチジョブ管理）

```sql
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
```

#### JobItems（ジョブ明細）

```sql
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
```

#### GeneratedImages（生成画像メタ情報）

```sql
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
```

#### ErrorLogs（エラーログ）

```sql
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
```

### 9-3. インデックス設計

```sql
-- Jobs
CREATE INDEX [IX_Jobs_Status_CreatedAt]  ON [Jobs] ([Status], [CreatedAt] DESC);
CREATE INDEX [IX_Jobs_Status_QueuedAt]   ON [Jobs] ([Status], [QueuedAt]) WHERE [Status] = 'Queued';
CREATE INDEX [IX_Jobs_TemplateId]        ON [Jobs] ([TemplateId], [CreatedAt] DESC);

-- JobItems
CREATE INDEX [IX_JobItems_JobId_Status_RowIndex] ON [JobItems] ([JobId], [Status], [RowIndex]);
CREATE INDEX [IX_JobItems_JobId_Error]           ON [JobItems] ([JobId], [RowIndex]) WHERE [Status] = 'Error';

-- GeneratedImages
CREATE INDEX [IX_GeneratedImages_JobId]          ON [GeneratedImages] ([JobId]) WHERE [IsAvailable] = 1;
CREATE INDEX [IX_GeneratedImages_ExpiresAt]      ON [GeneratedImages] ([ExpiresAt]) WHERE [ExpiresAt] IS NOT NULL;

-- ErrorLogs
CREATE INDEX [IX_ErrorLogs_JobId_OccurredAt]     ON [ErrorLogs] ([JobId], [OccurredAt] DESC);
CREATE INDEX [IX_ErrorLogs_Level_OccurredAt]     ON [ErrorLogs] ([Level], [OccurredAt] DESC);
```

### 9-4. 再実行を考慮した設計方針

| 再実行種別 | トリガー | DB操作 |
|-----------|---------|--------|
| チェックポイント再開 | プロセス障害後の自動復旧 | `LastProcessedIndex`以降のPending件を再処理 |
| エラー分のみ再実行 | ユーザーが手動で指示 | `Status = 'Error'`のJobItemのみ`Pending`に戻す |
| ジョブ全体の再実行 | ユーザーが全件やり直し | 新Jobを作成・`OriginalJobId`に元JobのIdを設定 |

**InputDataJsonによるCSV非依存の再実行保証**：元CSVが削除・上書きされた後でも`JobItems.InputDataJson`のスナップショットから再処理可能。

---

## 10. 環境構築手順

### 前提条件

- [x] Windows 10/11
- [x] Visual Studio 2022
- [x] .NET 8 SDK
- [x] SQL Server 2025 Developer Edition（`localhost`・デフォルトインスタンス）
- [x] SSMS（SQL Server Management Studio）

### セットアップ手順

**① リポジトリクローン**

```bash
git clone https://github.com/your-repo/ImageBatchGenerator.git
cd ImageBatchGenerator
```

**② appsettings.Development.json の作成**

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=image_batch_db;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Storage": {
    "OutputBasePath": "C:\\ImageBatch\\Output",
    "CsvUploadPath":  "C:\\ImageBatch\\Csv",
    "AssetPath":      "C:\\ImageBatch\\Assets"
  },
  "BatchSettings": {
    "MaxDegreeOfParallelism": 2,
    "MaxConcurrentJobs": 2,
    "RetryLimit": 3
  }
}
```

**③ 出力フォルダの作成**

```powershell
New-Item -ItemType Directory -Force -Path "C:\ImageBatch\Output"
New-Item -ItemType Directory -Force -Path "C:\ImageBatch\Csv"
New-Item -ItemType Directory -Force -Path "C:\ImageBatch\Assets"
```

**④ EF Coreマイグレーション実行**

```powershell
cd src/ImageBatchGenerator.Web
dotnet ef database update
```

**⑤ 起動確認**

```powershell
dotnet run
# → http://localhost:5000/health が 200 OK を返せばセットアップ完了
```

---

## 11. Claude Codeへの引き継ぎ情報

### プロジェクト概要（要約）

ASP.NET Core 8 + SQL Server 2025 + SkiaSharp によるテンプレートベース画像バッチ生成ツール。  
Clean Architecture構成。最初のタスクはPhase 1（基盤構築）から着手する。

### 最初に実装すべきタスク（Phase 1）

```
1. ソリューション作成
   dotnet new sln -n ImageBatchGenerator
   dotnet new webapi -n ImageBatchGenerator.Web
   dotnet new classlib -n ImageBatchGenerator.Domain
   dotnet new classlib -n ImageBatchGenerator.Application
   dotnet new classlib -n ImageBatchGenerator.Infrastructure

2. NuGetパッケージ追加（Web・Infrastructureプロジェクト）
   Microsoft.EntityFrameworkCore.SqlServer
   Microsoft.EntityFrameworkCore.Tools
   SkiaSharp
   SkiaSharp.HarfBuzz
   Microsoft.AspNetCore.SignalR
   Serilog.AspNetCore
   Serilog.Sinks.MSSqlServer

3. EFエンティティ・AppDbContext作成（上記DB設計に従う）

4. Initial Migration実行・DB作成

5. GET /health エンドポイント実装・動作確認
```

### 実装時の重要方針

- **既存コードを尊重する**：差分修正を基本とし、全書き直しは行わない
- **Clean Architectureの依存方向を厳守**：Domain → Application → Infrastructure の順
- **DB型はSQL Server準拠**：`UNIQUEIDENTIFIER`・`NVARCHAR`・`DATETIMEOFFSET`・`IDENTITY(1,1)`
- **PostgreSQL固有の記法は使用しない**：`gen_random_uuid()`→`NEWID()`、`BIGSERIAL`→`BIGINT IDENTITY(1,1)`
- **JSON列はNVARCHAR(MAX)**：`JSON_VALUE()`・`OPENJSON()`で操作
- **並列数はappsettingsで制御**：ハードコードしない
- **エラーは握りつぶさない**：必ずSerilogでログ記録後に上位へ伝播

### 環境固有の注意事項

| 項目 | 内容 |
|------|------|
| DB接続 | `Server=localhost` （インスタンス名なし・デフォルトインスタンス） |
| 認証方式 | Windows統合認証（`Trusted_Connection=True`） |
| ファイルパス | Windowsパス形式（`C:\ImageBatch\...`）・日本語パス注意 |
| Docker | 不使用 |
| WSL2 | 開発には不使用（PostgreSQL用にインストール済みだが本プロジェクトでは使わない） |
