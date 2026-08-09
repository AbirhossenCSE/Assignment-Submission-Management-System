'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { api } from '@/lib/api';
import { ApiResponse, Assignment, AssignmentStatus, Submission, SubmissionStatus } from '@/types';
import Button from '@/components/Button';
import { useToast } from '@/components/Toast';
import {
  ArrowLeft,
  Clock,
  Award,
  School,
  BookOpen,
  FileCheck,
  UserCheck,
  CheckCircle2,
  Loader2,
} from 'lucide-react';

export default function AssignmentDetailPage() {
  const params = useParams();
  const id = params?.id as string;
  const { showToast } = useToast();

  const [assignment, setAssignment] = useState<Assignment | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function fetchDetail() {
      if (!id) return;
      try {
        setIsLoading(true);
        const [assignRes, subRes] = await Promise.allSettled([
          api.get<ApiResponse<Assignment>>(`/assignments/${id}`),
          api.get<ApiResponse<Submission[]>>(`/assignments/${id}/submissions`),
        ]);

        if (assignRes.status === 'fulfilled' && assignRes.value.data.data) {
          setAssignment(assignRes.value.data.data);
        }

        if (subRes.status === 'fulfilled' && subRes.value.data.data) {
          setSubmissions(subRes.value.data.data);
        }
      } catch (err: any) {
        const msg = err.response?.data?.message || 'Failed to fetch assignment details.';
        showToast(msg, 'error');
      } finally {
        setIsLoading(false);
      }
    }

    fetchDetail();
  }, [id]);

  if (isLoading) {
    return (
      <div className="p-16 flex flex-col items-center justify-center text-slate-400 gap-3">
        <Loader2 className="h-8 w-8 animate-spin text-purple-500" />
        <p className="text-sm">Loading assignment details...</p>
      </div>
    );
  }

  if (!assignment) {
    return (
      <div className="max-w-3xl mx-auto p-8 rounded-2xl border border-white/10 bg-slate-900 text-center space-y-4">
        <p className="text-lg font-bold text-white">Assignment Not Found</p>
        <p className="text-sm text-slate-400">The requested assignment ID could not be found or has been deleted.</p>
        <Link href="/teacher/assignments">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />}>
            Back to Assignments
          </Button>
        </Link>
      </div>
    );
  }

  const isDraft = assignment.status === AssignmentStatus.Draft || (assignment.status as any) === 1;
  const isPast = new Date(assignment.deadline) < new Date();

  const totalSubmissions = submissions.length;
  const gradedCount = submissions.filter(
    (s) => s.status === SubmissionStatus.Graded || (s.status as any) === 4 || s.marks !== null
  ).length;

  const formattedDeadline = new Date(assignment.deadline).toLocaleString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      {/* Top Back Navigation Bar */}
      <div className="flex items-center justify-between">
        <Link
          href="/teacher/assignments"
          className="inline-flex items-center gap-2 text-xs font-semibold text-slate-400 hover:text-white transition"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back to Assignments</span>
        </Link>

        <Link href={`/teacher/assignments/${id}/submissions`}>
          <Button icon={<FileCheck className="h-4 w-4" />}>
            View Submissions ({totalSubmissions})
          </Button>
        </Link>
      </div>

      {/* Main Assignment Detail Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/80 p-6 sm:p-8 backdrop-blur-xl shadow-2xl space-y-6">
        {/* Title & Status Header */}
        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 pb-6 border-b border-white/10">
          <div className="space-y-2">
            <div className="flex items-center gap-2">
              <span
                className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${
                  isDraft
                    ? 'bg-amber-500/10 border-amber-500/20 text-amber-400'
                    : 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                }`}
              >
                {isDraft ? 'Draft' : 'Published'}
              </span>
              {isPast && (
                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border bg-red-500/10 border-red-500/20 text-red-400">
                  Deadline Passed
                </span>
              )}
            </div>
            <h1 className="text-2xl sm:text-3xl font-extrabold text-white">{assignment.title}</h1>
          </div>
        </div>

        {/* Metadata Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 p-4 rounded-xl border border-white/10 bg-slate-950/50 text-xs">
          <div className="space-y-1">
            <span className="flex items-center gap-1.5 text-slate-400 uppercase font-semibold tracking-wider">
              <BookOpen className="h-3.5 w-3.5 text-purple-400" />
              Subject
            </span>
            <p className="text-sm font-bold text-white">{assignment.subjectName}</p>
          </div>

          <div className="space-y-1">
            <span className="flex items-center gap-1.5 text-slate-400 uppercase font-semibold tracking-wider">
              <School className="h-3.5 w-3.5 text-indigo-400" />
              Class
            </span>
            <p className="text-sm font-bold text-white">{assignment.className}</p>
          </div>

          <div className="space-y-1">
            <span className="flex items-center gap-1.5 text-slate-400 uppercase font-semibold tracking-wider">
              <Clock className="h-3.5 w-3.5 text-amber-400" />
              Deadline
            </span>
            <p className={`text-sm font-bold ${isPast ? 'text-red-400' : 'text-white'}`}>
              {formattedDeadline}
            </p>
          </div>

          <div className="space-y-1">
            <span className="flex items-center gap-1.5 text-slate-400 uppercase font-semibold tracking-wider">
              <Award className="h-3.5 w-3.5 text-emerald-400" />
              Max Marks
            </span>
            <p className="text-sm font-bold text-emerald-400">{assignment.maxMarks} Points</p>
          </div>
        </div>

        {/* Submission Summary Banner */}
        <div className="p-4 rounded-xl border border-purple-500/20 bg-purple-500/10 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 text-xs">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-purple-500/20 text-purple-300">
              <UserCheck className="h-5 w-5" />
            </div>
            <div>
              <p className="font-bold text-white text-sm">Submission Status Summary</p>
              <p className="text-purple-200">
                {totalSubmissions === 0
                  ? 'No submissions received yet.'
                  : `${totalSubmissions} total submission${totalSubmissions === 1 ? '' : 's'} received (${gradedCount} graded).`}
              </p>
            </div>
          </div>
          <Link href={`/teacher/assignments/${id}/submissions`}>
            <Button size="sm" variant="secondary" icon={<CheckCircle2 className="h-3.5 w-3.5 text-emerald-400" />}>
              Review All Submissions
            </Button>
          </Link>
        </div>

        {/* Instructions & Description */}
        <div className="space-y-2">
          <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-400">
            Instructions & Content
          </h3>
          <div className="p-4 rounded-xl border border-white/10 bg-slate-950/40 text-sm text-slate-200 leading-relaxed whitespace-pre-line min-h-[100px]">
            {assignment.description}
          </div>
        </div>

        {/* Submission Rules */}
        <div className="pt-4 border-t border-white/10 flex items-center justify-between text-xs text-slate-400">
          <div>
            <span>Resubmission Policy: </span>
            <span className="font-semibold text-white">
              {assignment.allowResubmission ? 'Allowed before deadline' : 'Single submission only'}
            </span>
          </div>
          <div>
            <span>Created By: </span>
            <span className="font-semibold text-purple-300">{assignment.teacherName}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
