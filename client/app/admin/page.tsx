'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import { ApiResponse, ClassEntity, Subject, User } from '@/types';
import { School, BookOpen, Users, ArrowRight, Loader2 } from 'lucide-react';

export default function AdminOverviewPage() {
  const [classCount, setClassCount] = useState<number | null>(null);
  const [subjectCount, setSubjectCount] = useState<number | null>(null);
  const [teacherCount, setTeacherCount] = useState<number | null>(null);

  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchDashboardMetrics() {
      try {
        setIsLoading(true);

        const [classesRes, subjectsRes, teachersRes] = await Promise.allSettled([
          api.get<ApiResponse<ClassEntity[]>>('/classes'),
          api.get<ApiResponse<Subject[]>>('/subjects'),
          api.get<ApiResponse<User[]>>('/user?role=2'), // Role 2 = Teacher
        ]);

        if (classesRes.status === 'fulfilled' && classesRes.value.data.data) {
          setClassCount(classesRes.value.data.data.length);
        }

        if (subjectsRes.status === 'fulfilled' && subjectsRes.value.data.data) {
          setSubjectCount(subjectsRes.value.data.data.length);
        }

        if (teachersRes.status === 'fulfilled' && teachersRes.value.data.data) {
          setTeacherCount(teachersRes.value.data.data.length);
        }
      } catch (err) {
        console.error('Failed to fetch dashboard metrics:', err);
      } finally {
        setIsLoading(false);
      }
    }

    fetchDashboardMetrics();
  }, []);

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      {/* Overview Title */}
      <div>
        <h1 className="text-2xl font-extrabold text-white">System Overview</h1>
        <p className="text-sm text-slate-400">Quick statistics and operational shortcuts for portal administration.</p>
      </div>

      {/* Summary Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        {/* Classes Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Total Classes</span>
            <div className="p-2.5 rounded-xl bg-indigo-500/10 text-indigo-400 border border-indigo-500/20">
              <School className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-indigo-400" /> : classCount ?? 0}
          </div>
          <Link
            href="/admin/classes"
            className="inline-flex items-center gap-1.5 text-xs font-semibold text-indigo-400 hover:text-indigo-300 transition"
          >
            <span>Manage Classes</span>
            <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        {/* Subjects Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Total Subjects</span>
            <div className="p-2.5 rounded-xl bg-purple-500/10 text-purple-400 border border-purple-500/20">
              <BookOpen className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-purple-400" /> : subjectCount ?? 0}
          </div>
          <Link
            href="/admin/subjects"
            className="inline-flex items-center gap-1.5 text-xs font-semibold text-purple-400 hover:text-purple-300 transition"
          >
            <span>Manage Subjects</span>
            <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        {/* Teachers Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Active Teachers</span>
            <div className="p-2.5 rounded-xl bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
              <Users className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-emerald-400" /> : teacherCount ?? 0}
          </div>
          <p className="text-xs text-slate-400">Assigned across active subjects</p>
        </div>
      </div>
    </div>
  );
}
