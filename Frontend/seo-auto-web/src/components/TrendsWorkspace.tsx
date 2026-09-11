'use client';

import React, { useState, useEffect, useMemo } from 'react';
import {
  TrendingUp,
  Calendar,
  Globe,
  RefreshCw,
  Award,
  Zap,
  CheckCircle2,
  AlertTriangle,
  ArrowUpRight,
  ArrowDownRight,
  Filter,
  Play
} from 'lucide-react';
import {
  ResponsiveContainer,
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend
} from 'recharts';
import {
  api,
  WebsiteSummary,
  ScoreHistoryPoint,
  ReportHistoryResponse
} from '@/lib/api';

interface TrendsWorkspaceProps {
  initialWebsiteId?: string | null;
  showAlert: (type: 'success' | 'error', text: string) => void;
  onAuditNow?: (url: string) => void;
}

export default function TrendsWorkspace({
  initialWebsiteId,
  showAlert,
  onAuditNow
}: TrendsWorkspaceProps) {
  const [websites, setWebsites] = useState<WebsiteSummary[]>([]);
  const [selectedWebsiteId, setSelectedWebsiteId] = useState<string>(initialWebsiteId || '');
  const [timeRange, setTimeRange] = useState<'7d' | '30d' | 'all'>('30d');
  const [historyData, setHistoryData] = useState<ScoreHistoryPoint[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [chartLoading, setChartLoading] = useState<boolean>(false);

  // Load user's websites for the selector
  useEffect(() => {
    const loadWebsites = async () => {
      try {
        const data = await api.getWebsites();
        setWebsites(data);
        if (!selectedWebsiteId && data.length > 0) {
          setSelectedWebsiteId(data[0].id);
        }
      } catch (err: any) {
        console.error('Failed to load websites:', err);
      } finally {
        setLoading(false);
      }
    };

    loadWebsites();
  }, []);

  // Update selected website if initialWebsiteId prop changes
  useEffect(() => {
    if (initialWebsiteId) {
      setSelectedWebsiteId(initialWebsiteId);
    }
  }, [initialWebsiteId]);

  // Load history data whenever selectedWebsiteId or timeRange changes
  useEffect(() => {
    loadHistory();
  }, [selectedWebsiteId, timeRange]);

  const loadHistory = async () => {
    try {
      setChartLoading(true);

      let fromDate: string | undefined;
      const now = new Date();
      if (timeRange === '7d') {
        const d = new Date();
        d.setDate(now.getDate() - 7);
        fromDate = d.toISOString();
      } else if (timeRange === '30d') {
        const d = new Date();
        d.setDate(now.getDate() - 30);
        fromDate = d.toISOString();
      }

      const res = await api.getReportHistory({
        websiteId: selectedWebsiteId || undefined,
        from: fromDate
      });

      setHistoryData(res.history || []);
    } catch (err: any) {
      showAlert('error', err.message || 'Không thể tải lịch sử điểm số');
    } finally {
      setChartLoading(false);
    }
  };

  const selectedWebsite = useMemo(() => {
    return websites.find((w) => w.id === selectedWebsiteId);
  }, [websites, selectedWebsiteId]);

  // Formatted data for Recharts
  const formattedChartData = useMemo(() => {
    return historyData.map((pt) => {
      const d = new Date(pt.date);
      const dateLabel = `${d.getDate().toString().padStart(2, '0')}/${(d.getMonth() + 1)
        .toString()
        .padStart(2, '0')} ${d.getHours().toString().padStart(2, '0')}:${d
        .getMinutes()
        .toString()
        .padStart(2, '0')}`;

      return {
        ...pt,
        dateLabel,
        rawDate: d
      };
    });
  }, [historyData]);

  // Calculate KPIs
  const stats = useMemo(() => {
    if (historyData.length === 0) return null;

    const latest = historyData[historyData.length - 1];
    const prev = historyData.length > 1 ? historyData[historyData.length - 2] : null;

    const scoreDiff = prev ? latest.overallScore - prev.overallScore : 0;
    const avgOverall = Math.round(
      historyData.reduce((acc, cur) => acc + cur.overallScore, 0) / historyData.length
    );
    const avgPerf = Math.round(
      historyData.reduce((acc, cur) => acc + cur.performanceScore, 0) / historyData.length
    );
    const avgSeo = Math.round(
      historyData.reduce((acc, cur) => acc + cur.seoScore, 0) / historyData.length
    );

    return {
      latestOverall: latest.overallScore,
      scoreDiff,
      avgOverall,
      avgPerf,
      avgSeo,
      totalAudits: historyData.length
    };
  }, [historyData]);

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      return (
        <div className="bg-[#0b0f19] border border-white/10 rounded-xl p-3 shadow-xl text-xs font-mono">
          <p className="text-slate-400 font-semibold mb-2">{label}</p>
          <div className="space-y-1">
            {payload.map((entry: any, index: number) => (
              <div key={`item-${index}`} className="flex items-center justify-between gap-4">
                <span className="flex items-center gap-1.5" style={{ color: entry.color }}>
                  <span className="w-2 h-2 rounded-full" style={{ backgroundColor: entry.color }}></span>
                  {entry.name}:
                </span>
                <span className="font-bold text-white">{entry.value}/100</span>
              </div>
            ))}
          </div>
        </div>
      );
    }
    return null;
  };

  return (
    <div className="space-y-6">
      {/* Header & Filter Controls Bar */}
      <div className="hallmark-card p-6 rounded-2xl flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h3 className="text-base font-bold text-white tracking-tight flex items-center gap-2">
            <TrendingUp className="w-4 h-4 text-indigo-400" /> Xu Hướng & Lịch Sử Core Web Vitals
          </h3>
          <p className="text-xs text-slate-400 mt-1">
            Theo dõi tiến trình tối ưu hóa điểm số và sự thay đổi hiệu năng qua các đợt audit.
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-3">
          {/* Website Selector */}
          <div className="flex items-center gap-2 bg-slate-900/80 border border-white/10 rounded-xl px-3 py-1.5">
            <Globe className="w-3.5 h-3.5 text-slate-400" />
            <select
              value={selectedWebsiteId}
              onChange={(e) => setSelectedWebsiteId(e.target.value)}
              className="bg-transparent text-xs text-slate-200 focus:outline-none cursor-pointer"
            >
              <option value="" className="bg-slate-900 text-slate-300">
                Tất cả website
              </option>
              {websites.map((w) => (
                <option key={w.id} value={w.id} className="bg-slate-900 text-slate-200">
                  {w.name} ({w.url.replace(/^https?:\/\//, '')})
                </option>
              ))}
            </select>
          </div>

          {/* Time Range Selector */}
          <div className="flex items-center bg-slate-900/80 border border-white/10 rounded-xl p-1">
            <button
              onClick={() => setTimeRange('7d')}
              className={`px-3 py-1 text-[11px] font-mono rounded-lg transition ${
                timeRange === '7d' ? 'bg-indigo-600 text-white font-semibold' : 'text-slate-400 hover:text-white'
              }`}
            >
              7 ngày
            </button>
            <button
              onClick={() => setTimeRange('30d')}
              className={`px-3 py-1 text-[11px] font-mono rounded-lg transition ${
                timeRange === '30d' ? 'bg-indigo-600 text-white font-semibold' : 'text-slate-400 hover:text-white'
              }`}
            >
              30 ngày
            </button>
            <button
              onClick={() => setTimeRange('all')}
              className={`px-3 py-1 text-[11px] font-mono rounded-lg transition ${
                timeRange === 'all' ? 'bg-indigo-600 text-white font-semibold' : 'text-slate-400 hover:text-white'
              }`}
            >
              Tất cả
            </button>
          </div>

          <button
            onClick={loadHistory}
            className="p-2 rounded-xl bg-slate-900 border border-white/10 text-slate-400 hover:text-white hover:bg-slate-800 transition"
            title="Làm mới dữ liệu"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${chartLoading ? 'animate-spin text-indigo-400' : ''}`} />
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      {stats && (
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <div className="hallmark-card p-4 rounded-2xl">
            <div className="flex items-center justify-between text-slate-400 text-xs mb-2">
              <span className="flex items-center gap-1.5 font-mono">
                <Award className="w-3.5 h-3.5 text-indigo-400" /> Điểm Gần Nhất
              </span>
              {stats.scoreDiff !== 0 && (
                <span
                  className={`text-[10px] font-mono font-bold flex items-center ${
                    stats.scoreDiff > 0 ? 'text-emerald-400' : 'text-rose-400'
                  }`}
                >
                  {stats.scoreDiff > 0 ? <ArrowUpRight className="w-3 h-3" /> : <ArrowDownRight className="w-3 h-3" />}
                  {Math.abs(stats.scoreDiff)}đ
                </span>
              )}
            </div>
            <p className="text-2xl font-bold text-white">{stats.latestOverall}/100</p>
          </div>

          <div className="hallmark-card p-4 rounded-2xl">
            <div className="flex items-center justify-between text-slate-400 text-xs mb-2">
              <span className="flex items-center gap-1.5 font-mono">
                <Zap className="w-3.5 h-3.5 text-emerald-400" /> TB Performance
              </span>
            </div>
            <p className="text-2xl font-bold text-white">{stats.avgPerf}/100</p>
          </div>

          <div className="hallmark-card p-4 rounded-2xl">
            <div className="flex items-center justify-between text-slate-400 text-xs mb-2">
              <span className="flex items-center gap-1.5 font-mono">
                <CheckCircle2 className="w-3.5 h-3.5 text-blue-400" /> TB SEO On-page
              </span>
            </div>
            <p className="text-2xl font-bold text-white">{stats.avgSeo}/100</p>
          </div>

          <div className="hallmark-card p-4 rounded-2xl">
            <div className="flex items-center justify-between text-slate-400 text-xs mb-2">
              <span className="flex items-center gap-1.5 font-mono">
                <Calendar className="w-3.5 h-3.5 text-purple-400" /> Tổng Lần Audit
              </span>
            </div>
            <p className="text-2xl font-bold text-white">{stats.totalAudits}</p>
          </div>
        </div>
      )}

      {/* Main Chart Area */}
      <div className="hallmark-card p-6 rounded-2xl">
        <div className="flex items-center justify-between mb-6">
          <div>
            <h4 className="text-sm font-bold text-white flex items-center gap-2">
              Biểu đồ Biến thiên Điểm số theo Thời gian
            </h4>
            <p className="text-xs text-slate-400 mt-0.5">
              {selectedWebsite ? `Đang hiển thị cho: ${selectedWebsite.url}` : 'Tổng hợp các lượt audit của bạn'}
            </p>
          </div>

          {selectedWebsite && onAuditNow && (
            <button
              onClick={() => onAuditNow(selectedWebsite.url)}
              className="px-3.5 py-1.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold flex items-center gap-1.5 transition"
            >
              <Play className="w-3 h-3" /> Audit Ngay
            </button>
          )}
        </div>

        {chartLoading ? (
          <div className="h-72 flex flex-col items-center justify-center">
            <RefreshCw className="w-6 h-6 animate-spin text-indigo-500 mb-2" />
            <p className="text-xs font-mono text-slate-400">Đang đồng bộ biểu đồ...</p>
          </div>
        ) : formattedChartData.length === 0 ? (
          <div className="h-72 flex flex-col items-center justify-center text-center p-6 border border-dashed border-white/10 rounded-xl">
            <AlertTriangle className="w-8 h-8 text-amber-400 mb-2 opacity-80" />
            <h5 className="text-xs font-bold text-white mb-1">Chưa có dữ liệu audit cho mốc thời gian này</h5>
            <p className="text-[11px] text-slate-400 max-w-sm mb-4">
              Thực hiện thêm các lượt audit để hệ thống ghi nhận lịch sử và vẽ biểu đồ xu hướng.
            </p>
            {selectedWebsite && onAuditNow && (
              <button
                onClick={() => onAuditNow(selectedWebsite.url)}
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold flex items-center gap-2"
              >
                <Play className="w-3.5 h-3.5" /> Bắt đầu Audit Ngay
              </button>
            )}
          </div>
        ) : (
          <div className="h-80 w-full">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={formattedChartData} margin={{ top: 10, right: 30, left: -15, bottom: 5 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#ffffff15" />
                <XAxis dataKey="dateLabel" stroke="#94a3b8" fontSize={11} tickLine={false} />
                <YAxis domain={[0, 100]} stroke="#94a3b8" fontSize={11} tickLine={false} />
                <Tooltip content={<CustomTooltip />} />
                <Legend
                  wrapperStyle={{ paddingTop: '15px', fontSize: '11px', fontFamily: 'monospace' }}
                  iconType="circle"
                />
                <Line
                  type="monotone"
                  dataKey="overallScore"
                  name="Điểm Tổng Hợp"
                  stroke="#6366f1"
                  strokeWidth={3}
                  dot={{ r: 4, fill: '#6366f1' }}
                  activeDot={{ r: 6 }}
                />
                <Line
                  type="monotone"
                  dataKey="performanceScore"
                  name="Performance"
                  stroke="#10b981"
                  strokeWidth={2}
                  strokeDasharray="4 2"
                  dot={{ r: 3, fill: '#10b981' }}
                />
                <Line
                  type="monotone"
                  dataKey="seoScore"
                  name="SEO On-page"
                  stroke="#3b82f6"
                  strokeWidth={2}
                  dot={{ r: 3, fill: '#3b82f6' }}
                />
                <Line
                  type="monotone"
                  dataKey="accessibilityScore"
                  name="Accessibility"
                  stroke="#f59e0b"
                  strokeWidth={1.5}
                  strokeDasharray="2 2"
                  dot={false}
                />
                <Line
                  type="monotone"
                  dataKey="bestPracticesScore"
                  name="Best Practices"
                  stroke="#a855f7"
                  strokeWidth={1.5}
                  strokeDasharray="2 2"
                  dot={false}
                />
              </LineChart>
            </ResponsiveContainer>
          </div>
        )}
      </div>

      {/* History Log Table */}
      {formattedChartData.length > 0 && (
        <div className="hallmark-card rounded-2xl p-6">
          <h4 className="text-sm font-bold text-white mb-4">Chi tiết các lượt đánh giá gần đây</h4>
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs font-mono">
              <thead>
                <tr className="border-b border-white/10 text-slate-400">
                  <th className="pb-3 font-semibold">Thời gian</th>
                  <th className="pb-3 font-semibold text-center">Overall</th>
                  <th className="pb-3 font-semibold text-center">Performance</th>
                  <th className="pb-3 font-semibold text-center">SEO</th>
                  <th className="pb-3 font-semibold text-center">Accessibility</th>
                  <th className="pb-3 font-semibold text-center">Best Practices</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {[...formattedChartData].reverse().map((item, idx) => (
                  <tr key={idx} className="hover:bg-white/[0.02] transition">
                    <td className="py-3 text-slate-300">
                      {item.rawDate.toLocaleString('vi-VN')}
                    </td>
                    <td className="py-3 text-center">
                      <span className="font-bold px-2 py-0.5 rounded bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
                        {item.overallScore}
                      </span>
                    </td>
                    <td className="py-3 text-center">
                      <span className="text-emerald-400 font-semibold">{item.performanceScore}</span>
                    </td>
                    <td className="py-3 text-center">
                      <span className="text-blue-400 font-semibold">{item.seoScore}</span>
                    </td>
                    <td className="py-3 text-center text-amber-400">{item.accessibilityScore}</td>
                    <td className="py-3 text-center text-purple-400">{item.bestPracticesScore}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
