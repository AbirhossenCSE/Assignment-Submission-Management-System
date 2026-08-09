'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';
import { saveToken, saveUser } from '@/lib/auth';
import { ApiResponse, AuthResponse, getRoleName, User } from '@/types';
import { LogIn, Mail, Lock, AlertCircle, Loader2 } from 'lucide-react';

export default function LoginPage() {
  const router = useRouter();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const [emailError, setEmailError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [apiError, setApiError] = useState('');

  const [isLoading, setIsLoading] = useState(false);

  const validateForm = (): boolean => {
    let isValid = true;
    setEmailError('');
    setPasswordError('');
    setApiError('');

    if (!email.trim()) {
      setEmailError('Email address is required.');
      isValid = false;
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      setEmailError('Please enter a valid email address.');
      isValid = false;
    }

    if (!password) {
      setPasswordError('Password is required.');
      isValid = false;
    }

    return isValid;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;

    setIsLoading(true);
    setApiError('');

    try {
      const response = await api.post<ApiResponse<AuthResponse>>('/auth/login', {
        email: email.trim(),
        password: password,
      });

      const data = response.data.data;
      if (data && data.token) {
        saveToken(data.token);

        const user: User = {
          id: data.userId,
          fullName: data.fullName,
          email: data.email,
          role: data.role,
          classId: data.classId,
        };

        saveUser(user);

        const roleName = getRoleName(data.role);
        if (roleName === 'Admin') {
          router.push('/admin');
        } else if (roleName === 'Teacher') {
          router.push('/teacher');
        } else {
          router.push('/student');
        }
      } else {
        setApiError('Login failed. Please check your credentials.');
      }
    } catch (err: any) {
      if (err.response && err.response.data) {
        const errorData = err.response.data as ApiResponse<unknown>;
        const msg = errorData.message || (errorData.errors && errorData.errors[0]) || 'Invalid email or password.';
        setApiError(msg);
      } else {
        setApiError('Network error. Unable to connect to the backend API server.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-950 p-4 sm:p-6 text-slate-100">
      <div className="w-full max-w-md space-y-6">
        {/* Header Branding */}
        <div className="text-center space-y-2">
          <div className="inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-indigo-500/10 border border-indigo-500/20 text-indigo-400 shadow-xl mb-2">
            <LogIn className="h-7 w-7" />
          </div>
          <h1 className="text-2xl sm:text-3xl font-bold tracking-tight text-white">
            Welcome Back
          </h1>
          <p className="text-sm text-slate-400">
            Sign in to access your Assignment Portal
          </p>
        </div>

        {/* Form Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/80 p-6 sm:p-8 backdrop-blur-xl shadow-2xl space-y-5">
          {apiError && (
            <div className="flex items-start gap-3 rounded-xl border border-red-500/20 bg-red-500/10 p-3.5 text-sm text-red-300 animate-fade-in">
              <AlertCircle className="h-5 w-5 shrink-0 text-red-400 mt-0.5" />
              <span>{apiError}</span>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4" noValidate>
            {/* Email Field */}
            <div className="space-y-1.5">
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300">
                Email Address
              </label>
              <div className="relative">
                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3 text-slate-500">
                  <Mail className="h-4 w-4" />
                </div>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="admin@school.com"
                  disabled={isLoading}
                  className={`w-full rounded-xl border bg-slate-950/60 py-2.5 pl-10 pr-3 text-sm text-white placeholder-slate-500 outline-none transition duration-200 focus:ring-2 ${
                    emailError
                      ? 'border-red-500/50 focus:border-red-500 focus:ring-red-500/20'
                      : 'border-white/10 focus:border-indigo-500 focus:ring-indigo-500/20'
                  }`}
                />
              </div>
              {emailError && (
                <p className="text-xs text-red-400 pl-1">{emailError}</p>
              )}
            </div>

            {/* Password Field */}
            <div className="space-y-1.5">
              <label className="block text-xs font-semibold uppercase tracking-wider text-slate-300">
                Password
              </label>
              <div className="relative">
                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3 text-slate-500">
                  <Lock className="h-4 w-4" />
                </div>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  disabled={isLoading}
                  className={`w-full rounded-xl border bg-slate-950/60 py-2.5 pl-10 pr-3 text-sm text-white placeholder-slate-500 outline-none transition duration-200 focus:ring-2 ${
                    passwordError
                      ? 'border-red-500/50 focus:border-red-500 focus:ring-red-500/20'
                      : 'border-white/10 focus:border-indigo-500 focus:ring-indigo-500/20'
                  }`}
                />
              </div>
              {passwordError && (
                <p className="text-xs text-red-400 pl-1">{passwordError}</p>
              )}
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full mt-2 inline-flex items-center justify-center rounded-xl bg-gradient-to-r from-indigo-600 to-violet-600 py-3 text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 transition duration-200 hover:from-indigo-500 hover:to-violet-500 focus:outline-none focus:ring-2 focus:ring-indigo-500/40 active:scale-[0.99] disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {isLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  <span>Logging in...</span>
                </>
              ) : (
                <span>Sign In</span>
              )}
            </button>
          </form>

          {/* Quick Demo Credentials Footer */}
          <div className="pt-3 border-t border-white/10 text-xs text-slate-400 space-y-1 text-center">
            <p className="font-semibold text-slate-300 mb-1">Demo Accounts:</p>
            <p><span className="text-indigo-400">Admin:</span> admin@school.com / Admin@123</p>
            <p><span className="text-indigo-400">Teacher:</span> teacher1@school.com / Teacher@123</p>
            <p><span className="text-indigo-400">Student:</span> student1@school.com / Student@123</p>
          </div>
        </div>
      </div>
    </main>
  );
}
