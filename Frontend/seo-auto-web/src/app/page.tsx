/* Hallmark · pre-emit critique: P5 H5 E5 S5 R5 V5 */
'use client';

import { useState, useEffect } from 'react';
import { api, getAccessToken, clearTokens, UserProfile, getRefreshToken } from '@/lib/api';
import { 
  ShieldCheck, 
  User, 
  Lock, 
  Mail, 
  Key, 
  LogOut, 
  Activity, 
  CheckCircle2, 
  AlertCircle, 
  RefreshCw, 
  Sparkles, 
  Database, 
  Server, 
  Cpu, 
  Edit3, 
  ArrowRight,
  Code2,
  Terminal,
  Layers,
  Zap,
  Globe,
  Search,
  BarChart3,
  Check,
  LayoutDashboard,
  ShieldAlert
} from 'lucide-react';

export default function Home() {
  // Auth & User State
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);

  // Form Tabs (Auth View)
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');

  // Dashboard Tabs (Dashboard View)
  const [activeTab, setActiveTab] = useState<'audit' | 'profile' | 'security' | 'system'>('audit');

  // Input States
  const [loginEmail, setLoginEmail] = useState('');
  const [loginPassword, setLoginPassword] = useState('');

  const [regFullName, setRegFullName] = useState('');
  const [regEmail, setRegEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');

  const [updateFullName, setUpdateFullName] = useState('');
  const [updateEmail, setUpdateEmail] = useState('');

  const [oldPassword, setOldPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');

  // SEO Audit Demo Search State
  const [auditUrl, setAuditUrl] = useState('');
  const [isAuditing, setIsAuditing] = useState(false);

  // Status & Notification Messages
  const [actionMessage, setActionMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [submitting, setSubmitting] = useState(false);

  // Check Auth Status on Mount
  useEffect(() => {
    checkAuth();
  }, []);

  const checkAuth = async () => {
    const token = getAccessToken();
    if (!token) {
      setLoading(false);
      return;
    }

    try {
      const profile = await api.getProfile();
      setUser(profile);
      setUpdateFullName(profile.fullName);
      setUpdateEmail(profile.email);
    } catch (err: any) {
      console.error('Failed to load profile:', err);
      clearTokens();
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  // Helper to show temporary alert
  const showAlert = (type: 'success' | 'error', text: string) => {
    setActionMessage({ type, text });
    setTimeout(() => setActionMessage(null), 5000);
  };

  // Auth Handlers
  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setActionMessage(null);

    try {
      await api.login({ email: loginEmail, password: loginPassword });
      showAlert('success', 'Đăng nhập thành công!');
      await checkAuth();
    } catch (err: any) {
      showAlert('error', err.message || 'Đăng nhập thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setActionMessage(null);

    try {
      await api.register({ fullName: regFullName, email: regEmail, password: regPassword });
      showAlert('success', 'Đăng ký tài khoản thành công! Vui lòng đăng nhập.');
      setAuthMode('login');
      setLoginEmail(regEmail);
      setLoginPassword('');
    } catch (err: any) {
      showAlert('error', err.message || 'Đăng ký thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setActionMessage(null);

    try {
      const updated = await api.updateProfile({ fullName: updateFullName, email: updateEmail });
      setUser(updated);
      showAlert('success', 'Cập nhật thông tin cá nhân thành công!');
    } catch (err: any) {
      showAlert('error', err.message || 'Cập nhật thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setActionMessage(null);

    try {
      await api.changePassword({ oldPassword, newPassword });
      showAlert('success', 'Đổi mật khẩu thành công!');
      setOldPassword('');
      setNewPassword('');
    } catch (err: any) {
      showAlert('error', err.message || 'Đổi mật khẩu thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleManualRefreshToken = async () => {
    setSubmitting(true);
    setActionMessage(null);

    try {
      await api.refreshToken();
      showAlert('success', 'Refresh Token thành công! Đã cấp mới Access Token.');
      await checkAuth();
    } catch (err: any) {
      showAlert('error', err.message || 'Refresh Token thất bại');
    } finally {
      setSubmitting(false);
    }
  };

  const handleLogout = () => {
    clearTokens();
    setUser(null);
    showAlert('success', 'Đã đăng xuất khỏi hệ thống');
  };

  const fillDemoLogin = () => {
    setLoginEmail('quan@example.com');
    setLoginPassword('Password123!');
  };

  const handleStartAudit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!auditUrl) return;
    setIsAuditing(true);
    setTimeout(() => {
      setIsAuditing(false);
      showAlert('success', `Đã nhận yêu cầu phân tích URL: ${auditUrl}. Module Audit Service (Tháng 2) sẽ xử lý dữ liệu này!`);
    }, 1500);
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#030712] text-slate-200">
        <div className="flex flex-col items-center gap-4">
          <RefreshCw className="w-8 h-8 animate-spin text-indigo-500" />
          <p className="text-xs font-mono text-slate-400 uppercase tracking-widest">Đang tải cấu hình người dùng...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col bg-[#030712] text-slate-100 bg-grid-pattern relative">
      {/* Hallmark Ambient Glow Elements */}
      <div className="fixed top-0 left-1/4 w-96 h-96 bg-indigo-900/15 rounded-full blur-[140px] pointer-events-none"></div>
      <div className="fixed bottom-0 right-1/4 w-96 h-96 bg-purple-900/10 rounded-full blur-[140px] pointer-events-none"></div>

      {/* Header Bar */}
      <header className="relative z-10 border-b border-white/[0.08] bg-[#030712]/80 backdrop-blur-xl px-6 py-3.5">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-8 h-8 rounded-lg bg-indigo-600 flex items-center justify-center text-white shadow-md shadow-indigo-600/30">
              <Zap className="w-4 h-4" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <span className="font-bold tracking-tight text-sm text-white">SEO-AUTO-V2</span>
                <span className="text-[10px] font-mono uppercase tracking-wider px-2 py-0.5 rounded bg-indigo-500/15 text-indigo-400 border border-indigo-500/30">
                  {user?.role === 'Admin' ? 'Admin Portal' : 'User Platform'}
                </span>
              </div>
            </div>
          </div>

          <div className="flex items-center gap-4">
            {user ? (
              <div className="flex items-center gap-3 border-l border-white/10 pl-4">
                <div className="text-right hidden md:block">
                  <p className="text-xs font-semibold text-slate-200">{user.fullName}</p>
                  <p className="text-[10px] font-mono text-slate-400">{user.email}</p>
                </div>
                <button
                  onClick={handleLogout}
                  className="p-2 rounded-lg bg-slate-900 border border-white/10 hover:bg-rose-500/10 hover:border-rose-500/30 hover:text-rose-400 text-slate-400 transition"
                  title="Đăng xuất"
                >
                  <LogOut className="w-3.5 h-3.5" />
                </button>
              </div>
            ) : null}
          </div>
        </div>
      </header>

      {/* Main Content Area */}
      <main className="relative z-10 flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 md:p-8">
        {/* Global Action Alert Notification */}
        {actionMessage && (
          <div
            className={`mb-6 p-4 rounded-xl flex items-center gap-3 border transition-all ${
              actionMessage.type === 'success'
                ? 'bg-emerald-950/30 border-emerald-500/30 text-emerald-300'
                : 'bg-rose-950/30 border-rose-500/30 text-rose-300'
            }`}
          >
            {actionMessage.type === 'success' ? (
              <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0" />
            ) : (
              <AlertCircle className="w-5 h-5 text-rose-400 shrink-0" />
            )}
            <p className="text-xs font-mono">{actionMessage.text}</p>
          </div>
        )}

        {/* AUTHENTICATION VIEW (Guest Mode) */}
        {!user ? (
          <div className="max-w-md mx-auto my-10">
            <div className="hallmark-card rounded-2xl p-8 relative">
              <div className="text-center mb-8">
                <h2 className="text-xl font-bold tracking-tight text-white mb-2">
                  {authMode === 'login' ? 'Đăng nhập Khách hàng' : 'Đăng ký Tài khoản mới'}
                </h2>
                <p className="text-xs text-slate-400 font-mono">
                  SEO Optimization & Performance Platform
                </p>
              </div>

              {/* Segmented Auth Control */}
              <div className="grid grid-cols-2 gap-1 p-1 bg-slate-950 rounded-xl mb-6 border border-white/10">
                <button
                  type="button"
                  onClick={() => setAuthMode('login')}
                  className={`py-2 text-xs font-semibold rounded-lg transition-all ${
                    authMode === 'login'
                      ? 'bg-indigo-600 text-white shadow-md'
                      : 'text-slate-400 hover:text-white'
                  }`}
                >
                  Đăng nhập
                </button>
                <button
                  type="button"
                  onClick={() => setAuthMode('register')}
                  className={`py-2 text-xs font-semibold rounded-lg transition-all ${
                    authMode === 'register'
                      ? 'bg-indigo-600 text-white shadow-md'
                      : 'text-slate-400 hover:text-white'
                  }`}
                >
                  Đăng ký
                </button>
              </div>

              {/* Login Form */}
              {authMode === 'login' ? (
                <form onSubmit={handleLogin} className="space-y-4">
                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase tracking-wider mb-2">
                      Email address
                    </label>
                    <div className="relative">
                      <Mail className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
                      <input
                        type="email"
                        required
                        value={loginEmail}
                        onChange={(e) => setLoginEmail(e.target.value)}
                        placeholder="quan@example.com"
                        className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase tracking-wider mb-2">
                      Password
                    </label>
                    <div className="relative">
                      <Lock className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
                      <input
                        type="password"
                        required
                        value={loginPassword}
                        onChange={(e) => setLoginPassword(e.target.value)}
                        placeholder="••••••••"
                        className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs"
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="w-full py-3 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-xs transition shadow-lg shadow-indigo-600/25 flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    {submitting ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : (
                      <>
                        Vào Workspace Khách hàng <ArrowRight className="w-4 h-4" />
                      </>
                    )}
                  </button>

                  <button
                    type="button"
                    onClick={fillDemoLogin}
                    className="w-full mt-2 py-2 rounded-xl bg-slate-950 border border-white/10 text-[11px] font-mono text-slate-400 hover:text-indigo-400 hover:border-slate-700 transition"
                  >
                    💡 Tự điền dữ liệu mẫu (quan@example.com)
                  </button>
                </form>
              ) : (
                /* Register Form */
                <form onSubmit={handleRegister} className="space-y-4">
                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase tracking-wider mb-2">
                      Full Name
                    </label>
                    <div className="relative">
                      <User className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
                      <input
                        type="text"
                        required
                        value={regFullName}
                        onChange={(e) => setRegFullName(e.target.value)}
                        placeholder="Nguyen Hoanh Quan"
                        className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase tracking-wider mb-2">
                      Email address
                    </label>
                    <div className="relative">
                      <Mail className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
                      <input
                        type="email"
                        required
                        value={regEmail}
                        onChange={(e) => setRegEmail(e.target.value)}
                        placeholder="quan@example.com"
                        className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase tracking-wider mb-2">
                      Password (min 8 chars)
                    </label>
                    <div className="relative">
                      <Lock className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
                      <input
                        type="password"
                        required
                        minLength={8}
                        value={regPassword}
                        onChange={(e) => setRegPassword(e.target.value)}
                        placeholder="Password123!"
                        className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs"
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="w-full py-3 rounded-xl bg-emerald-600 hover:bg-emerald-500 text-white font-semibold text-xs transition shadow-lg shadow-emerald-600/25 flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    {submitting ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : (
                      <>
                        Khởi tạo Tài khoản <Sparkles className="w-4 h-4" />
                      </>
                    )}
                  </button>
                </form>
              )}
            </div>
          </div>
        ) : (
          /* REGULAR USER WORKSPACE VIEW */
          <div className="space-y-6">
            {/* User Welcome Banner */}
            <div className="hallmark-card rounded-2xl p-6 flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
              <div className="flex items-center gap-4">
                <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-lg font-bold text-white shadow-lg">
                  {user.fullName.charAt(0).toUpperCase()}
                </div>
                <div>
                  <h2 className="text-lg font-bold text-white flex items-center gap-2">
                    Chào mừng trở lại, {user.fullName}!
                    <span className="text-[10px] font-mono uppercase px-2 py-0.5 rounded bg-indigo-500/20 text-indigo-300 border border-indigo-500/30">
                      Tài khoản {user.role}
                    </span>
                  </h2>
                  <p className="text-xs text-slate-400 mt-0.5">
                    Hệ thống phân tích & Tối ưu hóa SEO tự động bằng Trí tuệ Nhân tạo AI
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                {/* Role Switcher Simulator for Testing */}
                <button
                  onClick={() => {
                    const newRole = user.role === 'Admin' ? 'User' : 'Admin';
                    setUser({ ...user, role: newRole });
                    showAlert('success', `Đã chuyển chế độ xem sang: ${newRole}`);
                  }}
                  className="px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 hover:border-indigo-500/50 text-[11px] font-mono text-slate-300 transition flex items-center gap-1.5"
                >
                  <ShieldAlert className="w-3.5 h-3.5 text-indigo-400" />
                  Đổi vai trò: {user.role === 'Admin' ? 'Khách hàng' : 'Quản trị viên (Admin)'}
                </button>
              </div>
            </div>

            {/* Navigation Tabs Bar */}
            <div className="flex items-center gap-2 border-b border-white/10 pb-3">
              <button
                onClick={() => setActiveTab('audit')}
                className={`px-4 py-2 text-xs font-mono font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'audit'
                    ? 'bg-indigo-600 text-white'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900'
                }`}
              >
                <Globe className="w-3.5 h-3.5" /> Phân tích Website (SEO Audit)
              </button>

              <button
                onClick={() => setActiveTab('profile')}
                className={`px-4 py-2 text-xs font-mono font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'profile'
                    ? 'bg-indigo-600 text-white'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900'
                }`}
              >
                <Edit3 className="w-3.5 h-3.5" /> Hồ sơ cá nhân
              </button>

              <button
                onClick={() => setActiveTab('security')}
                className={`px-4 py-2 text-xs font-mono font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'security'
                    ? 'bg-indigo-600 text-white'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900'
                }`}
              >
                <Key className="w-3.5 h-3.5" /> Bảo mật & Đổi mật khẩu
              </button>

              {/* ADMIN ONLY TAB */}
              {user.role === 'Admin' && (
                <button
                  onClick={() => setActiveTab('system')}
                  className={`px-4 py-2 text-xs font-mono font-semibold rounded-xl transition flex items-center gap-2 border border-purple-500/30 ${
                    activeTab === 'system'
                      ? 'bg-purple-600 text-white'
                      : 'text-purple-400 hover:bg-purple-950/30'
                  }`}
                >
                  <Terminal className="w-3.5 h-3.5" /> 🛠 Quản trị Hệ thống (Admin Only)
                </button>
              )}
            </div>

            {/* TAB 1: SEO AUDIT SEARCH WORKSPACE (CUSTOMER VIEW) */}
            {activeTab === 'audit' && (
              <div className="space-y-6">
                {/* Search / Audit URL Bar */}
                <div className="hallmark-card p-6 rounded-2xl">
                  <h3 className="text-sm font-bold text-white mb-1">Kiểm tra & Tối ưu SEO Website ngay tức thì</h3>
                  <p className="text-xs text-slate-400 mb-4">
                    Nhập đường dẫn trang web của bạn để Google PageSpeed API & Gemini AI phân tích điểm số
                  </p>

                  <form onSubmit={handleStartAudit} className="flex flex-col sm:flex-row gap-3">
                    <div className="relative flex-1">
                      <Globe className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-500" />
                      <input
                        type="url"
                        required
                        value={auditUrl}
                        onChange={(e) => setAuditUrl(e.target.value)}
                        placeholder="https://mywebsite.com"
                        className="w-full hallmark-input rounded-xl pl-10 pr-4 py-2.5 text-xs font-mono"
                      />
                    </div>
                    <button
                      type="submit"
                      disabled={isAuditing}
                      className="px-6 py-2.5 rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 text-white font-semibold text-xs transition shadow-lg shadow-indigo-600/30 flex items-center justify-center gap-2 disabled:opacity-50"
                    >
                      {isAuditing ? (
                        <RefreshCw className="w-4 h-4 animate-spin" />
                      ) : (
                        <>
                          <Search className="w-4 h-4" /> Phân tích SEO bằng AI
                        </>
                      )}
                    </button>
                  </form>
                </div>

                {/* Dashboard Stats for User */}
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-2">
                      <BarChart3 className="w-5 h-5 text-indigo-400" />
                      <span className="text-[10px] font-mono text-slate-400">Tháng 1/Tháng 2</span>
                    </div>
                    <h4 className="text-xs font-mono text-slate-400">Tổng số Website đã Audit</h4>
                    <p className="text-2xl font-bold text-white mt-1">0 Trang</p>
                  </div>

                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-2">
                      <Sparkles className="w-5 h-5 text-purple-400" />
                      <span className="text-[10px] font-mono text-slate-400">Gemini 1.5</span>
                    </div>
                    <h4 className="text-xs font-mono text-slate-400">Đoạn Code đã được AI tối ưu</h4>
                    <p className="text-2xl font-bold text-white mt-1">0 Đoạn Code</p>
                  </div>

                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-2">
                      <ShieldCheck className="w-5 h-5 text-emerald-400" />
                      <span className="text-[10px] font-mono text-slate-400">Realtime</span>
                    </div>
                    <h4 className="text-xs font-mono text-slate-400">Điểm SEO Trung bình</h4>
                    <p className="text-2xl font-bold text-white mt-1">— / 100</p>
                  </div>
                </div>
              </div>
            )}

            {/* TAB 2: UPDATE PROFILE */}
            {activeTab === 'profile' && (
              <div className="hallmark-card p-6 rounded-2xl max-w-xl">
                <h3 className="text-sm font-bold text-white mb-4">Cập nhật Hồ sơ cá nhân</h3>
                <form onSubmit={handleUpdateProfile} className="space-y-4">
                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase mb-2">
                      Họ và Tên
                    </label>
                    <input
                      type="text"
                      required
                      value={updateFullName}
                      onChange={(e) => setUpdateFullName(e.target.value)}
                      className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs"
                    />
                  </div>

                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase mb-2">
                      Email Address
                    </label>
                    <input
                      type="email"
                      required
                      value={updateEmail}
                      onChange={(e) => setUpdateEmail(e.target.value)}
                      className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs"
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="px-5 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-xs transition shadow-md flex items-center gap-2"
                  >
                    {submitting && <RefreshCw className="w-3.5 h-3.5 animate-spin" />}
                    Lưu Thay Đổi
                  </button>
                </form>
              </div>
            )}

            {/* TAB 3: CHANGE PASSWORD */}
            {activeTab === 'security' && (
              <div className="hallmark-card p-6 rounded-2xl max-w-xl">
                <h3 className="text-sm font-bold text-white mb-4">Đổi Mật Khẩu</h3>
                <form onSubmit={handleChangePassword} className="space-y-4">
                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase mb-2">
                      Mật khẩu Hiện tại
                    </label>
                    <input
                      type="password"
                      required
                      value={oldPassword}
                      onChange={(e) => setOldPassword(e.target.value)}
                      placeholder="••••••••"
                      className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs"
                    />
                  </div>

                  <div>
                    <label className="block text-[11px] font-mono text-slate-400 uppercase mb-2">
                      Mật khẩu Mới
                    </label>
                    <input
                      type="password"
                      required
                      minLength={8}
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      placeholder="Tối thiểu 8 ký tự"
                      className="w-full hallmark-input rounded-xl px-4 py-2.5 text-xs"
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="px-5 py-2.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white font-semibold text-xs transition shadow-md flex items-center gap-2"
                  >
                    {submitting && <RefreshCw className="w-3.5 h-3.5 animate-spin" />}
                    Cập nhật Mật khẩu
                  </button>
                </form>
              </div>
            )}

            {/* TAB 4: SYSTEM INFRASTRUCTURE (ADMIN ONLY) */}
            {activeTab === 'system' && user.role === 'Admin' && (
              <div className="space-y-6">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-3">
                      <Server className="w-5 h-5 text-indigo-400" />
                      <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                    </div>
                    <h3 className="text-xs font-mono uppercase text-slate-400">YARP API Gateway</h3>
                    <p className="text-xl font-bold text-white mt-1">Port 8000</p>
                  </div>

                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-3">
                      <Cpu className="w-5 h-5 text-purple-400" />
                      <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                    </div>
                    <h3 className="text-xs font-mono uppercase text-slate-400">Identity Service</h3>
                    <p className="text-xl font-bold text-white mt-1">Port 5001</p>
                  </div>

                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-3">
                      <Database className="w-5 h-5 text-blue-400" />
                      <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                    </div>
                    <h3 className="text-xs font-mono uppercase text-slate-400">PostgreSQL DB</h3>
                    <p className="text-xl font-bold text-white mt-1">Port 5432</p>
                  </div>

                  <div className="hallmark-card p-5 rounded-2xl">
                    <div className="flex items-center justify-between mb-3">
                      <Layers className="w-5 h-5 text-emerald-400" />
                      <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
                    </div>
                    <h3 className="text-xs font-mono uppercase text-slate-400">BuildingBlocks</h3>
                    <p className="text-xl font-bold text-white mt-1">ISoftDelete Active</p>
                  </div>
                </div>

                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  <div className="hallmark-card p-6 rounded-2xl">
                    <h3 className="text-xs font-mono uppercase text-indigo-400 mb-3 flex items-center gap-2">
                      <Key className="w-4 h-4" /> Access Token (Bearer JWT)
                    </h3>
                    <div className="p-3 bg-slate-950 rounded-xl border border-white/10 font-mono text-[11px] text-slate-300 break-all max-h-48 overflow-y-auto">
                      {getAccessToken() || 'Chưa có Token'}
                    </div>
                  </div>

                  <div className="hallmark-card p-6 rounded-2xl">
                    <h3 className="text-xs font-mono uppercase text-purple-400 mb-3 flex items-center gap-2">
                      <RefreshCw className="w-4 h-4" /> Refresh Token (7-Day Rotation)
                    </h3>
                    <div className="p-3 bg-slate-950 rounded-xl border border-white/10 font-mono text-[11px] text-slate-300 break-all max-h-48 overflow-y-auto">
                      {getRefreshToken() || 'Chưa có Token'}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </main>

      {/* Footer */}
      <footer className="relative z-10 border-t border-white/[0.08] bg-[#030712] py-4 text-center text-[11px] font-mono text-slate-500">
        <p>SEO-Auto-V2 Enterprise SaaS Platform • Hallmark Design System</p>
      </footer>
    </div>
  );
}
