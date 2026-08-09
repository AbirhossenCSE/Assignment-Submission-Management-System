'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import { getUser } from '@/lib/auth';
import { ApiResponse, Assignment, Submission, SubmissionStatus, User } from '@/types';
import {
  FileText,
  FileCheck,
  Clock,
  Award,
  ArrowRight,
  AlertTriangle,
  Loader2,
} from 'lucide-react';

export default function StudentOverviewPage() {
  const [user, setUser] = useState<User | null>(null);

  const [totalCount, setTotalCount] = useState<number | null>(null);
  const [submittedCount, setSubmittedCount] = useState<number | null>(null);
  const [pendingCount, setPendingCount] = useState<number | null>(null);
  const [gradedCount, setGradedCount] = useState<number | null>(null);

  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const currentUser = getUser();
    setUser(currentUser);

    async function fetchStudentMetrics() {
      if (!currentUser?.classId) {
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        const [assignRes, subRes] = await Promise.allSettled([
          api.get<ApiResponse<Assignment[]>>(`/assignments/class/${currentUser.classId}`),
          api.get<ApiResponse<Submission[]>>('/submissions/my'),
        ]);

        let availableAssignments: Assignment[] = [];
        let mySubmissions: Submission[] = [];

        if (assignRes.status === 'fulfilled' && assignRes.value.data.data) {
          availableAssignments = assignRes.value.data.data;
        }

        if (subRes.status === 'fulfilled' && subRes.value.data.data) {
          mySubmissions = subRes.value.data.data;
        }

        const submittedAssignmentIds = new Set(mySubmissions.map((s) => s.assignmentId));

        setTotalCount(availableAssignments.length);
        setSubmittedCount(mySubmissions.length);
        
        const pending = availableAssignments.filter((a) => !submittedAssignmentIds.has(a.id)).length;
        setPendingCount(pending);

        const graded = mySubmissions.filter(
          (s) => s.status === SubmissionStatus.Graded || (s.status as any) === 4 || s.marks !== null
        ).length;
        setGradedCount(graded);
      } catch (err) {
        console.error('Failed to fetch student overview metrics:', err);
      } finally {
        setIsLoading(false);
      }
    }

    fetchStudentMetrics();
  }, []);

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      {/* Overview Title */}
      <div>
        <h1 className="text-2xl font-extrabold text-white">Student Portal Overview</h1>
        <p className="text-sm text-slate-400">
          Welcome back, <span className="font-semibold text-slate-200">{user?.fullName || 'Student'}</span>! Track your class assignments, submissions, and grades.
        </p>
      </div>

      {/* No Class Assigned Warning Banner */}
      {!user?.classId && !isLoading && (
        <div className="flex items-start gap-4 p-5 rounded-2xl border border-amber-500/20 bg-amber-500/10 text-amber-300">
          <AlertTriangle className="h-6 w-6 shrink-0 text-amber-400 mt-0.5" />
          <div className="space-y-1">
            <h3 className="font-bold text-base text-amber-200">No Class Assigned</h3>
            <p className="text-xs text-amber-300/90 leading-relaxed">
              Your account is currently not assigned to an active class. Please contact your system administrator or teacher to assign you to a class so you can access your coursework.
            </p>
          </div>
        </div>
      )}

      {/* Summary Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Available Assignments Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Available</span>
            <div className="p-2.5 rounded-xl bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
              <FileText className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-emerald-400" /> : totalCount ?? 0}
          </div>
          <Link
            href="/student/assignments"
            className="inline-flex items-center gap-1.5 text-xs font-semibold text-emerald-400 hover:text-emerald-300 transition"
          >
            <span>View Class Assignments</span>
            <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        {/* Submitted Count Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Submitted</span>
            <div className="p-2.5 rounded-xl bg-blue-500/10 text-blue-400 border border-blue-500/20">
              <FileCheck className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-blue-400" /> : submittedCount ?? 0}
          </div>
          <Link
            href="/student/submissions"
            className="inline-flex items-center gap-1.5 text-xs font-semibold text-blue-400 hover:text-blue-300 transition"
          >
            <span>Submission History</span>
            <ArrowRight className="h-3.5 w-3.5" />
          </Link>
        </div>

        {/* Pending / Unsubmitted Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Pending</span>
            <div className="p-2.5 rounded-xl bg-amber-500/10 text-amber-400 border border-amber-500/20">
              <Clock className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-amber-400" /> : pendingCount ?? 0}
          </div>
          <p className="text-xs text-slate-400">Requires completion</p>
        </div>

        {/* Graded Count Card */}
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 p-6 backdrop-blur-xl space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold uppercase tracking-wider text-slate-400">Graded</span>
            <div className="p-2.5 rounded-xl bg-purple-500/10 text-purple-400 border border-purple-500/20">
              <Award className="h-5 w-5" />
            </div>
          </div>
          <div className="text-3xl font-extrabold text-white">
            {isLoading ? <Loader2 className="h-6 w-6 animate-spin text-purple-400" /> : gradedCount ?? 0}
          </div>
          <p className="text-xs text-slate-400">Feedback available</p>
        </div>
      </div>
    </div>
  );
}
