const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000';

export interface RegisterPayload {
  email: string;
  password: string;
  fullName: string;
}

export interface LoginPayload {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
}

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  role: string;
  createdAt: string;
}

export interface UpdateProfilePayload {
  fullName?: string;
  email?: string;
}

export interface ChangePasswordPayload {
  oldPassword: string;
  newPassword: string;
}

export interface SubmitAuditPayload {
  url: string;
  strategy?: 'Desktop' | 'Mobile';
}

export interface SubmitAuditResponse {
  auditId: string;
  message: string;
}

export interface RawMetrics {
  performanceScore: number;
  accessibilityScore: number;
  bestPracticesScore: number;
  seoScore: number;
  lcpMs: number;
  inpMs: number;
  cls: number;
  ttfbMs: number;
  fcpMs: number;
  speedIndexMs: number;
}

export interface SeoAnalysis {
  title: string;
  metaDescription: string;
  canonicalUrl: string;
  hasRobotsTxt: boolean;
  hasSitemap: boolean;
  h1Count: number;
  imagesWithoutAlt: number;
  openGraphData?: Record<string, string> | null;
  structuredData?: any;
}

export interface AuditDetail {
  id: string;
  userId: string;
  url: string;
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  strategy: 'Desktop' | 'Mobile';
  createdAt: string;
  completedAt?: string;
  errorMessage?: string;
  rawMetrics?: RawMetrics | null;
  seoAnalysis?: SeoAnalysis | null;
}

export interface AuditSummary {
  id: string;
  url: string;
  status: 'Pending' | 'Processing' | 'Completed' | 'Failed';
  strategy: 'Desktop' | 'Mobile';
  createdAt: string;
  completedAt?: string;
  errorMessage?: string;
  performanceScore?: number | null;
  seoScore?: number | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// Projects & Websites Interfaces
export interface CreateProjectPayload {
  name: string;
  description?: string;
}

export interface ProjectSummary {
  id: string;
  name: string;
  description?: string | null;
  createdAt: string;
  websiteCount: number;
  latestScore?: number | null;
}

export interface WebsiteSummary {
  id: string;
  projectId: string;
  url: string;
  name: string;
  faviconUrl?: string | null;
  createdAt: string;
  latestOverallScore?: number | null;
  latestAuditDate?: string | null;
}

export interface ProjectDetail {
  id: string;
  name: string;
  description?: string | null;
  createdAt: string;
  websites: WebsiteSummary[];
}

export interface CreateWebsitePayload {
  projectId: string;
  url: string;
  name: string;
  faviconUrl?: string;
}

export interface WebsiteReportItem {
  id: string;
  auditRequestId: string;
  strategy: string;
  overallScore: number;
  performanceScore: number;
  seoScore: number;
  accessibilityScore: number;
  bestPracticesScore: number;
  createdAt: string;
}

export interface WebsiteDetail {
  id: string;
  projectId: string;
  projectName: string;
  url: string;
  name: string;
  faviconUrl?: string | null;
  createdAt: string;
  latestOverallScore?: number | null;
  latestAuditDate?: string | null;
  recentReports: WebsiteReportItem[];
}

export interface ScoreHistoryPoint {
  date: string;
  overallScore: number;
  performanceScore: number;
  seoScore: number;
  accessibilityScore: number;
  bestPracticesScore: number;
}

export interface ReportHistoryResponse {
  websiteId?: string | null;
  url?: string | null;
  history: ScoreHistoryPoint[];
}

// Token Storage Helpers
export const getAccessToken = () => typeof window !== 'undefined' ? localStorage.getItem('access_token') : null;
export const getRefreshToken = () => typeof window !== 'undefined' ? localStorage.getItem('refresh_token') : null;

export const setTokens = (accessToken: string, refreshToken?: string) => {
  if (typeof window === 'undefined') return;
  localStorage.setItem('access_token', accessToken);
  if (refreshToken) {
    localStorage.setItem('refresh_token', refreshToken);
  }
};

export const clearTokens = () => {
  if (typeof window === 'undefined') return;
  localStorage.removeItem('access_token');
  localStorage.removeItem('refresh_token');
};

// Generic Fetch Wrapper
async function fetchWithAuth(endpoint: string, options: RequestInit = {}) {
  let token = getAccessToken();

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string> || {}),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  let response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  // Attempt auto-refresh if 401 Unauthorized and refresh token exists
  if (response.status === 401 && getRefreshToken()) {
    try {
      const refreshRes = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken: getRefreshToken() }),
      });

      if (refreshRes.ok) {
        const refreshData: AuthResponse = await refreshRes.json();
        setTokens(refreshData.accessToken, refreshData.refreshToken);
        headers['Authorization'] = `Bearer ${refreshData.accessToken}`;
        
        // Retry original request
        response = await fetch(`${API_BASE_URL}${endpoint}`, {
          ...options,
          headers,
        });
      } else {
        clearTokens();
      }
    } catch {
      clearTokens();
    }
  }

  return response;
}

// API Service Methods
export const api = {
  register: async (data: RegisterPayload) => {
    const res = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Đăng ký thất bại');
    return json;
  },

  login: async (data: LoginPayload): Promise<AuthResponse> => {
    const res = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Đăng nhập thất bại');
    setTokens(json.accessToken, json.refreshToken);
    return json;
  },

  getProfile: async (): Promise<UserProfile> => {
    const res = await fetchWithAuth('/api/users/me');
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể lấy thông tin người dùng');
    return json;
  },

  updateProfile: async (data: UpdateProfilePayload): Promise<UserProfile> => {
    const res = await fetchWithAuth('/api/users/me', {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Cập nhật hồ sơ thất bại');
    return json;
  },

  changePassword: async (data: ChangePasswordPayload) => {
    const res = await fetchWithAuth('/api/users/me/password', {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Đổi mật khẩu thất bại');
    return json;
  },

  refreshToken: async (): Promise<AuthResponse> => {
    const refreshToken = getRefreshToken();
    if (!refreshToken) throw new Error('Không tìm thấy Refresh Token');
    const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken }),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Refresh Token không hợp lệ');
    setTokens(json.accessToken, json.refreshToken);
    return json;
  },

  submitAudit: async (data: SubmitAuditPayload): Promise<SubmitAuditResponse> => {
    const res = await fetchWithAuth('/api/audits/submit', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể gửi yêu cầu Audit');
    return json;
  },

  getAuditById: async (id: string): Promise<AuditDetail> => {
    const res = await fetchWithAuth(`/api/audits/${id}`);
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không tìm thấy kết quả Audit');
    return json;
  },

  getMyAudits: async (page = 1, pageSize = 10): Promise<PagedResult<AuditSummary>> => {
    const res = await fetchWithAuth(`/api/audits/my?page=${page}&pageSize=${pageSize}`);
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể tải lịch sử Audit');
    return json;
  },

  // Projects Endpoints
  getProjects: async (): Promise<ProjectSummary[]> => {
    const res = await fetchWithAuth('/api/projects');
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể tải danh sách dự án');
    return json;
  },

  getProjectById: async (id: string): Promise<ProjectDetail> => {
    const res = await fetchWithAuth(`/api/projects/${id}`);
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể lấy thông tin dự án');
    return json;
  },

  createProject: async (data: CreateProjectPayload): Promise<ProjectSummary> => {
    const res = await fetchWithAuth('/api/projects', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể tạo dự án');
    return json;
  },

  deleteProject: async (id: string): Promise<void> => {
    const res = await fetchWithAuth(`/api/projects/${id}`, {
      method: 'DELETE',
    });
    if (!res.ok) {
      const json = await res.json().catch(() => ({}));
      throw new Error(json.message || json.detail || 'Không thể xóa dự án');
    }
  },

  // Websites Endpoints
  getWebsites: async (projectId?: string): Promise<WebsiteSummary[]> => {
    const query = projectId ? `?projectId=${projectId}` : '';
    const res = await fetchWithAuth(`/api/websites${query}`);
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể tải danh sách website');
    return json;
  },

  getWebsiteById: async (id: string): Promise<WebsiteDetail> => {
    const res = await fetchWithAuth(`/api/websites/${id}`);
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể lấy thông tin website');
    return json;
  },

  createWebsite: async (data: CreateWebsitePayload): Promise<WebsiteSummary> => {
    const res = await fetchWithAuth('/api/websites', {
      method: 'POST',
      body: JSON.stringify(data),
    });
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể thêm website vào dự án');
    return json;
  },

  deleteWebsite: async (id: string): Promise<void> => {
    const res = await fetchWithAuth(`/api/websites/${id}`, {
      method: 'DELETE',
    });
    if (!res.ok) {
      const json = await res.json().catch(() => ({}));
      throw new Error(json.message || json.detail || 'Không thể xóa website');
    }
  },

  // Reports Endpoints
  getReportHistory: async (params: { websiteId?: string; url?: string; from?: string; to?: string }): Promise<ReportHistoryResponse> => {
    const queryParts: string[] = [];
    if (params.websiteId) queryParts.push(`websiteId=${encodeURIComponent(params.websiteId)}`);
    if (params.url) queryParts.push(`url=${encodeURIComponent(params.url)}`);
    if (params.from) queryParts.push(`from=${encodeURIComponent(params.from)}`);
    if (params.to) queryParts.push(`to=${encodeURIComponent(params.to)}`);
    const query = queryParts.length > 0 ? `?${queryParts.join('&')}` : '';

    const res = await fetchWithAuth(`/api/reports/history${query}`);
    const json = await res.json();
    if (!res.ok) throw new Error(json.message || json.detail || 'Không thể tải lịch sử điểm số');
    return json;
  },
};
