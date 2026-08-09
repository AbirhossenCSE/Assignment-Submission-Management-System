import { User, getRoleName } from '@/types';

const TOKEN_KEY = 'auth_token';
const USER_KEY = 'auth_user';

export function saveToken(token: string): void {
  if (typeof window !== 'undefined') {
    localStorage.setItem(TOKEN_KEY, token);
    document.cookie = `token=${encodeURIComponent(token)}; path=/; max-age=86400; SameSite=Lax`;
  }
}

export function getToken(): string | null {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem(TOKEN_KEY);
    if (token) return token;
    
    // Fallback to cookie if localStorage is empty
    const match = document.cookie.match(new RegExp('(^| )token=([^;]+)'));
    if (match) return decodeURIComponent(match[2]);
  }
  return null;
}

export function removeToken(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem(TOKEN_KEY);
    document.cookie = 'token=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
  }
}

export function saveUser(user: User): void {
  if (typeof window !== 'undefined') {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    const roleName = getRoleName(user.role);
    document.cookie = `role=${encodeURIComponent(roleName)}; path=/; max-age=86400; SameSite=Lax`;
  }
}

export function getUser(): User | null {
  if (typeof window !== 'undefined') {
    const data = localStorage.getItem(USER_KEY);
    if (!data) return null;
    try {
      return JSON.parse(data) as User;
    } catch {
      return null;
    }
  }
  return null;
}

export function removeUser(): void {
  if (typeof window !== 'undefined') {
    localStorage.removeItem(USER_KEY);
    document.cookie = 'role=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
  }
}

export function isAuthenticated(): boolean {
  const token = getToken();
  return Boolean(token && token.trim().length > 0);
}

export function logout(): void {
  removeToken();
  removeUser();
  if (typeof window !== 'undefined') {
    window.location.href = '/login';
  }
}
