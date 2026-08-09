'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { api } from '@/lib/api';
import { ApiResponse, Assignment, Submission, SubmissionStatus } from '@/types';
import Button from '@/components/Button';
import Textarea from '@/components/Textarea';
import Input from '@/components/Input';
import { useToast } from '@/components/Toast';
import {
  ArrowLeft,
  Clock,
  Award,
  School,
  BookOpen,
  CheckCircle2,
  AlertCircle,
  ExternalLink,
  Send,
  Loader2,
  Lock,
} from 'lucide-react';

export default function StudentAssignmentDetailPage() {
  const params = useParams();
  const assignmentId = params?.id as string;
  const { showToast } = useToast();

  const [assignment, setAssignment] = useState<Assignment | null>(null);
  const [existingSubmission, setExistingSubmission] = useState<Submission | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Form state
  const [answerText, setAnswerText] = useState('');
  const [attachmentUrl, setAttachmentUrl] = useState('');
  const [answerError, setAnswerError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const fetchAssignmentData = async () => {
    if (!assignmentId) return;
    try {
      setIsLoading(true);
      const [assignRes, subRes] = await Promise.allSettled([
        api.get<ApiResponse<Assignment>>(`/assignments/${assignmentId}`),
        api.get<ApiResponse<Submission[]>>('/submissions/my'),
      ]);

      if (assignRes.status === 'fulfilled' && assignRes.value.data.data) {
        setAssignment(assignRes.value.data.data);
      }

      if (subRes.status === 'fulfilled' && subRes.value.data.data) {
        const sub = subRes.value.data.data.find((s) => s.assignmentId === assignmentId);
        if (sub) {
          setExistingSubmission(sub);
          setAnswerText(sub.answerText || '');
          setAttachmentUrl(sub.attachmentUrl || '');
        }
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to fetch assignment detail.';
      showToast(msg, 'error');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchAssignmentData();
  }, [assignmentId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setAnswerError('');

    if (!answerText.trim()) {
      setAnswerError('Answer content is required.');
      return;
    }

    setIsSubmitting(true);

    try {
      if (existingSubmission) {
        // PUT /api/submissions/{id} - Resubmit
        await api.put(`/submissions/${existingSubmission.id}`, {
          answerText: answerText.trim(),
          attachmentUrl: attachmentUrl.trim() || null,
        });
        showToast('Submission updated successfully!');
      } else {
        // POST /api/assignments/{assignmentId}/submissions
        await api.post(`/assignments/${assignmentId}/submissions`, {
          answerText: answerText.trim(),
          attachmentUrl: attachmentUrl.trim() || null,
        });
        showToast('Assignment submitted successfully!');
      }

      fetchAssignmentData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to submit assignment.';
      showToast(msg, 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isLoading) {
    return (
      <div className="p-16 flex flex-col items-center justify-center text-slate-400 gap-3 max-w-4xl mx-auto">
        <Loader2 className="h-8 w-8 animate-spin text-emerald-500" />
        <p className="text-sm">Loading assignment and submission details...</p>
      </div>
    );
  }

  if (!assignment) {
    return (
      <div className="max-w-3xl mx-auto p-8 rounded-2xl border border-white/10 bg-slate-900 text-center space-y-4">
        <p className="text-lg font-bold text-white">Assignment Not Found</p>
        <p className="text-sm text-slate-400">The requested assignment ID could not be found.</p>
        <Link href="/student/assignments">
          <Button variant="secondary" icon={<ArrowLeft className="h-4 w-4" />}>
            Back to Coursework
          </Button>
        </Link>
      </div>
    );
  }

  const isPast = new Date(assignment.deadline) < new Date();
  const isGraded =
    existingSubmission &&
    (existingSubmission.status === SubmissionStatus.Graded ||
      (existingSubmission.status as any) === 4 ||
      existingSubmission.marks !== null);

  const canResubmit =
    existingSubmission &&
    !isGraded &&
    !isPast &&
    assignment.allowResubmission;

  const canSubmitNew = !existingSubmission && !isPast;

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
          href="/student/assignments"
          className="inline-flex items-center gap-2 text-xs font-semibold text-slate-400 hover:text-white transition"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back to Coursework</span>
        </Link>
      </div>

      {/* Main Assignment Detail Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/80 p-6 sm:p-8 backdrop-blur-xl shadow-2xl space-y-6">
        {/* Title & Status Header */}
        <div className="flex flex-col sm:flex-row sm:items-start justify-between gap-4 pb-6 border-b border-white/10">
          <div className="space-y-2">
            <div className="flex items-center gap-2">
              <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border bg-emerald-500/10 border-emerald-500/20 text-emerald-400">
                {assignment.subjectName}
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

        {/* Assignment Instructions */}
        <div className="space-y-2">
          <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-400">
            Instructions & Content
          </h3>
          <div className="p-4 rounded-xl border border-white/10 bg-slate-950/40 text-sm text-slate-200 leading-relaxed whitespace-pre-line min-h-[80px]">
            {assignment.description}
          </div>
        </div>

        {/* ---------------- SUBMISSION WORKFLOW SECTIONS ---------------- */}

        {/* SCENARIO 1: Already Graded - Display Read-Only Grade & Teacher Feedback */}
        {isGraded && (
          <div className="p-6 rounded-2xl border border-emerald-500/30 bg-emerald-500/5 space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-emerald-400 font-bold text-base">
                <CheckCircle2 className="h-5 w-5" />
                <span>Assignment Graded & Evaluated</span>
              </div>
              <div className="px-3.5 py-1.5 rounded-xl bg-emerald-500/20 border border-emerald-500/30 font-mono font-extrabold text-emerald-300 text-sm">
                Score: {existingSubmission.marks} / {assignment.maxMarks} pts
              </div>
            </div>

            {existingSubmission.feedback && (
              <div className="space-y-1.5">
                <span className="text-xs font-semibold uppercase tracking-wider text-emerald-300">
                  Teacher Feedback
                </span>
                <div className="p-3.5 rounded-xl border border-emerald-500/20 bg-slate-950/60 text-sm text-emerald-200 font-sans">
                  "{existingSubmission.feedback}"
                </div>
              </div>
            )}

            <div className="space-y-2 pt-2 border-t border-emerald-500/20 text-xs">
              <span className="text-slate-400 font-semibold">Submitted Answer:</span>
              <p className="p-3 rounded-lg bg-slate-950/40 text-slate-300 font-mono whitespace-pre-line">
                {existingSubmission.answerText}
              </p>
            </div>
          </div>
        )}

        {/* SCENARIO 2: Unsubmitted & Deadline Passed */}
        {!existingSubmission && isPast && (
          <div className="p-5 rounded-2xl border border-red-500/20 bg-red-500/10 text-red-300 flex items-start gap-3">
            <AlertCircle className="h-5 w-5 text-red-400 shrink-0 mt-0.5" />
            <div className="space-y-1 text-xs">
              <p className="font-bold text-sm text-red-200">Deadline Has Passed</p>
              <p className="text-red-300/90 leading-relaxed">
                The deadline for this assignment was {formattedDeadline}. Submissions are no longer accepted for this task.
              </p>
            </div>
          </div>
        )}

        {/* SCENARIO 3: Submitted, Not Graded, Resubmission Locked or Deadline Passed */}
        {existingSubmission && !isGraded && !canResubmit && (
          <div className="p-6 rounded-2xl border border-white/10 bg-slate-950/60 space-y-4">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-blue-400 font-bold text-sm">
                <CheckCircle2 className="h-5 w-5" />
                <span>Submission Turned In</span>
              </div>
              <div className="flex items-center gap-1.5 text-xs text-slate-400">
                <Lock className="h-3.5 w-3.5 text-amber-400" />
                <span>Editing Locked {!assignment.allowResubmission ? '(Resubmission Disabled)' : '(Deadline Passed)'}</span>
              </div>
            </div>

            <div className="space-y-2 text-xs">
              <span className="text-slate-400 font-semibold">Submitted Answer Content:</span>
              <div className="p-3.5 rounded-xl border border-white/10 bg-slate-900 text-slate-200 font-mono whitespace-pre-line">
                {existingSubmission.answerText}
              </div>
            </div>

            {existingSubmission.attachmentUrl && (
              <div className="flex items-center justify-between text-xs pt-2">
                <span className="text-slate-400 font-semibold">Attached Link:</span>
                <a
                  href={existingSubmission.attachmentUrl}
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-1.5 text-emerald-400 hover:underline"
                >
                  <span>{existingSubmission.attachmentUrl}</span>
                  <ExternalLink className="h-3.5 w-3.5" />
                </a>
              </div>
            )}
          </div>
        )}

        {/* SCENARIO 4: Form for New Submission OR Resubmission Update */}
        {(canSubmitNew || canResubmit) && (
          <form onSubmit={handleSubmit} className="space-y-4 pt-4 border-t border-white/10">
            <div className="flex items-center justify-between">
              <h3 className="text-sm font-bold text-white flex items-center gap-2">
                <Send className="h-4 w-4 text-emerald-400" />
                <span>{canResubmit ? 'Update Your Submission' : 'Submit Your Answer'}</span>
              </h3>
              {canResubmit && (
                <span className="text-xs text-amber-400 font-mono">Resubmission allowed before deadline</span>
              )}
            </div>

            <Textarea
              label="Your Answer / Content"
              value={answerText}
              onChange={(e) => setAnswerText(e.target.value)}
              placeholder="Write your complete solution, essay, or answers here..."
              rows={5}
              error={answerError}
              required
            />

            <Input
              label="Attachment URL (Optional)"
              value={attachmentUrl}
              onChange={(e) => setAttachmentUrl(e.target.value)}
              placeholder="e.g. https://drive.google.com/file/d/your-file-link"
              helperText="Provide a link to your Google Drive, PDF, or GitHub repository"
            />

            <div className="flex items-center justify-end gap-3 pt-3">
              <Button type="submit" isLoading={isSubmitting}>
                {canResubmit ? 'Update Submission' : 'Submit Assignment'}
              </Button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
