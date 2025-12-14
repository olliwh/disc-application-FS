import ApiClient from "./api-client";

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAt: string;
}

const apiClient = new ApiClient<LoginResponse>("/Auth");

const loginService = {
  login: async (username: string, password: string): Promise<LoginResponse> => {
    return apiClient.post<LoginResponse>("/login", { username, password });
  },

  /**
   * Save a token to localStorage.
   * @param token - The JWT token string.
   */
  saveToken: (token: string) => {
    localStorage.setItem("token", token);
  },

  getToken: () => localStorage.getItem("token"),

  removeToken: () => {
    localStorage.removeItem("token");
  },

  getEmployeeIdFromToken: (): number | null => {
    const token = loginService.getToken();
    if (!token) return null;
    try {
      const [, payloadBase64] = token.split(".");
      const payload = JSON.parse(atob(payloadBase64));
      return payload.employeeId
        ? Number.parseInt(payload.employeeId, 10)
        : null;
    } catch {
      return null;
    }
  },

  getRoleFromToken: (): string | null => {
    const token = loginService.getToken();
    if (!token) return null;
    try {
      const [, payloadBase64] = token.split(".");
      const payload = JSON.parse(atob(payloadBase64));
      return payload.role || null;
    } catch {
      return null;
    }
  },
};

export default loginService;
