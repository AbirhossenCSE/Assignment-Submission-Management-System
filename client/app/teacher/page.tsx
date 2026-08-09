'use client';

import React, { useEffect, useState } from 'react';
import { getUser, logout } from '@/lib/auth';
import { User, getRoleName } from '@/types';
import { BookOpen, LogOut, UserCheck } from 'lucide-react';

export default function TeacherDashboardPage() {
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    setUser(getUser());
  }, []);

  return (
    <main className="min-h-screen bg-slate-950 text-slate-100 p-6 sm:p-10">
      <div className="max-w-4xl mx-auto space-y-8">
        {/* Top Header */}
        <header className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 border-b border-white/10">
          <div className="flex items-center gap-3">
            <div className="p-3 rounded-2xl bg-purple-500/10 border border-purple-500/20 text-purple-400">
              <BookOpen className="h-8 w-8" />
            </div>
            <div>
              <h1 className="text-2xl font-extrabold text-white">Teacher Dashboard</h1>
              <p className="text-xs text-slate-400">Assignment Management & Grading Portal</p>
            </div>
          </div>

          <button
            onClick={() => logout()}
            className="inline-flex items-center gap-2 px-4 py-2.5 rounded-xl bg-slate-900 border border-white/10 text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-800 transition duration-200"
          >
            <LogOut className="h-4 w-4 text-red-400" />
            <span>Sign Out</span>
          </button>
        </header>

        {/* Status Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 sm:p-8 backdrop-blur-xl space-y-4">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-xs font-semibold">
            <UserCheck className="h-3.5 w-3.5" />
            <span>Authenticated as {user ? getRoleName(user.role) : 'Teacher'}</span>
          </div>

          <h2 className="text-xl font-bold text-white">Teacher Dashboard - Coming Soon</h2>
          <p className="text-sm text-slate-400 leading-relaxed">
            Welcome back, <span className="font-semibold text-slate-200">{user?.fullName || 'Teacher'}</span> ({user?.email})! Creating assignments, reviewing student submissions, and grading tools will be available here.
          </p>
        </div>
      </div>
    </main>
  );
}
