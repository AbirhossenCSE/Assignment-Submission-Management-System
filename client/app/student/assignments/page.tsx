'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import { getUser } from '@/lib/auth';
import { ApiResponse, Assignment, Submission, SubmissionStatus, User } from '@/types';
import { useToast } from '@/components/Toast';
import Button from '@/components/Button';
import {
  FileText,
  Clock,
  CheckCircle2,
  AlertTriangle,
  Eye,
  Loader2,
  FileCheck,
} from 'lucide-react';

export default function StudentAssignmentsPage() {
  const { showToast } = useToast();
  const [user, setUser] = useState<User | null>(null);

  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [submissionsMap, setSubmissionsMap] = useState<Record<string, Submission>>({});
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const currentUser = getUser();
    setUser(currentUser);

    async function fetchAssignmentsData() {
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

        if (assignRes.status === 'fulfilled' && assignRes.value.data.data) {
          setAssignments(assignRes.value.data.data);
        }

        if (subRes.status === 'fulfilled' && subRes.value.data.data) {
          const map: Record<string, Submission> = {};
          subRes.value.data.data.forEach((sub) => {
            map[sub.assignmentId] = sub;
          });
          setSubmissionsMap(map);
        }
      } catch (err: any) {
        const msg = err.response?.data?.message || 'Failed to fetch class assignments.';
        showToast(msg, 'error');
      } finally {
        setIsLoading(false);
      }
    }

    fetchAssignmentsData();
  }, []);

  const formatDeadline = (isoString: string) => {
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

  const isPastDeadline = (isoString: string) => {
    const d = new Date(isoString);
    return !isNaN(d.getTime()) && d < new Date();
  };

  const renderStatusBadge = (assignmentId: string) => {
    const sub = submissionsMap[assignmentId];

    if (!sub) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-500/10 border border-slate-500/20 text-slate-400">
          Not Submitted
        </span>
      );
    }

    const s = Number(sub.status);

    if (s === SubmissionStatus.Graded || s === 4) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 border border-emerald-500/20 text-emerald-400">
          <CheckCircle2 className="h-3 w-3" />
          Graded ({sub.marks} pts)
        </span>
      );
    }

    if (sub.isLate || s === SubmissionStatus.Late || s === 3) {
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
        <h1 className="text-2xl font-extrabold text-white">Class Coursework</h1>
        <p className="text-sm text-slate-400">Assignments and deadlines published for your class section.</p>
      </div>

      {/* No Class Assigned Warning */}
      {!user?.classId && !isLoading ? (
        <div className="flex items-start gap-4 p-6 rounded-2xl border border-amber-500/20 bg-amber-500/10 text-amber-300">
          <AlertTriangle className="h-6 w-6 shrink-0 text-amber-400 mt-0.5" />
          <div className="space-y-1">
            <h3 className="font-bold text-base text-amber-200">You are not assigned to a class yet</h3>
            <p className="text-xs text-amber-300/90 leading-relaxed">
              Please contact your school administrator or teacher to assign you to a class section. Coursework assignments will appear here once assigned.
            </p>
          </div>
        </div>
      ) : (
        /* Assignments Table Card */
        <div className="rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl overflow-hidden shadow-2xl">
          {isLoading ? (
            <div className="p-12 flex flex-col items-center justify-center text-slate-400 gap-3">
              <Loader2 className="h-8 w-8 animate-spin text-emerald-500" />
              <p className="text-sm">Loading class assignments...</p>
            </div>
          ) : assignments.length === 0 ? (
            <div className="p-12 text-center text-slate-400 space-y-3">
              <FileText className="h-12 w-12 mx-auto text-slate-600" />
              <p className="text-base font-semibold text-slate-300">No published assignments for your class.</p>
              <p className="text-xs text-slate-500">Your teachers have not published any coursework for your class section yet.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left text-sm text-slate-300">
                <thead className="border-b border-white/10 bg-slate-950/60 text-xs font-semibold uppercase tracking-wider text-slate-400">
                  <tr>
                    <th className="py-3.5 px-6">Assignment Title</th>
                    <th className="py-3.5 px-6">Subject</th>
                    <th className="py-3.5 px-6">Deadline</th>
                    <th className="py-3.5 px-6">Max Marks</th>
                    <th className="py-3.5 px-6">Submission Status</th>
                    <th className="py-3.5 px-6 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/5">
                  {assignments.map((assignment) => {
                    const past = isPastDeadline(assignment.deadline);

                    return (
                      <tr key={assignment.id} className="hover:bg-white/5 transition duration-150">
                        <td className="py-4 px-6 font-bold text-white max-w-xs truncate">
                          <Link
                            href={`/student/assignments/${assignment.id}`}
                            className="hover:text-emerald-400 transition"
                          >
                            {assignment.title}
                          </Link>
                        </td>
                        <td className="py-4 px-6 text-slate-300 font-semibold">{assignment.subjectName}</td>
                        <td className="py-4 px-6">
                          <div className="flex items-center gap-1.5">
                            <Clock className={`h-3.5 w-3.5 ${past ? 'text-red-400' : 'text-slate-400'}`} />
                            <span className={past ? 'text-red-400 font-semibold' : 'text-slate-300'}>
                              {formatDeadline(assignment.deadline)}
                            </span>
                          </div>
                          {past && (
                            <span className="inline-block mt-1 text-[10px] uppercase font-bold text-red-400 bg-red-500/10 px-2 py-0.5 rounded border border-red-500/20">
                              Deadline Passed
                            </span>
                          )}
                        </td>
                        <td className="py-4 px-6 font-mono text-emerald-300 font-semibold">
                          {assignment.maxMarks} pts
                        </td>
                        <td className="py-4 px-6">{renderStatusBadge(assignment.id)}</td>
                        <td className="py-4 px-6 text-right">
                          <Link href={`/student/assignments/${assignment.id}`}>
                            <Button variant="secondary" size="sm" icon={<Eye className="h-3.5 w-3.5" />}>
                              View & Submit
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
      )}
    </div>
  );
}
