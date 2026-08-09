'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import { ApiResponse, Submission, SubmissionStatus } from '@/types';
import Button from '@/components/Button';
import { useToast } from '@/components/Toast';
import {
  FileCheck,
  CheckCircle2,
  Clock,
  Eye,
  Loader2,
  Award,
} from 'lucide-react';

export default function StudentSubmissionsHistoryPage() {
  const { showToast } = useToast();

  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchMySubmissions() {
      try {
        setIsLoading(true);
        const res = await api.get<ApiResponse<Submission[]>>('/submissions/my');
        if (res.data && res.data.data) {
          setSubmissions(res.data.data);
        }
      } catch (err: any) {
        const msg = err.response?.data?.message || 'Failed to fetch submission history.';
        showToast(msg, 'error');
      } finally {
        setIsLoading(false);
      }
    }

    fetchMySubmissions();
  }, []);

  const formatTimestamp = (isoString?: string | null) => {
    if (!isoString) return 'N/A';
    const d = new Date(isoString);
    if (isNaN(d.getTime())) return isoString;
    return d.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      hour12: true,
    });
  };

  const renderStatusBadge = (status: SubmissionStatus | number, isLate: boolean) => {
    const s = Number(status);

    if (s === SubmissionStatus.Graded || s === 4) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">
          <CheckCircle2 className="h-3 w-3" />
          Graded
        </span>
      );
    }

    if (isLate || s === SubmissionStatus.Late || s === 3) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-500/10 border border-amber-500/20 text-amber-400">
          <Clock className="h-3 w-3" />
          Submitted Late
        </span>
      );
    }

    return (
      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-500/10 border border-blue-500/20 text-blue-400">
        <FileCheck className="h-3 w-3" />
        Submitted
      </span>
    );
  };

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header Bar */}
      <div>
        <h1 className="text-2xl font-extrabold text-white">My Submissions History</h1>
        <p className="text-sm text-slate-400">Complete record of your submitted coursework, grades, and teacher feedback.</p>
      </div>

      {/* Submissions Table Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-12 flex flex-col items-center justify-center text-slate-400 gap-3">
            <Loader2 className="h-8 w-8 animate-spin text-emerald-500" />
            <p className="text-sm">Loading submission history...</p>
          </div>
        ) : submissions.length === 0 ? (
          <div className="p-12 text-center text-slate-400 space-y-3">
            <FileCheck className="h-12 w-12 mx-auto text-slate-600" />
            <p className="text-base font-semibold text-slate-300">No submissions found.</p>
            <p className="text-xs text-slate-500">You have not submitted any coursework assignments yet.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-300">
              <thead className="border-b border-white/10 bg-slate-950/60 text-xs font-semibold uppercase tracking-wider text-slate-400">
                <tr>
                  <th className="py-3.5 px-6">Assignment Title</th>
                  <th className="py-3.5 px-6">Submitted At</th>
                  <th className="py-3.5 px-6">Status</th>
                  <th className="py-3.5 px-6">Grade / Marks</th>
                  <th className="py-3.5 px-6">Teacher Feedback</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {submissions.map((sub) => {
                  const isGraded = sub.marks !== null && sub.marks !== undefined;
                  return (
                    <tr key={sub.id} className="hover:bg-white/5 transition duration-150">
                      <td className="py-4 px-6 font-bold text-white max-w-xs truncate">
                        <Link
                          href={`/student/assignments/${sub.assignmentId}`}
                          className="hover:text-emerald-400 transition"
                        >
                          {sub.assignmentTitle}
                        </Link>
                      </td>
                      <td className="py-4 px-6 text-slate-300">
                        {formatTimestamp(sub.submittedAt)}
                      </td>
                      <td className="py-4 px-6">{renderStatusBadge(sub.status, sub.isLate)}</td>
                      <td className="py-4 px-6 font-mono">
                        {isGraded ? (
                          <span className="font-bold text-emerald-400 flex items-center gap-1">
                            <Award className="h-3.5 w-3.5" />
                            {sub.marks} / {sub.maxMarks} pts
                          </span>
                        ) : (
                          <span className="text-slate-500">-</span>
                        )}
                      </td>
                      <td className="py-4 px-6 max-w-xs truncate text-xs text-slate-400 italic">
                        {sub.feedback ? `"${sub.feedback}"` : 'No feedback yet'}
                      </td>
                      <td className="py-4 px-6 text-right">
                        <Link href={`/student/assignments/${sub.assignmentId}`}>
                          <Button variant="secondary" size="sm" icon={<Eye className="h-3.5 w-3.5" />}>
                            View Detail
                          </Button>
                        </Link>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
