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
  Code2
} from 'lucide-react';

export default function Home() {
  // Auth & User State
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);

  // Form Tabs (Auth View)
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login');

  // Dashboard Tabs (Dashboard View)
  const [activeTab, setActiveTab] = useState<'overview' | 'profile' | 'security' | 'tokens'>('overview');

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

  // Quick Demo fill
  const fillDemoLogin = () => {
    setLoginEmail('quan@example.com');
    setLoginPassword('Password123!');
  };

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-[#090d16] text-slate-200">
        <div className="flex flex-col items-center gap-4">
          <RefreshCw className="w-10 h-10 animate-spin text-indigo-500" />
          <p className="text-sm font-medium tracking-wide text-slate-400">Đang kết nối tới API Gateway (Port 8000)...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex flex-col bg-[#090d16] text-slate-100 selection:bg-indigo-500 selection:text-white">
      {/* Background Gradient Mesh */}
      <div className="fixed inset-0 pointer-events-none z-0 overflow-hidden">
        <div className="absolute -top-40 -left-40 w-96 h-96 bg-indigo-600/20 rounded-full blur-[128px]"></div>
        <div className="absolute top-1/2 -right-40 w-96 h-96 bg-purple-600/15 rounded-full blur-[128px]"></div>
        <div className="absolute -bottom-40 left-1/3 w-96 h-96 bg-blue-600/15 rounded-full blur-[128px]"></div>
      </div>

      {/* Header Bar */}
      <header className="relative z-10 border-b border-slate-800/80 bg-slate-950/40 backdrop-blur-md px-6 py-4">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-500 flex items-center justify-center shadow-lg shadow-indigo-500/25">
              <Sparkles className="w-5 h-5 text-white" />
            </div>
            <div>
              <h1 className="font-bold text-lg tracking-tight bg-clip-text text-transparent bg-gradient-to-r from-white via-slate-200 to-indigo-200">
                SEO-Auto-V2
              </h1>
              <p className="text-xs text-slate-400 font-medium">Enterprise SaaS Platform</p>
            </div>
          </div>

          <div className="flex items-center gap-4">
            <div className="hidden md:flex items-center gap-2 px-3 py-1.5 rounded-full bg-slate-900/80 border border-slate-800 text-xs text-emerald-400">
              <span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
              Gateway: Port 8000 (Active)
            </div>

            {user ? (
              <div className="flex items-center gap-3 border-l border-slate-800 pl-4">
                <div className="text-right hidden sm:block">
                  <p className="text-sm font-semibold text-slate-200">{user.fullName}</p>
                  <p className="text-xs text-slate-400">{user.email}</p>
                </div>
                <button
                  onClick={handleLogout}
                  className="p-2 rounded-lg bg-slate-900 border border-slate-800 hover:bg-rose-500/10 hover:border-rose-500/30 hover:text-rose-400 text-slate-400 transition"
                  title="Đăng xuất"
                >
                  <LogOut className="w-4 h-4" />
                </button>
              </div>
            ) : null}
          </div>
        </div>
      </header>

      {/* Main Container */}
      <main className="relative z-10 flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 md:p-8">
        {/* Global Action Alert Notification */}
        {actionMessage && (
          <div
            className={`mb-6 p-4 rounded-xl flex items-center gap-3 border backdrop-blur-md transition-all animate-in fade-in slide-in-from-top-4 ${
              actionMessage.type === 'success'
                ? 'bg-emerald-950/40 border-emerald-500/30 text-emerald-300'
                : 'bg-rose-950/40 border-rose-500/30 text-rose-300'
            }`}
          >
            {actionMessage.type === 'success' ? (
              <CheckCircle2 className="w-5 h-5 text-emerald-400 shrink-0" />
            ) : (
              <AlertCircle className="w-5 h-5 text-rose-400 shrink-0" />
            )}
            <p className="text-sm font-medium">{actionMessage.text}</p>
          </div>
        )}

        {/* AUTHENTICATION VIEW (When not logged in) */}
        {!user ? (
          <div className="max-w-md mx-auto my-8">
            <div className="glass-panel rounded-2xl p-8 shadow-2xl glow-indigo">
              <div className="text-center mb-8">
                <h2 className="text-2xl font-bold tracking-tight text-white mb-2">
                  {authMode === 'login' ? 'Đăng nhập Hệ thống' : 'Tạo Tài khoản mới'}
                </h2>
                <p className="text-sm text-slate-400">
                  {authMode === 'login'
                    ? 'Nhập thông tin xác thực để kết nối API Gateway'
                    : 'Điền thông tin bên dưới để khởi tạo tài khoản mới'}
                </p>
              </div>

              {/* Toggle Mode Tabs */}
              <div className="grid grid-cols-2 gap-1 p-1 bg-slate-900/80 rounded-xl mb-6 border border-slate-800">
                <button
                  type="button"
                  onClick={() => setAuthMode('login')}
                  className={`py-2 text-sm font-medium rounded-lg transition-all ${
                    authMode === 'login'
                      ? 'bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-md'
                      : 'text-slate-400 hover:text-white'
                  }`}
                >
                  Đăng nhập
                </button>
                <button
                  type="button"
                  onClick={() => setAuthMode('register')}
                  className={`py-2 text-sm font-medium rounded-lg transition-all ${
                    authMode === 'register'
                      ? 'bg-gradient-to-r from-indigo-600 to-violet-600 text-white shadow-md'
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
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Email
                    </label>
                    <div className="relative">
                      <Mail className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-400" />
                      <input
                        type="email"
                        required
                        value={loginEmail}
                        onChange={(e) => setLoginEmail(e.target.value)}
                        placeholder="user@example.com"
                        className="w-full glass-input rounded-xl pl-10 pr-4 py-2.5 text-sm"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Mật khẩu
                    </label>
                    <div className="relative">
                      <Lock className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-400" />
                      <input
                        type="password"
                        required
                        value={loginPassword}
                        onChange={(e) => setLoginPassword(e.target.value)}
                        placeholder="••••••••"
                        className="w-full glass-input rounded-xl pl-10 pr-4 py-2.5 text-sm"
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="w-full py-3 rounded-xl bg-gradient-to-r from-indigo-600 via-violet-600 to-purple-600 text-white font-semibold text-sm hover:brightness-110 active:scale-[0.99] transition shadow-lg shadow-indigo-600/30 flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    {submitting ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : (
                      <>
                        Xác thực & Vào Dashboard <ArrowRight className="w-4 h-4" />
                      </>
                    )}
                  </button>

                  {/* Demo Quick Fill Button */}
                  <button
                    type="button"
                    onClick={fillDemoLogin}
                    className="w-full mt-2 py-2 rounded-xl bg-slate-900 border border-slate-800 text-xs text-slate-400 hover:text-indigo-400 hover:border-slate-700 transition"
                  >
                    💡 Tự điền dữ liệu mẫu (quan@example.com)
                  </button>
                </form>
              ) : (
                /* Register Form */
                <form onSubmit={handleRegister} className="space-y-4">
                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Họ và Tên
                    </label>
                    <div className="relative">
                      <User className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-400" />
                      <input
                        type="text"
                        required
                        value={regFullName}
                        onChange={(e) => setRegFullName(e.target.value)}
                        placeholder="Nguyen Hoanh Quan"
                        className="w-full glass-input rounded-xl pl-10 pr-4 py-2.5 text-sm"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Email
                    </label>
                    <div className="relative">
                      <Mail className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-400" />
                      <input
                        type="email"
                        required
                        value={regEmail}
                        onChange={(e) => setRegEmail(e.target.value)}
                        placeholder="quan@example.com"
                        className="w-full glass-input rounded-xl pl-10 pr-4 py-2.5 text-sm"
                      />
                    </div>
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Mật khẩu (Tối thiểu 8 ký tự)
                    </label>
                    <div className="relative">
                      <Lock className="w-4 h-4 absolute left-3.5 top-3.5 text-slate-400" />
                      <input
                        type="password"
                        required
                        minLength={8}
                        value={regPassword}
                        onChange={(e) => setRegPassword(e.target.value)}
                        placeholder="Password123!"
                        className="w-full glass-input rounded-xl pl-10 pr-4 py-2.5 text-sm"
                      />
                    </div>
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="w-full py-3 rounded-xl bg-gradient-to-r from-emerald-600 to-teal-600 text-white font-semibold text-sm hover:brightness-110 active:scale-[0.99] transition shadow-lg shadow-emerald-600/30 flex items-center justify-center gap-2 disabled:opacity-50"
                  >
                    {submitting ? (
                      <RefreshCw className="w-4 h-4 animate-spin" />
                    ) : (
                      <>
                        Tạo Tài khoản <Sparkles className="w-4 h-4" />
                      </>
                    )}
                  </button>
                </form>
              )}
            </div>
          </div>
        ) : (
          /* DASHBOARD VIEW (When Logged In) */
          <div className="space-y-6">
            {/* User Welcome Banner */}
            <div className="glass-panel rounded-2xl p-6 relative overflow-hidden flex flex-col md:flex-row items-start md:items-center justify-between gap-4">
              <div className="flex items-center gap-4">
                <div className="w-14 h-14 rounded-2xl bg-gradient-to-tr from-indigo-500 via-purple-500 to-pink-500 flex items-center justify-center text-xl font-bold text-white shadow-xl">
                  {user.fullName.charAt(0).toUpperCase()}
                </div>
                <div>
                  <h2 className="text-xl font-bold text-white flex items-center gap-2">
                    Xin chào, {user.fullName}!
                    <span className="text-xs px-2.5 py-0.5 rounded-full bg-indigo-500/20 text-indigo-300 border border-indigo-500/30 font-semibold">
                      {user.role}
                    </span>
                  </h2>
                  <p className="text-xs text-slate-400 mt-1">
                    ID: <code className="text-indigo-300 font-mono">{user.id}</code> • Ngày tạo:{' '}
                    {new Date(user.createdAt).toLocaleDateString('vi-VN')}
                  </p>
                </div>
              </div>

              <div className="flex items-center gap-2">
                <button
                  onClick={handleManualRefreshToken}
                  disabled={submitting}
                  className="px-4 py-2 rounded-xl bg-slate-900 border border-slate-800 hover:border-indigo-500/50 text-xs font-semibold text-indigo-300 hover:text-white transition flex items-center gap-2"
                >
                  <RefreshCw className={`w-3.5 h-3.5 ${submitting ? 'animate-spin' : ''}`} />
                  Refresh Token
                </button>
              </div>
            </div>

            {/* Dashboard Tabs Bar */}
            <div className="flex items-center gap-2 border-b border-slate-800 pb-2">
              <button
                onClick={() => setActiveTab('overview')}
                className={`px-4 py-2.5 text-sm font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'overview'
                    ? 'bg-indigo-600/20 text-indigo-400 border border-indigo-500/30'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
                }`}
              >
                <Activity className="w-4 h-4" /> Hạ tầng & Trạng thái
              </button>

              <button
                onClick={() => setActiveTab('profile')}
                className={`px-4 py-2.5 text-sm font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'profile'
                    ? 'bg-indigo-600/20 text-indigo-400 border border-indigo-500/30'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
                }`}
              >
                <Edit3 className="w-4 h-4" /> Cập nhật Profile
              </button>

              <button
                onClick={() => setActiveTab('security')}
                className={`px-4 py-2.5 text-sm font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'security'
                    ? 'bg-indigo-600/20 text-indigo-400 border border-indigo-500/30'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
                }`}
              >
                <Key className="w-4 h-4" /> Bảo mật & Đổi mật khẩu
              </button>

              <button
                onClick={() => setActiveTab('tokens')}
                className={`px-4 py-2.5 text-sm font-semibold rounded-xl transition flex items-center gap-2 ${
                  activeTab === 'tokens'
                    ? 'bg-indigo-600/20 text-indigo-400 border border-indigo-500/30'
                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-900/50'
                }`}
              >
                <Code2 className="w-4 h-4" /> JWT Claims Debugger
              </button>
            </div>

            {/* TAB 1: OVERVIEW */}
            {activeTab === 'overview' && (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div className="glass-panel p-5 rounded-2xl">
                  <div className="flex items-center justify-between mb-3">
                    <Server className="w-6 h-6 text-indigo-400" />
                    <span className="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
                  </div>
                  <h3 className="text-sm font-semibold text-slate-300">YARP API Gateway</h3>
                  <p className="text-2xl font-bold text-white mt-1">Port 8000</p>
                  <p className="text-xs text-slate-400 mt-2">Định tuyến RESTful Proxy</p>
                </div>

                <div className="glass-panel p-5 rounded-2xl">
                  <div className="flex items-center justify-between mb-3">
                    <Cpu className="w-6 h-6 text-purple-400" />
                    <span className="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
                  </div>
                  <h3 className="text-sm font-semibold text-slate-300">Identity Service</h3>
                  <p className="text-2xl font-bold text-white mt-1">Port 5001</p>
                  <p className="text-xs text-slate-400 mt-2">.NET 10 • Vertical Slice</p>
                </div>

                <div className="glass-panel p-5 rounded-2xl">
                  <div className="flex items-center justify-between mb-3">
                    <Database className="w-6 h-6 text-blue-400" />
                    <span className="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
                  </div>
                  <h3 className="text-sm font-semibold text-slate-300">PostgreSQL Database</h3>
                  <p className="text-2xl font-bold text-white mt-1">seo_auto_identity_db</p>
                  <p className="text-xs text-slate-400 mt-2">Docker Container (5432)</p>
                </div>

                <div className="glass-panel p-5 rounded-2xl">
                  <div className="flex items-center justify-between mb-3">
                    <ShieldCheck className="w-6 h-6 text-emerald-400" />
                    <span className="w-2.5 h-2.5 rounded-full bg-emerald-500"></span>
                  </div>
                  <h3 className="text-sm font-semibold text-slate-300">Cơ chế Bảo mật</h3>
                  <p className="text-2xl font-bold text-white mt-1">JWT + BCrypt</p>
                  <p className="text-xs text-slate-400 mt-2">Refresh Token Rotation</p>
                </div>
              </div>
            )}

            {/* TAB 2: UPDATE PROFILE */}
            {activeTab === 'profile' && (
              <div className="glass-panel p-6 rounded-2xl max-w-2xl">
                <h3 className="text-lg font-bold text-white mb-4">Cập nhật Hồ sơ cá nhân (`PUT /api/users/me`)</h3>
                <form onSubmit={handleUpdateProfile} className="space-y-4">
                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Họ và Tên
                    </label>
                    <input
                      type="text"
                      required
                      value={updateFullName}
                      onChange={(e) => setUpdateFullName(e.target.value)}
                      className="w-full glass-input rounded-xl px-4 py-2.5 text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Địa chỉ Email
                    </label>
                    <input
                      type="email"
                      required
                      value={updateEmail}
                      onChange={(e) => setUpdateEmail(e.target.value)}
                      className="w-full glass-input rounded-xl px-4 py-2.5 text-sm"
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="px-6 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-semibold text-sm transition shadow-lg shadow-indigo-600/30 flex items-center gap-2"
                  >
                    {submitting && <RefreshCw className="w-4 h-4 animate-spin" />}
                    Lưu Thay Đổi
                  </button>
                </form>
              </div>
            )}

            {/* TAB 3: CHANGE PASSWORD */}
            {activeTab === 'security' && (
              <div className="glass-panel p-6 rounded-2xl max-w-2xl">
                <h3 className="text-lg font-bold text-white mb-4">Đổi Mật Khẩu (`PUT /api/users/me/password`)</h3>
                <form onSubmit={handleChangePassword} className="space-y-4">
                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Mật khẩu Hiện tại
                    </label>
                    <input
                      type="password"
                      required
                      value={oldPassword}
                      onChange={(e) => setOldPassword(e.target.value)}
                      placeholder="••••••••"
                      className="w-full glass-input rounded-xl px-4 py-2.5 text-sm"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-semibold text-slate-300 uppercase tracking-wider mb-2">
                      Mật khẩu Mới
                    </label>
                    <input
                      type="password"
                      required
                      minLength={8}
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      placeholder="Mật khẩu mới tối thiểu 8 ký tự"
                      className="w-full glass-input rounded-xl px-4 py-2.5 text-sm"
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={submitting}
                    className="px-6 py-2.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white font-semibold text-sm transition shadow-lg shadow-purple-600/30 flex items-center gap-2"
                  >
                    {submitting && <RefreshCw className="w-4 h-4 animate-spin" />}
                    Cập nhật Mật khẩu
                  </button>
                </form>
              </div>
            )}

            {/* TAB 4: TOKEN DEBUGGER */}
            {activeTab === 'tokens' && (
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="glass-panel p-6 rounded-2xl">
                  <h3 className="text-sm font-bold uppercase tracking-wider text-indigo-400 mb-3">
                    🔑 Access Token (JWT)
                  </h3>
                  <div className="p-3 bg-slate-950 rounded-xl border border-slate-800 font-mono text-xs text-slate-300 break-all max-h-48 overflow-y-auto">
                    {getAccessToken() || 'Chưa có Token'}
                  </div>
                </div>

                <div className="glass-panel p-6 rounded-2xl">
                  <h3 className="text-sm font-bold uppercase tracking-wider text-purple-400 mb-3">
                    🔄 Refresh Token
                  </h3>
                  <div className="p-3 bg-slate-950 rounded-xl border border-slate-800 font-mono text-xs text-slate-300 break-all max-h-48 overflow-y-auto">
                    {getRefreshToken() || 'Chưa có Token'}
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </main>

      {/* Footer */}
      <footer className="relative z-10 border-t border-slate-800/60 bg-slate-950/20 py-4 text-center text-xs text-slate-400">
        <p>SEO-Auto-V2 Enterprise SaaS Platform • Monorepo Architecture (.NET 10 + Next.js)</p>
      </footer>
    </div>
  );
}
