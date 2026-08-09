import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  const token = request.cookies.get('token')?.value;
  const rawRole = request.cookies.get('role')?.value;

  const role = normalizeRole(rawRole);

  const isProtectedPath =
    pathname.startsWith('/admin') ||
    pathname.startsWith('/teacher') ||
    pathname.startsWith('/student');

  const isAuthPath = pathname.startsWith('/login');

  // 1. Unauthenticated user trying to access protected route
  if (isProtectedPath && !token) {
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('callbackUrl', pathname);
    return NextResponse.redirect(loginUrl);
  }

  // 2. Authenticated user trying to access /login
  if (isAuthPath && token && role) {
    const dashboardUrl = new URL(getRoleDashboardPath(role), request.url);
    return NextResponse.redirect(dashboardUrl);
  }

  // 3. Role-based path protection for authenticated users
  if (token && role && isProtectedPath) {
    if (pathname.startsWith('/admin') && role !== 'Admin') {
      return NextResponse.redirect(new URL(getRoleDashboardPath(role), request.url));
    }
    if (pathname.startsWith('/teacher') && role !== 'Teacher') {
      return NextResponse.redirect(new URL(getRoleDashboardPath(role), request.url));
    }
    if (pathname.startsWith('/student') && role !== 'Student') {
      return NextResponse.redirect(new URL(getRoleDashboardPath(role), request.url));
    }
  }

  return NextResponse.next();
}

function normalizeRole(rawRole?: string): 'Admin' | 'Teacher' | 'Student' | null {
  if (!rawRole) return null;
  const r = rawRole.toLowerCase();
  if (r === 'admin' || r === '1') return 'Admin';
  if (r === 'teacher' || r === '2') return 'Teacher';
  if (r === 'student' || r === '3') return 'Student';
  return null;
}

function getRoleDashboardPath(role: 'Admin' | 'Teacher' | 'Student' | null): string {
  if (role === 'Admin') return '/admin';
  if (role === 'Teacher') return '/teacher';
  if (role === 'Student') return '/student';
  return '/login';
}

export const config = {
  matcher: ['/admin/:path*', '/teacher/:path*', '/student/:path*', '/login'],
};
