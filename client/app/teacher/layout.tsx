'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { getUser, logout } from '@/lib/auth';
import { User } from '@/types';
import { ToastProvider } from '@/components/Toast';
import {
  LayoutDashboard,
  FileText,
  LogOut,
  BookOpen,
  User as UserIcon,
  Menu,
  X,
} from 'lucide-react';

export default function TeacherLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const [user, setUser] = useState<User | null>(null);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  useEffect(() => {
    setUser(getUser());
  }, []);

  const navItems = [
    { label: 'Dashboard', href: '/teacher', icon: LayoutDashboard },
    { label: 'My Assignments', href: '/teacher/assignments', icon: FileText },
  ];

  return (
    <ToastProvider>
      <div className="flex min-h-screen bg-slate-950 text-slate-100">
        {/* Sidebar for Desktop */}
        <aside className="hidden lg:flex w-64 flex-col border-r border-white/10 bg-slate-900/90 p-5 space-y-6 shrink-0 backdrop-blur-xl">
          {/* Branding */}
          <div className="flex items-center gap-3 px-2">
            <div className="p-2.5 rounded-xl bg-purple-500/10 border border-purple-500/20 text-purple-400">
              <BookOpen className="h-6 w-6" />
            </div>
            <div>
              <h2 className="text-base font-extrabold text-white leading-tight">Teacher Portal</h2>
              <p className="text-[11px] font-mono text-purple-400">Educator Dashboard</p>
            </div>
          </div>

          {/* Nav Links */}
          <nav className="flex-1 space-y-1.5 pt-4">
            {navItems.map((item) => {
              const Icon = item.icon;
              const isActive = pathname === item.href || (item.href !== '/teacher' && pathname.startsWith(item.href));
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-semibold transition duration-150 ${
                    isActive
                      ? 'bg-purple-600/20 border border-purple-500/30 text-white shadow-lg shadow-purple-500/10'
                      : 'text-slate-400 hover:text-white hover:bg-white/5'
                  }`}
                >
                  <Icon className={`h-5 w-5 ${isActive ? 'text-purple-400' : 'text-slate-400'}`} />
                  <span>{item.label}</span>
                </Link>
              );
            })}
          </nav>

          {/* Logout Footer */}
          <div className="pt-4 border-t border-white/10">
            <button
              onClick={() => logout()}
              className="w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-semibold text-slate-400 hover:text-red-400 hover:bg-red-500/10 transition duration-150"
            >
              <LogOut className="h-5 w-5 text-red-400" />
              <span>Sign Out</span>
            </button>
          </div>
        </aside>

        {/* Main Content Area */}
        <div className="flex-1 flex flex-col min-w-0">
          {/* Top Bar */}
          <header className="sticky top-0 z-30 flex items-center justify-between border-b border-white/10 bg-slate-900/80 px-6 py-4 backdrop-blur-xl">
            <div className="flex items-center gap-3">
              <button
                onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                className="lg:hidden p-2 rounded-xl border border-white/10 text-slate-300 hover:bg-white/5"
              >
                {isMobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
              </button>
              <h1 className="text-lg font-bold text-white hidden sm:block">
                Assignment & Submission Management System
              </h1>
            </div>

            {/* Teacher Profile Info */}
            <div className="flex items-center gap-3">
              <div className="text-right hidden sm:block">
                <p className="text-sm font-bold text-white">{user?.fullName || 'Teacher'}</p>
                <p className="text-xs text-slate-400">{user?.email || 'teacher@school.com'}</p>
              </div>
              <div className="h-9 w-9 rounded-full bg-gradient-to-tr from-purple-500 to-indigo-600 flex items-center justify-center text-white font-bold text-sm shadow-md">
                <UserIcon className="h-5 w-5" />
              </div>
            </div>
          </header>

          {/* Mobile Dropdown Nav */}
          {isMobileMenuOpen && (
            <div className="lg:hidden border-b border-white/10 bg-slate-900 p-4 space-y-2">
              {navItems.map((item) => {
                const Icon = item.icon;
                const isActive = pathname === item.href || (item.href !== '/teacher' && pathname.startsWith(item.href));
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    onClick={() => setIsMobileMenuOpen(false)}
                    className={`flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-semibold ${
                      isActive ? 'bg-purple-600/20 text-white' : 'text-slate-400 hover:text-white'
                    }`}
                  >
                    <Icon className="h-5 w-5" />
                    <span>{item.label}</span>
                  </Link>
                );
              })}
              <button
                onClick={() => logout()}
                className="w-full flex items-center gap-3 px-3.5 py-2.5 rounded-xl text-sm font-semibold text-red-400 hover:bg-red-500/10"
              >
                <LogOut className="h-5 w-5" />
                <span>Sign Out</span>
              </button>
            </div>
          )}

          {/* Content Body */}
          <main className="flex-1 p-6 sm:p-8 overflow-y-auto">{children}</main>
        </div>
      </div>
    </ToastProvider>
  );
}
