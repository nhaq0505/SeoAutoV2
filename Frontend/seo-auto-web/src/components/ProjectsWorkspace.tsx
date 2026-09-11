'use client';

import React, { useState, useEffect } from 'react';
import {
  FolderPlus,
  Globe,
  Plus,
  Trash2,
  ExternalLink,
  ArrowLeft,
  RefreshCw,
  Folder,
  TrendingUp,
  Play,
  Calendar
} from 'lucide-react';
import {
  api,
  ProjectSummary,
  ProjectDetail
} from '@/lib/api';

interface ProjectsWorkspaceProps {
  showAlert: (type: 'success' | 'error', text: string) => void;
  onAuditWebsite: (url: string) => void;
  onViewTrends: (websiteId: string) => void;
}

export default function ProjectsWorkspace({
  showAlert,
  onAuditWebsite,
  onViewTrends
}: ProjectsWorkspaceProps) {
  const [projects, setProjects] = useState<ProjectSummary[]>([]);
  const [selectedProject, setSelectedProject] = useState<ProjectDetail | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [detailLoading, setDetailLoading] = useState<boolean>(false);

  // Modals state
  const [showCreateProject, setShowCreateProject] = useState<boolean>(false);
  const [showAddWebsite, setShowAddWebsite] = useState<boolean>(false);
  const [submitting, setSubmitting] = useState<boolean>(false);

  // Form inputs
  const [projectName, setProjectName] = useState<string>('');
  const [projectDesc, setProjectDesc] = useState<string>('');
  const [websiteUrl, setWebsiteUrl] = useState<string>('');
  const [websiteName, setWebsiteName] = useState<string>('');

  const loadProjects = async () => {
    try {
      setLoading(true);
      const data = await api.getProjects();
      setProjects(data);
    } catch (err: any) {
      showAlert('error', err.message || 'Không thể tải danh sách dự án');
    } finally {
      setLoading(false);
    }
  };

  const loadProjectDetail = async (id: string) => {
    try {
      setDetailLoading(true);
      const data = await api.getProjectById(id);
      setSelectedProject(data);
    } catch (err: any) {
      showAlert('error', err.message || 'Không thể xem chi tiết dự án');
    } finally {
      setDetailLoading(false);
    }
  };

  useEffect(() => {
    loadProjects();
  }, []);

  const handleCreateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!projectName.trim()) return;

    try {
      setSubmitting(true);
      await api.createProject({
        name: projectName.trim(),
        description: projectDesc.trim() || undefined
      });
      showAlert('success', 'Đã tạo dự án mới thành công!');
      setShowCreateProject(false);
      setProjectName('');
      setProjectDesc('');
      await loadProjects();
    } catch (err: any) {
      showAlert('error', err.message || 'Tạo dự án thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteProject = async (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    if (!confirm('Bạn có chắc chắn muốn xóa dự án này? Toàn bộ website theo dõi sẽ bị xóa.')) return;

    try {
      await api.deleteProject(id);
      showAlert('success', 'Đã xóa dự án thành công');
      if (selectedProject?.id === id) {
        setSelectedProject(null);
      }
      await loadProjects();
    } catch (err: any) {
      showAlert('error', err.message || 'Xóa dự án thất bại');
    }
  };

  const handleAddWebsite = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProject || !websiteUrl.trim() || !websiteName.trim()) return;

    try {
      setSubmitting(true);
      await api.createWebsite({
        projectId: selectedProject.id,
        url: websiteUrl.trim(),
        name: websiteName.trim()
      });
      showAlert('success', 'Đã thêm website vào dự án thành công!');
      setShowAddWebsite(false);
      setWebsiteUrl('');
      setWebsiteName('');
      await loadProjectDetail(selectedProject.id);
      await loadProjects();
    } catch (err: any) {
      showAlert('error', err.message || 'Thêm website thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteWebsite = async (id: string) => {
    if (!confirm('Bạn có chắc chắn muốn xóa website này khỏi dự án?')) return;

    try {
      await api.deleteWebsite(id);
      showAlert('success', 'Đã xóa website thành công');
      if (selectedProject) {
        await loadProjectDetail(selectedProject.id);
      }
      await loadProjects();
    } catch (err: any) {
      showAlert('error', err.message || 'Xóa website thất bại');
    }
  };

  const getScoreColor = (score?: number | null) => {
    if (score == null) return 'text-slate-400 bg-slate-800/60 border-slate-700/50';
    if (score >= 90) return 'text-emerald-400 bg-emerald-950/40 border-emerald-500/30';
    if (score >= 50) return 'text-amber-400 bg-amber-950/40 border-amber-500/30';
    return 'text-rose-400 bg-rose-950/40 border-rose-500/30';
  };

  if (loading) {
    return (
      <div className="hallmark-card rounded-2xl p-12 text-center flex flex-col items-center justify-center">
        <RefreshCw className="w-8 h-8 animate-spin text-indigo-500 mb-3" />
        <p className="text-xs font-mono text-slate-400">Đang tải danh sách dự án...</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* View 1: Detailed Project View (If a project is selected) */}
      {selectedProject ? (
        <div className="space-y-6">
          <div className="hallmark-card p-6 rounded-2xl">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div className="flex items-start gap-4">
                <button
                  onClick={() => setSelectedProject(null)}
                  className="p-2.5 rounded-xl bg-slate-800/80 hover:bg-slate-700/80 text-slate-300 transition shrink-0"
                  title="Quay lại danh sách dự án"
                >
                  <ArrowLeft className="w-4 h-4" />
                </button>
                <div>
                  <div className="flex items-center gap-3">
                    <h3 className="text-lg font-bold text-white tracking-tight">{selectedProject.name}</h3>
                    <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
                      {selectedProject.websites.length} Websites
                    </span>
                  </div>
                  <p className="text-xs text-slate-400 mt-1">
                    {selectedProject.description || 'Không có mô tả chi tiết cho dự án này.'}
                  </p>
                </div>
              </div>

              <button
                onClick={() => setShowAddWebsite(true)}
                className="px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-xs transition shadow-lg shadow-indigo-600/30 flex items-center gap-2 self-start md:self-auto"
              >
                <Plus className="w-4 h-4" /> Thêm Website vào Dự án
              </button>
            </div>
          </div>

          {/* Websites in this Project */}
          {detailLoading ? (
            <div className="hallmark-card rounded-2xl p-12 text-center flex flex-col items-center">
              <RefreshCw className="w-6 h-6 animate-spin text-indigo-500 mb-2" />
              <p className="text-xs font-mono text-slate-400">Đang tải danh sách website...</p>
            </div>
          ) : selectedProject.websites.length === 0 ? (
            <div className="hallmark-card rounded-2xl p-12 text-center flex flex-col items-center justify-center">
              <div className="w-12 h-12 rounded-2xl bg-indigo-950/60 border border-indigo-500/30 flex items-center justify-center text-indigo-400 mb-4">
                <Globe className="w-6 h-6" />
              </div>
              <h4 className="text-sm font-bold text-white mb-1">Chưa có website nào trong dự án</h4>
              <p className="text-xs text-slate-400 max-w-sm mb-4">
                Thêm website đầu tiên để theo dõi điểm số hiệu năng và nhận cảnh báo định kỳ.
              </p>
              <button
                onClick={() => setShowAddWebsite(true)}
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold flex items-center gap-2"
              >
                <Plus className="w-3.5 h-3.5" /> Thêm Website Ngay
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              {selectedProject.websites.map((website) => (
                <div
                  key={website.id}
                  className="hallmark-card rounded-2xl p-5 hover:border-indigo-500/40 transition flex flex-col justify-between group relative"
                >
                  <div>
                    <div className="flex items-start justify-between gap-3 mb-3">
                      <div className="flex items-center gap-2.5 min-w-0">
                        <div className="w-8 h-8 rounded-lg bg-indigo-950/70 border border-indigo-500/30 flex items-center justify-center text-indigo-400 shrink-0">
                          <Globe className="w-4 h-4" />
                        </div>
                        <div className="min-w-0">
                          <h4 className="text-xs font-bold text-white truncate">{website.name}</h4>
                          <a
                            href={website.url}
                            target="_blank"
                            rel="noreferrer"
                            className="text-[11px] font-mono text-slate-400 hover:text-indigo-400 transition flex items-center gap-1 truncate"
                          >
                            <span className="truncate">{website.url}</span>
                            <ExternalLink className="w-3 h-3 shrink-0" />
                          </a>
                        </div>
                      </div>

                      <button
                        onClick={() => handleDeleteWebsite(website.id)}
                        className="p-1.5 rounded-lg text-slate-500 hover:text-rose-400 hover:bg-rose-500/10 transition opacity-0 group-hover:opacity-100"
                        title="Xóa website"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>

                    <div className="bg-slate-950/60 rounded-xl p-3 border border-white/5 flex items-center justify-between mb-4">
                      <span className="text-[11px] font-mono text-slate-400">Điểm Audit gần nhất:</span>
                      <span
                        className={`text-xs font-bold px-2.5 py-0.5 rounded-lg border ${getScoreColor(
                          website.latestOverallScore
                        )}`}
                      >
                        {website.latestOverallScore != null ? `${website.latestOverallScore}/100` : 'Chưa có'}
                      </span>
                    </div>

                    {website.latestAuditDate && (
                      <p className="text-[10px] font-mono text-slate-500 mb-4 flex items-center gap-1.5">
                        <Calendar className="w-3 h-3" />
                        {new Date(website.latestAuditDate).toLocaleString('vi-VN')}
                      </p>
                    )}
                  </div>

                  <div className="flex items-center gap-2 pt-2 border-t border-white/5">
                    <button
                      onClick={() => onAuditWebsite(website.url)}
                      className="flex-1 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-[11px] font-semibold transition flex items-center justify-center gap-1.5 shadow-sm"
                    >
                      <Play className="w-3 h-3" /> Audit Ngay
                    </button>
                    <button
                      onClick={() => onViewTrends(website.id)}
                      className="px-3 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-200 text-[11px] font-semibold transition flex items-center justify-center gap-1.5"
                      title="Xem biểu đồ lịch sử"
                    >
                      <TrendingUp className="w-3.5 h-3.5 text-indigo-400" />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      ) : (
        /* View 2: Projects Overview Grid */
        <div className="space-y-6">
          <div className="hallmark-card p-6 rounded-2xl flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h3 className="text-base font-bold text-white tracking-tight flex items-center gap-2">
                <Folder className="w-4 h-4 text-indigo-400" /> Dự án Theo Dõi (Projects)
              </h3>
              <p className="text-xs text-slate-400 mt-1">
                Tổ chức các website theo từng nhóm dự án hoặc khách hàng để theo dõi định kỳ.
              </p>
            </div>

            <button
              onClick={() => setShowCreateProject(true)}
              className="px-4 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-xs transition shadow-lg shadow-indigo-600/30 flex items-center gap-2 self-start sm:self-auto"
            >
              <FolderPlus className="w-4 h-4" /> Tạo Dự án Mới
            </button>
          </div>

          {projects.length === 0 ? (
            <div className="hallmark-card rounded-2xl p-12 text-center flex flex-col items-center justify-center">
              <div className="w-12 h-12 rounded-2xl bg-indigo-950/60 border border-indigo-500/30 flex items-center justify-center text-indigo-400 mb-4">
                <Folder className="w-6 h-6" />
              </div>
              <h4 className="text-sm font-bold text-white mb-1">Chưa có dự án nào được tạo</h4>
              <p className="text-xs text-slate-400 max-w-sm mb-4">
                Tạo dự án để quản lý danh mục các website cần theo dõi liên tục Core Web Vitals.
              </p>
              <button
                onClick={() => setShowCreateProject(true)}
                className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold flex items-center gap-2"
              >
                <FolderPlus className="w-3.5 h-3.5" /> Tạo Dự án Đầu Tiên
              </button>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
              {projects.map((project) => (
                <div
                  key={project.id}
                  onClick={() => loadProjectDetail(project.id)}
                  className="hallmark-card rounded-2xl p-5 hover:border-indigo-500/50 hover:shadow-xl hover:shadow-indigo-500/5 transition cursor-pointer flex flex-col justify-between group"
                >
                  <div>
                    <div className="flex items-start justify-between gap-3 mb-3">
                      <div className="flex items-center gap-2.5">
                        <div className="w-10 h-10 rounded-xl bg-indigo-950/60 border border-indigo-500/30 flex items-center justify-center text-indigo-400 group-hover:scale-105 transition">
                          <Folder className="w-5 h-5" />
                        </div>
                        <div>
                          <h4 className="text-sm font-bold text-white group-hover:text-indigo-300 transition">
                            {project.name}
                          </h4>
                          <span className="text-[10px] font-mono text-slate-500">
                            {new Date(project.createdAt).toLocaleDateString('vi-VN')}
                          </span>
                        </div>
                      </div>

                      <button
                        onClick={(e) => handleDeleteProject(project.id, e)}
                        className="p-1.5 rounded-lg text-slate-500 hover:text-rose-400 hover:bg-rose-500/10 transition opacity-0 group-hover:opacity-100"
                        title="Xóa dự án"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    </div>

                    <p className="text-xs text-slate-400 line-clamp-2 mb-4">
                      {project.description || 'Chưa có phần mô tả ngắn cho dự án này.'}
                    </p>
                  </div>

                  <div className="pt-3 border-t border-white/5 flex items-center justify-between">
                    <span className="text-[11px] font-mono text-slate-400 flex items-center gap-1.5">
                      <Globe className="w-3.5 h-3.5 text-slate-500" />
                      {project.websiteCount} Websites
                    </span>

                    <span
                      className={`text-[11px] font-bold px-2.5 py-0.5 rounded-md border ${getScoreColor(
                        project.latestScore
                      )}`}
                    >
                      {project.latestScore != null ? `Điểm: ${project.latestScore}` : 'Chưa có audit'}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* MODAL: Tạo Dự Án Mới */}
      {showCreateProject && (
        <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="hallmark-card rounded-2xl max-w-md w-full p-6 border border-white/15 animate-in fade-in zoom-in-95 duration-150">
            <h3 className="text-sm font-bold text-white mb-1">Tạo Dự án Mới</h3>
            <p className="text-xs text-slate-400 mb-4">
              Nhập tên và thông tin cơ bản để gom nhóm các website theo dõi.
            </p>

            <form onSubmit={handleCreateProject} className="space-y-4">
              <div>
                <label className="block text-[11px] font-mono text-slate-300 uppercase mb-1.5">
                  Tên dự án <span className="text-rose-400">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={projectName}
                  onChange={(e) => setProjectName(e.target.value)}
                  placeholder="Ví dụ: Hệ thống E-commerce Khách hàng A"
                  className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs"
                />
              </div>

              <div>
                <label className="block text-[11px] font-mono text-slate-300 uppercase mb-1.5">
                  Mô tả dự án (Tùy chọn)
                </label>
                <textarea
                  rows={3}
                  value={projectDesc}
                  onChange={(e) => setProjectDesc(e.target.value)}
                  placeholder="Mô tả mục tiêu SEO hoặc ghi chú cho dự án..."
                  className="w-full hallmark-input rounded-xl px-4 py-2 text-xs resize-none"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreateProject(false)}
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold transition"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={submitting || !projectName.trim()}
                  className="px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold transition flex items-center gap-1.5 disabled:opacity-50"
                >
                  {submitting && <RefreshCw className="w-3.5 h-3.5 animate-spin" />}
                  Tạo Dự Án
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: Thêm Website Vào Dự Án */}
      {showAddWebsite && selectedProject && (
        <div className="fixed inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4">
          <div className="hallmark-card rounded-2xl max-w-md w-full p-6 border border-white/15 animate-in fade-in zoom-in-95 duration-150">
            <h3 className="text-sm font-bold text-white mb-1">Thêm Website vào Dự án</h3>
            <p className="text-xs text-slate-400 mb-4">
              Thêm URL website vào <strong className="text-indigo-300">{selectedProject.name}</strong>.
            </p>

            <form onSubmit={handleAddWebsite} className="space-y-4">
              <div>
                <label className="block text-[11px] font-mono text-slate-300 uppercase mb-1.5">
                  URL Website <span className="text-rose-400">*</span>
                </label>
                <input
                  type="url"
                  required
                  value={websiteUrl}
                  onChange={(e) => {
                    setWebsiteUrl(e.target.value);
                    if (!websiteName && e.target.value) {
                      try {
                        const host = new URL(e.target.value).hostname;
                        setWebsiteName(host);
                      } catch {
                        // ignore until valid url
                      }
                    }
                  }}
                  placeholder="https://example.com"
                  className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs font-mono"
                />
              </div>

              <div>
                <label className="block text-[11px] font-mono text-slate-300 uppercase mb-1.5">
                  Tên hiển thị <span className="text-rose-400">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={websiteName}
                  onChange={(e) => setWebsiteName(e.target.value)}
                  placeholder="Ví dụ: Trang chủ Example"
                  className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-2">
                <button
                  type="button"
                  onClick={() => setShowAddWebsite(false)}
                  className="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold transition"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  disabled={submitting || !websiteUrl.trim() || !websiteName.trim()}
                  className="px-5 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white text-xs font-semibold transition flex items-center gap-1.5 disabled:opacity-50"
                >
                  {submitting && <RefreshCw className="w-3.5 h-3.5 animate-spin" />}
                  Thêm Website
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
