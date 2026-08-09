'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { api } from '@/lib/api';
import { getUser } from '@/lib/auth';
import { ApiResponse, Assignment, AssignmentStatus, Subject, User } from '@/types';
import Modal from '@/components/Modal';
import ConfirmDialog from '@/components/ConfirmDialog';
import Button from '@/components/Button';
import Input from '@/components/Input';
import Textarea from '@/components/Textarea';
import Select from '@/components/Select';
import { useToast } from '@/components/Toast';
import {
  Plus,
  Edit2,
  Trash2,
  Send,
  Eye,
  FileText,
  Clock,
  AlertCircle,
  Loader2,
} from 'lucide-react';

export default function TeacherAssignmentsPage() {
  const { showToast } = useToast();
  const currentUser = getUser();

  const [assignments, setAssignments] = useState<Assignment[]>([]);
  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Add / Edit Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingAssignment, setEditingAssignment] = useState<Assignment | null>(null);

  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [subjectId, setSubjectId] = useState('');
  const [classId, setClassId] = useState('');
  const [className, setClassName] = useState('');
  const [deadline, setDeadline] = useState('');
  const [maxMarks, setMaxMarks] = useState<number | string>(100);
  const [allowResubmission, setAllowResubmission] = useState(true);
  const [status, setStatus] = useState<AssignmentStatus>(AssignmentStatus.Draft);

  // Errors
  const [titleError, setTitleError] = useState('');
  const [descriptionError, setDescriptionError] = useState('');
  const [subjectError, setSubjectError] = useState('');
  const [deadlineError, setDeadlineError] = useState('');
  const [maxMarksError, setMaxMarksError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Publish Dialog State
  const [isPublishOpen, setIsPublishOpen] = useState(false);
  const [publishingAssignmentId, setPublishingAssignmentId] = useState<string | null>(null);
  const [isPublishing, setIsPublishing] = useState(false);

  // Delete Dialog State
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingAssignmentId, setDeletingAssignmentId] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [assignRes, subjRes] = await Promise.allSettled([
        api.get<ApiResponse<Assignment[]>>('/assignments/my'),
        api.get<ApiResponse<Subject[]>>('/subjects'),
      ]);

      if (assignRes.status === 'fulfilled' && assignRes.value.data.data) {
        setAssignments(assignRes.value.data.data);
      }

      if (subjRes.status === 'fulfilled' && subjRes.value.data.data) {
        const allSubjs = subjRes.value.data.data;
        // Filter subjects assigned to current teacher if teacherId is present
        const teacherSubjs = currentUser?.id
          ? allSubjs.filter((s) => s.teacherId === currentUser.id)
          : allSubjs;
        setSubjects(teacherSubjs.length > 0 ? teacherSubjs : allSubjs);
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to fetch assignments.';
      showToast(msg, 'error');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Handle subject dropdown change to auto-fill classId and className
  const handleSubjectChange = (selectedId: string) => {
    setSubjectId(selectedId);
    const selectedSubj = subjects.find((s) => s.id === selectedId);
    if (selectedSubj) {
      setClassId(selectedSubj.classId);
      setClassName(selectedSubj.className || 'Class N/A');
    } else {
      setClassId('');
      setClassName('');
    }
  };

  const openAddModal = () => {
    setEditingAssignment(null);
    setTitle('');
    setDescription('');
    setSubjectId('');
    setClassId('');
    setClassName('');
    setDeadline('');
    setMaxMarks(100);
    setAllowResubmission(true);
    setStatus(AssignmentStatus.Draft);

    setTitleError('');
    setDescriptionError('');
    setSubjectError('');
    setDeadlineError('');
    setMaxMarksError('');

    setIsModalOpen(true);
  };

  const openEditModal = (assignment: Assignment) => {
    setEditingAssignment(assignment);
    setTitle(assignment.title);
    setDescription(assignment.description);
    setSubjectId(assignment.subjectId);
    setClassId(assignment.classId);
    setClassName(assignment.className);
    
    // Format ISO string to YYYY-MM-THH:mm for datetime-local input
    if (assignment.deadline) {
      const d = new Date(assignment.deadline);
      const tzOffset = d.getTimezoneOffset() * 60000;
      const localISOTime = new Date(d.getTime() - tzOffset).toISOString().slice(0, 16);
      setDeadline(localISOTime);
    } else {
      setDeadline('');
    }

    setMaxMarks(assignment.maxMarks);
    setAllowResubmission(assignment.allowResubmission);
    setStatus(
      assignment.status === AssignmentStatus.Published || (assignment.status as any) === 2
        ? AssignmentStatus.Published
        : AssignmentStatus.Draft
    );

    setTitleError('');
    setDescriptionError('');
    setSubjectError('');
    setDeadlineError('');
    setMaxMarksError('');

    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    let isValid = true;

    setTitleError('');
    setDescriptionError('');
    setSubjectError('');
    setDeadlineError('');
    setMaxMarksError('');

    if (!title.trim()) {
      setTitleError('Assignment title is required.');
      isValid = false;
    }

    if (!description.trim()) {
      setDescriptionError('Assignment description is required.');
      isValid = false;
    }

    if (!subjectId) {
      setSubjectError('Subject selection is required.');
      isValid = false;
    }

    if (!deadline) {
      setDeadlineError('Deadline is required.');
      isValid = false;
    } else {
      const deadlineDate = new Date(deadline);
      if (isNaN(deadlineDate.getTime())) {
        setDeadlineError('Invalid deadline format.');
        isValid = false;
      } else if (!editingAssignment && deadlineDate <= new Date()) {
        setDeadlineError('Deadline must be a future date/time.');
        isValid = false;
      }
    }

    const marksNum = Number(maxMarks);
    if (isNaN(marksNum) || marksNum <= 0) {
      setMaxMarksError('Max marks must be a positive number.');
      isValid = false;
    }

    if (!isValid) return;

    setIsSubmitting(true);

    try {
      const payload = {
        title: title.trim(),
        description: description.trim(),
        classId: classId,
        subjectId: subjectId,
        deadline: new Date(deadline).toISOString(),
        maxMarks: marksNum,
        status: status,
        allowResubmission: allowResubmission,
      };

      if (editingAssignment) {
        // PUT /api/assignments/{id}
        await api.put(`/assignments/${editingAssignment.id}`, payload);
        showToast('Assignment updated successfully!');
      } else {
        // POST /api/assignments
        await api.post('/assignments', payload);
        showToast('Assignment created successfully!');
      }

      setIsModalOpen(false);
      fetchData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to save assignment.';
      showToast(msg, 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  const openPublishDialog = (id: string) => {
    setPublishingAssignmentId(id);
    setIsPublishOpen(true);
  };

  const handlePublishConfirm = async () => {
    if (!publishingAssignmentId) return;
    setIsPublishing(true);

    try {
      // PATCH /api/assignments/{id}/publish
      await api.patch(`/assignments/${publishingAssignmentId}/publish`);
      showToast('Assignment published successfully!');
      setIsPublishOpen(false);
      fetchData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to publish assignment.';
      showToast(msg, 'error');
    } finally {
      setIsPublishing(false);
    }
  };

  const openDeleteDialog = (id: string) => {
    setDeletingAssignmentId(id);
    setIsDeleteOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingAssignmentId) return;
    setIsDeleting(true);

    try {
      // DELETE /api/assignments/{id}
      await api.delete(`/assignments/${deletingAssignmentId}`);
      showToast('Assignment deleted successfully.');
      setIsDeleteOpen(false);
      fetchData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to delete assignment.';
      showToast(msg, 'error');
    } finally {
      setIsDeleting(false);
    }
  };

  const subjectOptions = subjects.map((s) => ({
    value: s.id,
    label: `${s.name} (${s.code}) - ${s.className || 'Class N/A'}`,
  }));

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

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold text-white">My Assignments</h1>
          <p className="text-sm text-slate-400">Create, publish, and manage coursework for your classes.</p>
        </div>
        <Button onClick={openAddModal} icon={<Plus className="h-4 w-4" />}>
          Create Assignment
        </Button>
      </div>

      {/* Assignments Table Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-12 flex flex-col items-center justify-center text-slate-400 gap-3">
            <Loader2 className="h-8 w-8 animate-spin text-purple-500" />
            <p className="text-sm">Loading assignments...</p>
          </div>
        ) : assignments.length === 0 ? (
          <div className="p-12 text-center text-slate-400 space-y-3">
            <FileText className="h-12 w-12 mx-auto text-slate-600" />
            <p className="text-base font-semibold text-slate-300">No assignments created yet.</p>
            <p className="text-xs text-slate-500">Click "Create Assignment" to publish your first assignment.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-300">
              <thead className="border-b border-white/10 bg-slate-950/60 text-xs font-semibold uppercase tracking-wider text-slate-400">
                <tr>
                  <th className="py-3.5 px-6">Title</th>
                  <th className="py-3.5 px-6">Subject & Class</th>
                  <th className="py-3.5 px-6">Deadline</th>
                  <th className="py-3.5 px-6">Max Marks</th>
                  <th className="py-3.5 px-6">Status</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {assignments.map((assignment) => {
                  const isDraft =
                    assignment.status === AssignmentStatus.Draft || (assignment.status as any) === 1;
                  const past = isPastDeadline(assignment.deadline);

                  return (
                    <tr key={assignment.id} className="hover:bg-white/5 transition duration-150">
                      <td className="py-4 px-6 font-bold text-white max-w-xs truncate">
                        <Link
                          href={`/teacher/assignments/${assignment.id}`}
                          className="hover:text-purple-400 transition"
                        >
                          {assignment.title}
                        </Link>
                      </td>
                      <td className="py-4 px-6">
                        <p className="font-semibold text-slate-200">{assignment.subjectName}</p>
                        <p className="text-xs text-slate-400">{assignment.className}</p>
                      </td>
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
                      <td className="py-4 px-6 font-mono text-purple-300 font-semibold">
                        {assignment.maxMarks} pts
                      </td>
                      <td className="py-4 px-6">
                        <span
                          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${
                            isDraft
                              ? 'bg-amber-500/10 border-amber-500/20 text-amber-400'
                              : 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                          }`}
                        >
                          {isDraft ? 'Draft' : 'Published'}
                        </span>
                      </td>
                      <td className="py-4 px-6 text-right space-x-2">
                        <Link href={`/teacher/assignments/${assignment.id}`}>
                          <Button variant="secondary" size="sm" icon={<Eye className="h-3.5 w-3.5" />}>
                            View
                          </Button>
                        </Link>

                        {isDraft && (
                          <Button
                            variant="secondary"
                            size="sm"
                            onClick={() => openPublishDialog(assignment.id)}
                            icon={<Send className="h-3.5 w-3.5 text-emerald-400" />}
                          >
                            Publish
                          </Button>
                        )}

                        <Button
                          variant="secondary"
                          size="sm"
                          onClick={() => openEditModal(assignment)}
                          icon={<Edit2 className="h-3.5 w-3.5" />}
                        >
                          Edit
                        </Button>

                        <Button
                          variant="danger"
                          size="sm"
                          onClick={() => openDeleteDialog(assignment.id)}
                          icon={<Trash2 className="h-3.5 w-3.5" />}
                        >
                          Delete
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

      {/* Add / Edit Assignment Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingAssignment ? 'Edit Assignment' : 'Create New Assignment'}
        maxWidth="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="Assignment Title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="e.g. Algebra Quiz #1 or Midterm Project"
            error={titleError}
            required
          />

          <Textarea
            label="Description & Instructions"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Provide clear instructions for students..."
            error={descriptionError}
            rows={4}
            required
          />

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Select
              label="Assigned Subject"
              value={subjectId}
              onChange={(e) => handleSubjectChange(e.target.value)}
              options={subjectOptions}
              placeholder="Select subject"
              error={subjectError}
              required
            />

            <Input
              label="Target Class (Auto-filled)"
              value={className || 'Select a subject first'}
              disabled
              readOnly
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Input
              label="Deadline (Date & Time)"
              type="datetime-local"
              value={deadline}
              onChange={(e) => setDeadline(e.target.value)}
              error={deadlineError}
              required
            />

            <Input
              label="Max Marks"
              type="number"
              min="1"
              value={maxMarks}
              onChange={(e) => setMaxMarks(e.target.value)}
              error={maxMarksError}
              required
            />
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2">
            {/* Allow Resubmission Checkbox */}
            <label className="flex items-center gap-3 p-3 rounded-xl border border-white/10 bg-slate-950/40 cursor-pointer">
              <input
                type="checkbox"
                checked={allowResubmission}
                onChange={(e) => setAllowResubmission(e.target.checked)}
                className="h-4 w-4 rounded border-white/20 bg-slate-900 text-purple-600 focus:ring-purple-500"
              />
              <div>
                <span className="block text-xs font-semibold text-white">Allow Resubmission</span>
                <span className="block text-[11px] text-slate-400">Students can update answers before deadline</span>
              </div>
            </label>

            {/* Status Radio Options */}
            <div className="p-3 rounded-xl border border-white/10 bg-slate-950/40 space-y-1">
              <span className="block text-xs font-semibold uppercase tracking-wider text-slate-300">Status</span>
              <div className="flex items-center gap-4 pt-1 text-xs text-white">
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    name="status"
                    checked={status === AssignmentStatus.Draft}
                    onChange={() => setStatus(AssignmentStatus.Draft)}
                    className="text-amber-500 focus:ring-amber-500"
                  />
                  <span>Draft</span>
                </label>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="radio"
                    name="status"
                    checked={status === AssignmentStatus.Published}
                    onChange={() => setStatus(AssignmentStatus.Published)}
                    className="text-emerald-500 focus:ring-emerald-500"
                  />
                  <span>Publish Now</span>
                </label>
              </div>
            </div>
          </div>

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-white/10">
            <Button
              type="button"
              variant="ghost"
              onClick={() => setIsModalOpen(false)}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={isSubmitting}>
              {editingAssignment ? 'Update Assignment' : 'Create Assignment'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Confirm Publish Dialog */}
      <ConfirmDialog
        isOpen={isPublishOpen}
        onClose={() => setIsPublishOpen(false)}
        onConfirm={handlePublishConfirm}
        title="Publish Assignment"
        message="Are you sure you want to publish this assignment? Once published, students will immediately be able to view and submit their answers."
        confirmText="Publish Now"
        isLoading={isPublishing}
      />

      {/* Confirm Delete Dialog */}
      <ConfirmDialog
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleDeleteConfirm}
        title="Delete Assignment"
        message="Are you sure you want to delete this assignment? This action cannot be undone."
        isLoading={isDeleting}
      />
    </div>
  );
}
