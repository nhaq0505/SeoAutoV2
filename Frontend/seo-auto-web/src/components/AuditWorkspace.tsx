'use client';

import { useState, useEffect, useRef } from 'react';
import { 
  api, 
  AuditDetail, 
  AuditSummary 
} from '@/lib/api';
import {
  Globe,
  Search,
  RefreshCw,
  CheckCircle2,
  AlertCircle,
  Clock,
  ExternalLink,
  ShieldCheck,
  Zap,
  Eye,
  FileCode,
  Smartphone,
  Monitor,
  Sparkles,
  Layers,
  ArrowUpRight
} from 'lucide-react';

interface AuditWorkspaceProps {
  showAlert: (type: 'success' | 'error', text: string) => void;
}

export default function AuditWorkspace({ showAlert }: AuditWorkspaceProps) {
  // Input form state
  const [url, setUrl] = useState('');
  const [strategy, setStrategy] = useState<'Desktop' | 'Mobile'>('Desktop');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Active audit and polling state
  const [activeAudit, setActiveAudit] = useState<AuditDetail | null>(null);
  const [isPolling, setIsPolling] = useState(false);
  const [pollingStatusText, setPollingStatusText] = useState('Đang gửi yêu cầu...');
  const pollIntervalRef = useRef<NodeJS.Timeout | null>(null);

  // Audit history state
  const [history, setHistory] = useState<AuditSummary[]>([]);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [historyTotal, setHistoryTotal] = useState(0);

  // Result section ref for auto-scroll
  const resultRef = useRef<HTMLDivElement>(null);

  // Load history on mount
  useEffect(() => {
    loadHistory();
    return () => {
      if (pollIntervalRef.current) clearInterval(pollIntervalRef.current);
    };
  }, []);

  const loadHistory = async () => {
    setLoadingHistory(true);
    try {
      const res = await api.getMyAudits(1, 10);
      setHistory(res.items);
      setHistoryTotal(res.totalCount);
    } catch (err: any) {
      console.error('Failed to load audit history:', err);
    } finally {
      setLoadingHistory(false);
    }
  };

  const handleStartAudit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!url) return;

    setIsSubmitting(true);
    setActiveAudit(null);
    setPollingStatusText('Đang xếp hàng vào Message Broker...');

    try {
      const res = await api.submitAudit({ url, strategy });
      showAlert('success', 'Yêu cầu Audit đã được tiếp nhận và đang xử lý ngầm!');
      setIsPolling(true);
      startPolling(res.auditId);
    } catch (err: any) {
      showAlert('error', err.message || 'Gửi yêu cầu Audit thất bại');
      setIsSubmitting(false);
    }
  };

  const startPolling = (auditId: string) => {
    let attempts = 0;
    const maxAttempts = 60; // 60 * 2.5s = ~150 seconds timeout

    if (pollIntervalRef.current) clearInterval(pollIntervalRef.current);

    pollIntervalRef.current = setInterval(async () => {
      attempts++;
      try {
        const audit = await api.getAuditById(auditId);
        setActiveAudit(audit);

        if (audit.status === 'Processing') {
          setPollingStatusText('Đang phân tích Google PageSpeed & cào dữ liệu On-page HTML...');
        } else if (audit.status === 'Completed') {
          if (pollIntervalRef.current) clearInterval(pollIntervalRef.current);
          setIsPolling(false);
          setIsSubmitting(false);
          showAlert('success', 'Phân tích hoàn tất! Dữ liệu đã sẵn sàng.');
          loadHistory();
          // Scroll to result view
          resultRef.current?.scrollIntoView({ behavior: 'smooth' });
        } else if (audit.status === 'Failed') {
          if (pollIntervalRef.current) clearInterval(pollIntervalRef.current);
          setIsPolling(false);
          setIsSubmitting(false);
          showAlert('error', audit.errorMessage || 'Quá trình phân tích thất bại');
        }

        if (attempts >= maxAttempts) {
          if (pollIntervalRef.current) clearInterval(pollIntervalRef.current);
          setIsPolling(false);
          setIsSubmitting(false);
          showAlert('error', 'Quá thời gian chờ phản hồi từ hệ thống.');
        }
      } catch (err) {
        console.error('Polling error:', err);
      }
    }, 2500);
  };

  const handleViewAudit = async (id: string) => {
    try {
      const audit = await api.getAuditById(id);
      setActiveAudit(audit);
      resultRef.current?.scrollIntoView({ behavior: 'smooth' });
    } catch (err: any) {
      showAlert('error', err.message || 'Không thể tải chi tiết Audit');
    }
  };

  const getScoreColor = (score: number) => {
    if (score >= 90) return 'text-emerald-400 border-emerald-500/30 bg-emerald-500/10';
    if (score >= 50) return 'text-amber-400 border-amber-500/30 bg-amber-500/10';
    return 'text-rose-400 border-rose-500/30 bg-rose-500/10';
  };

  const getScoreBadge = (score: number) => {
    if (score >= 90) return { label: 'Tốt', color: 'bg-emerald-500/20 text-emerald-400' };
    if (score >= 50) return { label: 'Cần cải thiện', color: 'bg-amber-500/20 text-amber-400' };
    return { label: 'Kém', color: 'bg-rose-500/20 text-rose-400' };
  };

  return (
    <div className="space-y-8">
      {/* 1. SEARCH / AUDIT URL INPUT FORM */}
      <div className="hallmark-card p-6 rounded-2xl relative overflow-hidden">
        <div className="absolute -right-10 -bottom-10 w-40 h-40 bg-indigo-600/10 rounded-full blur-2xl pointer-events-none"></div>

        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 mb-4">
          <div>
            <h3 className="text-base font-bold text-white flex items-center gap-2">
              <Zap className="w-4 h-4 text-indigo-400" />
              Kiểm tra & Tối ưu SEO Website tức thì
            </h3>
            <p className="text-xs text-slate-400 mt-0.5">
              Phân tích Core Web Vitals chuẩn Google và kiểm tra các tiêu chí SEO On-page
            </p>
          </div>

          {/* Strategy Toggle */}
          <div className="flex items-center gap-1 bg-slate-900/80 p-1 rounded-xl border border-slate-800 self-start">
            <button
              type="button"
              onClick={() => setStrategy('Desktop')}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium flex items-center gap-1.5 transition ${
                strategy === 'Desktop'
                  ? 'bg-indigo-600 text-white shadow-sm'
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              <Monitor className="w-3.5 h-3.5" /> Desktop
            </button>
            <button
              type="button"
              onClick={() => setStrategy('Mobile')}
              className={`px-3 py-1.5 rounded-lg text-xs font-medium flex items-center gap-1.5 transition ${
                strategy === 'Mobile'
                  ? 'bg-indigo-600 text-white shadow-sm'
                  : 'text-slate-400 hover:text-slate-200'
              }`}
            >
              <Smartphone className="w-3.5 h-3.5" /> Mobile
            </button>
          </div>
        </div>

        <form onSubmit={handleStartAudit} className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <Globe className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
            <input
              type="url"
              required
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              placeholder="https://example.com"
              className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs font-mono"
              disabled={isSubmitting || isPolling}
            />
          </div>
          <button
            type="submit"
            disabled={isSubmitting || isPolling || !url}
            className="px-6 py-2.5 rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 text-white font-semibold text-xs transition shadow-lg shadow-indigo-600/30 flex items-center justify-center gap-2 disabled:opacity-50 hover:opacity-95 cursor-pointer disabled:cursor-not-allowed"
          >
            {isSubmitting || isPolling ? (
              <>
                <RefreshCw className="w-4 h-4 animate-spin" /> Đang phân tích...
              </>
            ) : (
              <>
                <Search className="w-4 h-4" /> Bắt đầu Audit
              </>
            )}
          </button>
        </form>

        {/* Realtime Polling Status Banner */}
        {isPolling && (
          <div className="mt-4 p-3.5 rounded-xl bg-indigo-500/10 border border-indigo-500/20 flex items-center gap-3 animate-pulse">
            <RefreshCw className="w-4 h-4 text-indigo-400 animate-spin flex-shrink-0" />
            <span className="text-xs font-mono text-indigo-300">
              {pollingStatusText}
            </span>
          </div>
        )}
      </div>

      {/* 2. DETAILED AUDIT RESULT DISPLAY */}
      {activeAudit && activeAudit.status === 'Completed' && (
        <div ref={resultRef} className="space-y-6 animate-fadeIn">
          {/* Result Header */}
          <div className="hallmark-card p-6 rounded-2xl">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-800 pb-4 mb-6">
              <div>
                <div className="flex items-center gap-2 mb-1">
                  <span className="text-[11px] font-mono uppercase tracking-wider px-2 py-0.5 rounded-md bg-indigo-500/20 text-indigo-300">
                    {activeAudit.strategy}
                  </span>
                  <span className="text-xs text-slate-500 font-mono">
                    {new Date(activeAudit.createdAt).toLocaleString('vi-VN')}
                  </span>
                </div>
                <h2 className="text-lg font-bold text-white flex items-center gap-2 break-all">
                  <a 
                    href={activeAudit.url} 
                    target="_blank" 
                    rel="noreferrer" 
                    className="hover:text-indigo-400 transition flex items-center gap-1.5"
                  >
                    {activeAudit.url}
                    <ExternalLink className="w-3.5 h-3.5 text-slate-400" />
                  </a>
                </h2>
              </div>

              <div className="flex items-center gap-2">
                <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-500/20 text-emerald-400 border border-emerald-500/30">
                  <CheckCircle2 className="w-3.5 h-3.5" /> Hoàn tất
                </span>
              </div>
            </div>

            {/* 4 Score Gauges */}
            {activeAudit.rawMetrics && (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
                {[
                  { title: 'Performance', score: activeAudit.rawMetrics.performanceScore },
                  { title: 'SEO Score', score: activeAudit.rawMetrics.seoScore },
                  { title: 'Accessibility', score: activeAudit.rawMetrics.accessibilityScore },
                  { title: 'Best Practices', score: activeAudit.rawMetrics.bestPracticesScore },
                ].map((item, idx) => (
                  <div 
                    key={idx} 
                    className={`p-4 rounded-xl border flex flex-col items-center justify-center text-center ${getScoreColor(item.score)}`}
                  >
                    <span className="text-3xl font-extrabold font-mono tracking-tight">{item.score}</span>
                    <span className="text-xs font-semibold mt-1 text-slate-200">{item.title}</span>
                    <span className={`text-[10px] font-mono px-2 py-0.5 rounded-full mt-2 ${getScoreBadge(item.score).color}`}>
                      {getScoreBadge(item.score).label}
                    </span>
                  </div>
                ))}
              </div>
            )}

            {/* Core Web Vitals Grid */}
            {activeAudit.rawMetrics && (
              <div className="bg-slate-900/50 rounded-xl p-4 border border-slate-800">
                <h4 className="text-xs font-mono uppercase tracking-wider text-slate-400 mb-3 flex items-center gap-2">
                  <Zap className="w-3.5 h-3.5 text-amber-400" /> Các chỉ số Core Web Vitals
                </h4>
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-3">
                  <div className="p-3 rounded-lg bg-slate-800/40 border border-slate-700/50">
                    <span className="text-[10px] font-mono text-slate-400 block">LCP (Largest Paint)</span>
                    <span className="text-sm font-bold font-mono text-white mt-1 block">
                      {(activeAudit.rawMetrics.lcpMs / 1000).toFixed(2)}s
                    </span>
                  </div>

                  <div className="p-3 rounded-lg bg-slate-800/40 border border-slate-700/50">
                    <span className="text-[10px] font-mono text-slate-400 block">FCP (First Paint)</span>
                    <span className="text-sm font-bold font-mono text-white mt-1 block">
                      {(activeAudit.rawMetrics.fcpMs / 1000).toFixed(2)}s
                    </span>
                  </div>

                  <div className="p-3 rounded-lg bg-slate-800/40 border border-slate-700/50">
                    <span className="text-[10px] font-mono text-slate-400 block">CLS (Layout Shift)</span>
                    <span className="text-sm font-bold font-mono text-white mt-1 block">
                      {activeAudit.rawMetrics.cls.toFixed(3)}
                    </span>
                  </div>

                  <div className="p-3 rounded-lg bg-slate-800/40 border border-slate-700/50">
                    <span className="text-[10px] font-mono text-slate-400 block">TTFB (Server Time)</span>
                    <span className="text-sm font-bold font-mono text-white mt-1 block">
                      {activeAudit.rawMetrics.ttfbMs}ms
                    </span>
                  </div>

                  <div className="p-3 rounded-lg bg-slate-800/40 border border-slate-700/50">
                    <span className="text-[10px] font-mono text-slate-400 block">TBT / INP</span>
                    <span className="text-sm font-bold font-mono text-white mt-1 block">
                      {activeAudit.rawMetrics.inpMs}ms
                    </span>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* On-Page SEO Inspection Card */}
          {activeAudit.seoAnalysis && (
            <div className="hallmark-card p-6 rounded-2xl">
              <h3 className="text-sm font-bold text-white mb-4 flex items-center gap-2">
                <FileCode className="w-4 h-4 text-indigo-400" />
                Kiểm tra SEO On-page & Cấu trúc Thẻ HTML
              </h3>

              <div className="space-y-4">
                {/* Title */}
                <div className="p-3.5 rounded-xl bg-slate-900/60 border border-slate-800">
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-semibold text-slate-300">Meta Title</span>
                    <span className={`text-[10px] font-mono px-2 py-0.5 rounded ${
                      activeAudit.seoAnalysis.title.length >= 40 && activeAudit.seoAnalysis.title.length <= 65
                        ? 'bg-emerald-500/20 text-emerald-400'
                        : 'bg-amber-500/20 text-amber-400'
                    }`}>
                      {activeAudit.seoAnalysis.title.length} ký tự (Khuyến nghị 50-60)
                    </span>
                  </div>
                  <p className="text-xs text-slate-200 font-mono break-all">
                    {activeAudit.seoAnalysis.title || <span className="text-rose-400 italic">Chưa có thẻ Title</span>}
                  </p>
                </div>

                {/* Description */}
                <div className="p-3.5 rounded-xl bg-slate-900/60 border border-slate-800">
                  <div className="flex items-center justify-between mb-1">
                    <span className="text-xs font-semibold text-slate-300">Meta Description</span>
                    <span className={`text-[10px] font-mono px-2 py-0.5 rounded ${
                      activeAudit.seoAnalysis.metaDescription.length >= 120 && activeAudit.seoAnalysis.metaDescription.length <= 165
                        ? 'bg-emerald-500/20 text-emerald-400'
                        : 'bg-amber-500/20 text-amber-400'
                    }`}>
                      {activeAudit.seoAnalysis.metaDescription.length} ký tự (Khuyến nghị 150-160)
                    </span>
                  </div>
                  <p className="text-xs text-slate-200 font-mono break-all">
                    {activeAudit.seoAnalysis.metaDescription || <span className="text-rose-400 italic">Chưa có thẻ Description</span>}
                  </p>
                </div>

                {/* Canonical URL */}
                <div className="p-3.5 rounded-xl bg-slate-900/60 border border-slate-800">
                  <span className="text-xs font-semibold text-slate-300 block mb-1">Thẻ Canonical URL</span>
                  <p className="text-xs text-slate-200 font-mono break-all">
                    {activeAudit.seoAnalysis.canonicalUrl || <span className="text-slate-500 italic">Không tìm thấy thẻ Canonical</span>}
                  </p>
                </div>

                {/* SEO Checks Grid */}
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 pt-2">
                  {/* H1 Count */}
                  <div className="p-3 rounded-xl bg-slate-900/40 border border-slate-800 text-center">
                    <span className="text-[10px] font-mono text-slate-400 block mb-1">Số lượng thẻ H1</span>
                    <span className={`text-lg font-bold font-mono ${
                      activeAudit.seoAnalysis.h1Count === 1 ? 'text-emerald-400' : 'text-amber-400'
                    }`}>
                      {activeAudit.seoAnalysis.h1Count}
                    </span>
                    <span className="text-[10px] text-slate-500 block mt-0.5">
                      {activeAudit.seoAnalysis.h1Count === 1 ? '✓ Chuẩn duy nhất 1 H1' : '⚠ Nên có đúng 1 H1'}
                    </span>
                  </div>

                  {/* Images without Alt */}
                  <div className="p-3 rounded-xl bg-slate-900/40 border border-slate-800 text-center">
                    <span className="text-[10px] font-mono text-slate-400 block mb-1">Ảnh thiếu thẻ Alt</span>
                    <span className={`text-lg font-bold font-mono ${
                      activeAudit.seoAnalysis.imagesWithoutAlt === 0 ? 'text-emerald-400' : 'text-rose-400'
                    }`}>
                      {activeAudit.seoAnalysis.imagesWithoutAlt}
                    </span>
                    <span className="text-[10px] text-slate-500 block mt-0.5">
                      {activeAudit.seoAnalysis.imagesWithoutAlt === 0 ? '✓ Đầy đủ Alt Text' : 'Cần bổ sung alt'}
                    </span>
                  </div>

                  {/* Robots.txt */}
                  <div className="p-3 rounded-xl bg-slate-900/40 border border-slate-800 text-center">
                    <span className="text-[10px] font-mono text-slate-400 block mb-1">File robots.txt</span>
                    <span className={`text-sm font-bold font-mono flex items-center justify-center gap-1 mt-1.5 ${
                      activeAudit.seoAnalysis.hasRobotsTxt ? 'text-emerald-400' : 'text-rose-400'
                    }`}>
                      {activeAudit.seoAnalysis.hasRobotsTxt ? '✓ Đã có' : '✗ Chưa tìm thấy'}
                    </span>
                  </div>

                  {/* Sitemap */}
                  <div className="p-3 rounded-xl bg-slate-900/40 border border-slate-800 text-center">
                    <span className="text-[10px] font-mono text-slate-400 block mb-1">Sitemap XML</span>
                    <span className={`text-sm font-bold font-mono flex items-center justify-center gap-1 mt-1.5 ${
                      activeAudit.seoAnalysis.hasSitemap ? 'text-emerald-400' : 'text-rose-400'
                    }`}>
                      {activeAudit.seoAnalysis.hasSitemap ? '✓ Đã có' : '✗ Chưa tìm thấy'}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* 3. AUDIT HISTORY TABLE */}
      <div className="hallmark-card p-6 rounded-2xl">
        <div className="flex items-center justify-between mb-4">
          <div>
            <h3 className="text-sm font-bold text-white flex items-center gap-2">
              <Clock className="w-4 h-4 text-indigo-400" />
              Lịch sử các trang web đã Audit ({historyTotal})
            </h3>
            <p className="text-xs text-slate-400 mt-0.5">
              Danh sách các báo cáo phân tích trước đó của bạn
            </p>
          </div>
          <button
            onClick={loadHistory}
            disabled={loadingHistory}
            className="p-2 rounded-lg bg-slate-900 hover:bg-slate-800 text-slate-400 hover:text-white transition text-xs flex items-center gap-1 border border-slate-800 cursor-pointer"
            title="Làm mới lịch sử"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loadingHistory ? 'animate-spin' : ''}`} />
          </button>
        </div>

        {loadingHistory && history.length === 0 ? (
          <div className="py-8 text-center text-xs text-slate-500 font-mono">
            Đang tải dữ liệu lịch sử...
          </div>
        ) : history.length === 0 ? (
          <div className="py-8 text-center text-xs text-slate-500 font-mono">
            Bạn chưa thực hiện lần Audit nào. Hãy nhập URL ở trên để bắt đầu!
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead>
                <tr className="border-b border-slate-800/80 text-slate-400 font-mono uppercase text-[10px]">
                  <th className="pb-3 font-semibold">Trang web (URL)</th>
                  <th className="pb-3 font-semibold">Thiết bị</th>
                  <th className="pb-3 font-semibold">Trạng thái</th>
                  <th className="pb-3 font-semibold text-center">Điểm Perf</th>
                  <th className="pb-3 font-semibold text-center">Điểm SEO</th>
                  <th className="pb-3 font-semibold">Thời gian</th>
                  <th className="pb-3 font-semibold text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-800/40">
                {history.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-900/40 transition">
                    <td className="py-3 font-medium text-slate-200 max-w-[220px] truncate pr-4">
                      {item.url}
                    </td>
                    <td className="py-3 text-slate-400 font-mono">
                      {item.strategy}
                    </td>
                    <td className="py-3">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold ${
                        item.status === 'Completed'
                          ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20'
                          : item.status === 'Processing'
                          ? 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 animate-pulse'
                          : item.status === 'Failed'
                          ? 'bg-rose-500/10 text-rose-400 border border-rose-500/20'
                          : 'bg-slate-800 text-slate-400'
                      }`}>
                        {item.status}
                      </span>
                    </td>
                    <td className="py-3 text-center font-mono font-bold">
                      {item.performanceScore !== null && item.performanceScore !== undefined ? (
                        <span className={item.performanceScore >= 90 ? 'text-emerald-400' : item.performanceScore >= 50 ? 'text-amber-400' : 'text-rose-400'}>
                          {item.performanceScore}
                        </span>
                      ) : '—'}
                    </td>
                    <td className="py-3 text-center font-mono font-bold">
                      {item.seoScore !== null && item.seoScore !== undefined ? (
                        <span className={item.seoScore >= 90 ? 'text-emerald-400' : item.seoScore >= 50 ? 'text-amber-400' : 'text-rose-400'}>
                          {item.seoScore}
                        </span>
                      ) : '—'}
                    </td>
                    <td className="py-3 text-slate-400 font-mono text-[11px]">
                      {new Date(item.createdAt).toLocaleDateString('vi-VN')}
                    </td>
                    <td className="py-3 text-right">
                      <button
                        onClick={() => handleViewAudit(item.id)}
                        className="px-2.5 py-1 rounded-lg bg-indigo-600/20 hover:bg-indigo-600/40 text-indigo-300 text-[11px] font-medium transition cursor-pointer inline-flex items-center gap-1"
                      >
                        <Eye className="w-3 h-3" /> Chi tiết
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
