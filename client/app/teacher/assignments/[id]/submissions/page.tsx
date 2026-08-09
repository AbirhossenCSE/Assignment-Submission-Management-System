'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { api } from '@/lib/api';
import { ApiResponse, Assignment, Submission, SubmissionStatus } from '@/types';
import Modal from '@/components/Modal';
import Button from '@/components/Button';
import Input from '@/components/Input';
import Textarea from '@/components/Textarea';
import { useToast } from '@/components/Toast';
import {
  ArrowLeft,
  Clock,
  Award,
  ExternalLink,
  CheckCircle2,
  AlertCircle,
  FileCheck,
  Award as AwardIcon,
  Loader2,
  Calendar,
  UserCheck,
} from 'lucide-react';

export default function TeacherSubmissionsPage() {
  const params = useParams();
  const assignmentId = params?.id as string;
  const { showToast } = useToast();

  const [assignment, setAssignment] = useState<Assignment | null>(null);
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Review & Grade Modal State
  const [isReviewOpen, setIsReviewOpen] = useState(false);
  const [selectedSubmission, setSelectedSubmission] = useState<Submission | null>(null);
  const [marks, setMarks] = useState<number | string>('');
  const [feedback, setFeedback] = useState('');
  const [marksError, setMarksError] = useState('');
  const [isSubmittingGrade, setIsSubmittingGrade] = useState(false);

  const fetchSubmissionsData = async () => {
    if (!assignmentId) return;
    try {
      setIsLoading(true);
      const [assignRes, subRes] = await Promise.allSettled([
        api.get<ApiResponse<Assignment>>(`/assignments/${assignmentId}`),
        api.get<ApiResponse<Submission[]>>(`/assignments/${assignmentId}/submissions`),
      ]);

      if (assignRes.status === 'fulfilled' && assignRes.value.data.data) {
        setAssignment(assignRes.value.data.data);
      }

      if (subRes.status === 'fulfilled' && subRes.value.data.data) {
        setSubmissions(subRes.value.data.data);
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to load submissions.';
      showToast(msg, 'error');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchSubmissionsData();
  }, [assignmentId]);

  const openReviewModal = (sub: Submission) => {
    setSelectedSubmission(sub);
    setMarks(sub.marks !== null && sub.marks !== undefined ? sub.marks : '');
    setFeedback(sub.feedback || '');
    setMarksError('');
    setIsReviewOpen(true);
  };

  const handleGradeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedSubmission || !assignment) return;

    setMarksError('');
    const marksNum = Number(marks);

    if (marks === '' || isNaN(marksNum)) {
      setMarksError('Please enter a valid numeric mark.');
      return;
    }

    if (marksNum < 0) {
      setMarksError('Marks cannot be negative.');
      return;
    }

    if (marksNum > assignment.maxMarks) {
      setMarksError(`Marks cannot exceed the assignment maximum of ${assignment.maxMarks} points.`);
      return;
    }

    setIsSubmittingGrade(true);

    try {
      // PATCH /api/submissions/{id}/grade
      const res = await api.patch<ApiResponse<Submission>>(`/submissions/${selectedSubmission.id}/grade`, {
        marks: marksNum,
        feedback: feedback.trim() || null,
      });

      showToast(`Successfully graded ${selectedSubmission.studentName}'s submission!`);
      setIsReviewOpen(false);

      // Optimistically update list or refresh data
      if (res.data && res.data.data) {
        const updatedSub = res.data.data;
        setSubmissions((prev) =>
          prev.map((s) => (s.id === updatedSub.id ? updatedSub : s))
        );
      } else {
        fetchSubmissionsData();
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to submit grade.';
      showToast(msg, 'error');
    } finally {
      setIsSubmittingGrade(false);
    }
  };

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
          Late
        </span>
      );
    }

    if (s === SubmissionStatus.Submitted || s === 2) {
      return (
        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-500/10 border border-blue-500/20 text-blue-400">
          <FileCheck className="h-3 w-3" />
          Submitted
        </span>
      );
    }

    return (
      <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-slate-500/10 border border-slate-500/20 text-slate-400">
        Pending
      </span>
    );
  };

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Back Navigation Bar */}
      <div className="flex items-center justify-between">
        <Link
          href={`/teacher/assignments/${assignmentId}`}
          className="inline-flex items-center gap-2 text-xs font-semibold text-slate-400 hover:text-white transition"
        >
          <ArrowLeft className="h-4 w-4" />
          <span>Back to Assignment Details</span>
        </Link>
      </div>

      {/* Parent Assignment Context Card */}
      {assignment && (
        <div className="rounded-2xl border border-white/10 bg-slate-900/80 p-6 backdrop-blur-xl space-y-4 shadow-xl">
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-white/10">
            <div>
              <span className="text-xs font-semibold uppercase tracking-wider text-purple-400">
                {assignment.subjectName} • {assignment.className}
              </span>
              <h1 className="text-2xl font-extrabold text-white mt-1">{assignment.title}</h1>
            </div>
            <div className="flex items-center gap-3">
              <div className="px-3.5 py-1.5 rounded-xl border border-amber-500/20 bg-amber-500/10 text-amber-300 text-xs font-semibold flex items-center gap-1.5">
                <Calendar className="h-4 w-4" />
                <span>Deadline: {formatTimestamp(assignment.deadline)}</span>
              </div>
              <div className="px-3.5 py-1.5 rounded-xl border border-emerald-500/20 bg-emerald-500/10 text-emerald-300 text-xs font-bold flex items-center gap-1.5">
                <Award className="h-4 w-4" />
                <span>Max Marks: {assignment.maxMarks} Points</span>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Submissions List Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl overflow-hidden shadow-2xl">
        <div className="px-6 py-4 border-b border-white/10 bg-slate-950/40 flex items-center justify-between">
          <h2 className="text-base font-bold text-white flex items-center gap-2">
            <UserCheck className="h-5 w-5 text-purple-400" />
            <span>Student Submissions ({submissions.length})</span>
          </h2>
        </div>

        {isLoading ? (
          <div className="p-12 flex flex-col items-center justify-center text-slate-400 gap-3">
            <Loader2 className="h-8 w-8 animate-spin text-purple-500" />
            <p className="text-sm">Loading student submissions...</p>
          </div>
        ) : submissions.length === 0 ? (
          <div className="p-12 text-center text-slate-400 space-y-3">
            <FileCheck className="h-12 w-12 mx-auto text-slate-600" />
            <p className="text-base font-semibold text-slate-300">No submissions yet.</p>
            <p className="text-xs text-slate-500">Students assigned to this class have not turned in any answers yet.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-300">
              <thead className="border-b border-white/10 bg-slate-950/60 text-xs font-semibold uppercase tracking-wider text-slate-400">
                <tr>
                  <th className="py-3.5 px-6">Student Name</th>
                  <th className="py-3.5 px-6">Submitted At</th>
                  <th className="py-3.5 px-6">Status</th>
                  <th className="py-3.5 px-6">Grade / Marks</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {submissions.map((sub) => {
                  const isGraded = sub.marks !== null && sub.marks !== undefined;
                  return (
                    <tr key={sub.id} className="hover:bg-white/5 transition duration-150">
                      <td className="py-4 px-6 font-bold text-white">{sub.studentName}</td>
                      <td className="py-4 px-6 text-slate-300">
                        {sub.submittedAt ? formatTimestamp(sub.submittedAt) : '-'}
                      </td>
                      <td className="py-4 px-6">{renderStatusBadge(sub.status, sub.isLate)}</td>
                      <td className="py-4 px-6 font-mono">
                        {isGraded ? (
                          <span className="font-bold text-emerald-400">
                            {sub.marks} / {assignment?.maxMarks ?? '-'} pts
                          </span>
                        ) : (
                          <span className="text-slate-500">-</span>
                        )}
                      </td>
                      <td className="py-4 px-6 text-right">
                        <Button
                          variant={isGraded ? 'secondary' : 'primary'}
                          size="sm"
                          onClick={() => openReviewModal(sub)}
                          icon={<AwardIcon className="h-3.5 w-3.5" />}
                        >
                          {isGraded ? 'Review & Edit Grade' : 'Review & Grade'}
                        </Button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Review & Grade Modal */}
      {selectedSubmission && (
        <Modal
          isOpen={isReviewOpen}
          onClose={() => setIsReviewOpen(false)}
          title={`Review Submission — ${selectedSubmission.studentName}`}
          maxWidth="lg"
        >
          <div className="space-y-6">
            {/* Submission Details Header */}
            <div className="p-4 rounded-xl border border-white/10 bg-slate-950/60 space-y-3">
              <div className="flex flex-wrap items-center justify-between gap-2 text-xs">
                <div className="flex items-center gap-2">
                  <span className="font-semibold text-slate-300">Submitted:</span>
                  <span className="text-slate-400">{formatTimestamp(selectedSubmission.submittedAt)}</span>
                </div>
                {renderStatusBadge(selectedSubmission.status, selectedSubmission.isLate)}
              </div>

              {/* Attachment Link */}
              {selectedSubmission.attachmentUrl && (
                <div className="pt-2 border-t border-white/10 flex items-center justify-between text-xs">
                  <span className="text-slate-400 font-semibold">Attachment File:</span>
                  <a
                    href={selectedSubmission.attachmentUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="inline-flex items-center gap-1.5 text-purple-400 hover:text-purple-300 font-medium underline"
                  >
                    <span>Open Attachment</span>
                    <ExternalLink className="h-3.5 w-3.5" />
                  </a>
                </div>
              )}
            </div>

            {/* Student Answer Text */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold uppercase tracking-wider text-slate-400">
                Student's Answer Content
              </h4>
              <div className="p-4 rounded-xl border border-white/10 bg-slate-950/40 text-sm text-slate-200 leading-relaxed whitespace-pre-line max-h-60 overflow-y-auto font-mono">
                {selectedSubmission.answerText || 'No answer text provided.'}
              </div>
            </div>

            {/* Grading Form */}
            <form onSubmit={handleGradeSubmit} className="space-y-4 pt-4 border-t border-white/10">
              <h4 className="text-xs font-extrabold uppercase tracking-wider text-purple-400 flex items-center gap-1.5">
                <AwardIcon className="h-4 w-4" />
                Evaluation & Grading
              </h4>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Input
                  label={`Marks Awarded (Max: ${assignment?.maxMarks ?? 100})`}
                  type="number"
                  min="0"
                  max={assignment?.maxMarks}
                  value={marks}
                  onChange={(e) => setMarks(e.target.value)}
                  placeholder={`0 - ${assignment?.maxMarks ?? 100}`}
                  error={marksError}
                  required
                />
              </div>

              <Textarea
                label="Teacher Feedback (Optional)"
                value={feedback}
                onChange={(e) => setFeedback(e.target.value)}
                placeholder="Provide constructive feedback for the student..."
                rows={3}
              />

              <div className="flex items-center justify-end gap-3 pt-3 border-t border-white/10">
                <Button
                  type="button"
                  variant="ghost"
                  onClick={() => setIsReviewOpen(false)}
                  disabled={isSubmittingGrade}
                >
                  Cancel
                </Button>
                <Button type="submit" isLoading={isSubmittingGrade}>
                  {selectedSubmission.marks !== null ? 'Update Grade' : 'Submit Grade'}
                </Button>
              </div>
            </form>
          </div>
        </Modal>
      )}
    </div>
  );
}
