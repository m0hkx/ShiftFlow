export interface RegisterRequest {
  organizationName: string;
  username: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// The refresh token is not in the body, the server sets it as an HttpOnly cookie
export interface AuthResponse {
  token: string;
  username: string;
  role: string;
}

export interface CurrentUser {
  username: string;
  role: string;
}
