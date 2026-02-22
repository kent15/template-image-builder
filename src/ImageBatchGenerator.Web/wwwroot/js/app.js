// ImageBatch Generator - Alpine.js Application
function app() {
  return {
    page: 'dashboard',
    wizardStep: 1,
    selectedTemplate: null,
    selectedJobId: null,
    assetTab: 'all',
    jobStatusFilter: 'all',
    loading: false,
    toast: { show: false, message: '', type: 'success' },
    pollingTimer: null,

    // API data
    templates: [],
    assets: [],
    jobs: [],

    // Wizard state
    wizardJobName: '',
    wizardFormat: 'png',
    wizardQuality: 90,
    wizardNamingRule: '{row_index}.png',
    wizardInputMode: 'direct',
    wizardDirectRows: [{}],
    wizardCsvFileName: '',
    wizardCsvHeaders: [],
    wizardCsvRows: [],
    wizardMapping: [],

    // Template modal
    showTemplateModal: false,
    newTemplate: {
      name: '', description: '', width: 800, height: 600,
      layerConfigJson: '{"layers":[{"id":"title","type":"Text","x":50,"y":100,"width":700,"height":100,"fontSize":48,"fontColor":"#333333","horizontalAlign":"center"},{"id":"subtitle","type":"Text","x":50,"y":250,"width":700,"height":60,"fontSize":24,"fontColor":"#666666","horizontalAlign":"center"}]}'
    },

    // Asset upload
    showAssetModal: false,
    assetUploadCategory: 'ProductImage',

    sidebarItems: [
      { id: 'dashboard', label: 'ダッシュボード', icon: '📊', section: null },
      { id: 'templates', label: 'テンプレート管理', icon: '📐', section: '素材管理' },
      { id: 'assets', label: '素材ライブラリ', icon: '🖼', section: null },
      { id: 'wizard', label: '新規ジョブ作成', icon: '➕', section: 'ジョブ管理' },
      { id: 'jobs', label: 'ジョブ一覧', icon: '📋', section: null },
      { id: 'logs', label: 'ログビューア', icon: '📄', section: 'システム' },
      { id: 'settings', label: '設定', icon: '⚙', section: null },
    ],

    // ── Lifecycle ──
    async init() {
      await Promise.all([this.loadTemplates(), this.loadAssets(), this.loadJobs()]);
      this.startPolling();
    },

    startPolling() {
      this.pollingTimer = setInterval(async () => {
        if (this.jobs.some(j => j.status === 'Running' || j.status === 'Queued')) {
          await this.loadJobs();
        }
      }, 3000);
    },

    // ── Navigation ──
    navigate(p) {
      this.page = p;
      if (p === 'wizard') {
        this.wizardStep = 1;
        this.selectedTemplate = null;
        this.wizardJobName = '';
        this.wizardDirectRows = [{}];
        this.wizardCsvFileName = '';
        this.wizardCsvHeaders = [];
        this.wizardCsvRows = [];
        this.wizardMapping = [];
      }
    },
    goToJobDetail(jobId) {
      this.selectedJobId = jobId;
      this.page = 'job-detail';
    },

    // ── API: Templates ──
    async loadTemplates() {
      try {
        const r = await fetch('/api/templates');
        if (r.ok) this.templates = await r.json();
      } catch (e) { console.error('loadTemplates', e); }
    },
    async registerTemplate() {
      this.loading = true;
      try {
        const r = await fetch('/api/templates', {
          method: 'POST', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(this.newTemplate)
        });
        if (r.ok) {
          await this.loadTemplates();
          this.showTemplateModal = false;
          this.newTemplate = { name: '', description: '', width: 800, height: 600, layerConfigJson: this.newTemplate.layerConfigJson };
          this.showToast('テンプレートを登録しました');
        } else {
          const e = await r.json().catch(() => ({}));
          this.showToast(e.error || '登録に失敗しました', 'error');
        }
      } finally { this.loading = false; }
    },
    async deleteTemplate(id) {
      if (!confirm('このテンプレートを削除しますか？')) return;
      await fetch('/api/templates/' + id, { method: 'DELETE' });
      await this.loadTemplates();
      this.showToast('テンプレートを削除しました');
    },

    // ── API: Assets ──
    async loadAssets() {
      try {
        const q = this.assetTab === 'all' ? '' : '?category=' + this.assetTab;
        const r = await fetch('/api/assets' + q);
        if (r.ok) this.assets = await r.json();
      } catch (e) { console.error('loadAssets', e); }
    },
    async handleAssetUpload(event) {
      const file = event.target.files[0];
      if (!file) return;
      const form = new FormData();
      form.append('file', file);
      form.append('category', this.assetUploadCategory);
      this.loading = true;
      try {
        const r = await fetch('/api/assets', { method: 'POST', body: form });
        if (r.ok) {
          await this.loadAssets();
          this.showAssetModal = false;
          this.showToast('素材をアップロードしました');
        } else {
          this.showToast('アップロードに失敗しました', 'error');
        }
      } finally { this.loading = false; }
    },
    async handleZipUpload(event) {
      const file = event.target.files[0];
      if (!file) return;
      const form = new FormData();
      form.append('zipFile', file);
      form.append('category', this.assetUploadCategory);
      this.loading = true;
      try {
        const r = await fetch('/api/assets/batch', { method: 'POST', body: form });
        if (r.ok) {
          const d = await r.json();
          await this.loadAssets();
          this.showAssetModal = false;
          this.showToast(d.count + '件の素材をアップロードしました');
        }
      } finally { this.loading = false; }
    },
    async deleteAsset(id) {
      if (!confirm('この素材を削除しますか？')) return;
      await fetch('/api/assets/' + id, { method: 'DELETE' });
      await this.loadAssets();
    },

    // ── API: Jobs ──
    async loadJobs() {
      try {
        const r = await fetch('/api/jobs');
        if (r.ok) this.jobs = await r.json();
      } catch (e) { console.error('loadJobs', e); }
    },
    async executeJob() {
      if (!this.selectedTemplate) return;
      const tpl = this.wizardSelectedTemplate;
      let inputRows = [];
      if (this.wizardInputMode === 'csv' && this.wizardCsvRows.length > 0) {
        inputRows = this.wizardCsvRows.map(row => {
          const mapped = {};
          this.wizardMapping.forEach(m => {
            if (m.csvColumn && row[m.csvColumn] !== undefined)
              mapped[m.layerId] = row[m.csvColumn];
          });
          return mapped;
        });
      } else {
        inputRows = this.wizardDirectRows.filter(r => Object.values(r).some(v => v));
      }
      if (inputRows.length === 0) inputRows = [{ _placeholder: 'test' }];

      const payload = {
        name: this.wizardJobName || (tpl ? tpl.name : '新規ジョブ'),
        templateId: this.selectedTemplate,
        mappingRulesJson: JSON.stringify(this.wizardMapping),
        outputSettingsJson: JSON.stringify({ format: this.wizardFormat, quality: this.wizardQuality }),
        totalCount: inputRows.length,
        inputRows: inputRows,
        createdBy: 'WebUI'
      };
      this.loading = true;
      try {
        const cr = await fetch('/api/jobs', {
          method: 'POST', headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        });
        if (!cr.ok) {
          const e = await cr.json().catch(() => ({}));
          this.showToast(e.error || 'ジョブ作成に失敗しました', 'error');
          return;
        }
        const job = await cr.json();
        await fetch('/api/jobs/' + job.id + '/start', { method: 'POST' });
        await this.loadJobs();
        this.showToast('ジョブを開始しました');
        this.goToJobDetail(job.id);
      } finally { this.loading = false; }
    },
    async cancelJob(id) {
      if (!confirm('ジョブをキャンセルしますか？')) return;
      await fetch('/api/jobs/' + id + '/cancel', { method: 'POST' });
      await this.loadJobs();
      this.showToast('ジョブをキャンセルしました');
    },
    async retryJob(id) {
      await fetch('/api/jobs/' + id + '/retry', { method: 'POST' });
      await this.loadJobs();
      this.showToast('エラー分の再実行を開始しました');
    },
    downloadJob(id) {
      window.location.href = '/api/jobs/' + id + '/download';
    },

    // ── Wizard helpers ──
    get wizardSelectedTemplate() {
      return this.templates.find(t => t.id === this.selectedTemplate);
    },
    get wizardLayers() {
      const tpl = this.wizardSelectedTemplate;
      if (!tpl || !tpl.layerConfigJson) return [];
      try {
        return JSON.parse(tpl.layerConfigJson).layers || [];
      } catch { return []; }
    },
    initWizardStep2() {
      const layers = this.wizardLayers;
      if (this.wizardDirectRows.length === 1 && Object.keys(this.wizardDirectRows[0]).length === 0) {
        const row = {};
        layers.forEach(l => row[l.id] = '');
        this.wizardDirectRows = [row];
      }
      if (this.wizardMapping.length === 0) {
        this.wizardMapping = layers.map(l => ({
          layerId: l.id, layerType: l.type, csvColumn: ''
        }));
      }
    },
    addDirectRow() {
      const row = {};
      this.wizardLayers.forEach(l => row[l.id] = '');
      this.wizardDirectRows.push(row);
    },
    removeDirectRow(idx) {
      if (this.wizardDirectRows.length > 1) this.wizardDirectRows.splice(idx, 1);
    },
    handleCsvFile(event) {
      const file = event.target.files[0];
      if (!file) return;
      this.wizardCsvFileName = file.name;
      const reader = new FileReader();
      reader.onload = (e) => {
        let text = e.target.result;
        if (text.charCodeAt(0) === 0xFEFF) text = text.slice(1);
        const lines = text.trim().split('\n');
        if (lines.length < 2) return;
        this.wizardCsvHeaders = lines[0].split(',').map(h => h.trim().replace(/^"|"$/g, ''));
        this.wizardCsvRows = lines.slice(1).map(line => {
          const vals = line.split(',').map(v => v.trim().replace(/^"|"$/g, ''));
          const row = {};
          this.wizardCsvHeaders.forEach((h, i) => row[h] = vals[i] || '');
          return row;
        });
        this.wizardMapping = this.wizardLayers.map(l => ({
          layerId: l.id, layerType: l.type,
          csvColumn: this.wizardCsvHeaders.find(h => h === l.id) || this.wizardCsvHeaders[0] || ''
        }));
        this.wizardInputMode = 'csv';
      };
      reader.readAsText(file, 'UTF-8');
    },
    get wizardInputCount() {
      if (this.wizardInputMode === 'csv') return this.wizardCsvRows.length;
      return this.wizardDirectRows.filter(r => Object.values(r).some(v => v)).length;
    },

    // ── Computed ──
    get selectedJob() {
      return this.jobs.find(j => j.id === this.selectedJobId) || null;
    },
    get filteredJobs() {
      if (this.jobStatusFilter === 'all') return this.jobs;
      return this.jobs.filter(j => j.status === this.jobStatusFilter);
    },
    get filteredAssets() {
      if (this.assetTab === 'all') return this.assets;
      return this.assets.filter(a => a.category === this.assetTab);
    },
    get runningCount() { return this.jobs.filter(j => j.status === 'Running' || j.status === 'Queued').length; },
    get completedCount() { return this.jobs.filter(j => j.status === 'Completed' || j.status === 'CompletedWithWarning').length; },
    get failedCount() { return this.jobs.filter(j => j.status === 'Failed').length; },
    get totalGenerated() { return this.jobs.reduce((s, j) => s + (j.successCount || 0), 0); },

    // ── Helpers ──
    statusClass(s) {
      return { Running: 'bg-blue-100 text-blue-700', Queued: 'bg-slate-100 text-slate-600', Completed: 'bg-green-100 text-green-700', CompletedWithWarning: 'bg-amber-100 text-amber-700', Failed: 'bg-red-100 text-red-700', Cancelled: 'bg-slate-200 text-slate-500' }[s] || 'bg-slate-100 text-slate-600';
    },
    statusLabel(s) {
      return { Running: '実行中', Queued: '待機中', Completed: '完了', CompletedWithWarning: '警告付完了', Failed: '失敗', Cancelled: 'キャンセル', Created: '作成済' }[s] || s;
    },
    progressPercent(job) {
      if (!job) return 0;
      const total = job.totalCount || 0;
      const processed = job.processedCount || 0;
      return total > 0 ? Math.round((processed / total) * 100) : 0;
    },
    shortId(id) {
      if (!id) return '';
      return id.substring(0, 8).toUpperCase();
    },
    formatDate(d) {
      if (!d) return '-';
      try { return new Date(d).toLocaleString('ja-JP', { month: '2-digit', day: '2-digit', hour: '2-digit', minute: '2-digit' }); } catch { return d; }
    },
    formatBytes(b) {
      if (!b) return '0 B';
      const k = 1024, s = ['B', 'KB', 'MB', 'GB'];
      const i = Math.floor(Math.log(b) / Math.log(k));
      return parseFloat((b / Math.pow(k, i)).toFixed(1)) + ' ' + s[i];
    },
    layerIcon(type) {
      return { Text: '📝', Background: '🎨', ProductImage: '🖼' }[type] || '📦';
    },
    tplColor(tpl) {
      const colors = ['#6366f1', '#ec4899', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'];
      if (!tpl) return colors[0];
      let hash = 0;
      for (let i = 0; i < tpl.name.length; i++) hash = tpl.name.charCodeAt(i) + ((hash << 5) - hash);
      return colors[Math.abs(hash) % colors.length];
    },
    showToast(msg, type) {
      this.toast = { show: true, message: msg, type: type || 'success' };
      setTimeout(() => this.toast.show = false, 3000);
    },
  };
}
