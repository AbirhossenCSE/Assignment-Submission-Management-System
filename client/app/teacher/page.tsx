'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import { ApiResponse, Assignment, AssignmentStatus } from '@/types';
import { FileText, FileEdit, CheckCircle2, ArrowRight, Loader2 } from 'lucide-react';

export default function TeacherOverviewPage() {
  const [totalCount, setTotalCount] = useState<number | null>(null);
  const [draftCount, setDraftCount] = useState<number | null>(null);
  const [publishedCount, setPublishedCount] = useState<number | null>(null);

  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchMetrics() {
      try {
        setIsLoading(true);
        const res = await api.get<ApiResponse<Assignment[]>>('/assignments/my');
        if (res.data && res.data.data) {
          const list = res.data.data;
          setTotalCount(list.length);
          setDraftCount(list.filter((a) => a.status === AssignmentStatus.Draft || a.status === 1).length);
          setPublishedCount(list.filter((a) => a.status === AssignmentStatus.Published || a.status === 2).length);
        }
      } catch (err) {
        console.error('Failed to fetch teacher assignment metrics:', err);
      } finally {
        setIsLoading(false);
      }
    }

    fetchMetrics();
  }, []);

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      {/* Overview Title */}
      <div>
        <h1 className="text-2xl font-extrabold text-white">Teacher Overview</h1>
        <p className="text-sm text-slate-400">Track and manage your published assignments and drafts.</p>
      </div>

      {/* Summary Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-6">
        {/* Total Assignments Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Total Assignments</span>
            <div className="p-2.5 rounded-xl bg-purple-500/10 text-purple-400 border border-purple-500/20">
              <FileText className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-purple-400" /> : totalCount ?? 0}
          </div>
          <Link
            href="/teacher/assignments"
            className="inline-flex items-center gap-1.5 text-xs font-semibold text-purple-400 hover:text-purple-300 transition"
          >
            <span>View All Assignments</span>
            <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        {/* Draft Count Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Draft Assignments</span>
            <div className="p-2.5 rounded-xl bg-amber-500/10 text-amber-400 border border-amber-500/20">
              <FileEdit className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-amber-400" /> : draftCount ?? 0}
          </div>
          <p className="text-xs text-slate-400">Ready to edit and publish</p>
        </div>

        {/* Published Count Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Published Assignments</span>
            <div className="p-2.5 rounded-xl bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
              <CheckCircle2 className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-emerald-400" /> : publishedCount ?? 0}
          </div>
          <p className="text-xs text-slate-400">Active and visible to students</p>
        </div>
      </div>
    </div>
  );
}
