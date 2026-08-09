'use client';

import React, { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { ApiResponse, ClassEntity, Subject, User } from '@/types';
import Modal from '@/components/Modal';
import ConfirmDialog from '@/components/ConfirmDialog';
import Button from '@/components/Button';
import Input from '@/components/Input';
import Select from '@/components/Select';
import { useToast } from '@/components/Toast';
import { Plus, Edit2, Trash2, UserPlus, BookOpen, Loader2 } from 'lucide-react';

export default function SubjectsPage() {
  const { showToast } = useToast();

  const [subjects, setSubjects] = useState<Subject[]>([]);
  const [classes, setClasses] = useState<ClassEntity[]>([]);
  const [teachers, setTeachers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Add/Edit Subject Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingSubject, setEditingSubject] = useState<Subject | null>(null);
  const [name, setName] = useState('');
  const [code, setCode] = useState('');
  const [classId, setClassId] = useState('');
  const [nameError, setNameError] = useState('');
  const [codeError, setCodeError] = useState('');
  const [classIdError, setClassIdError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Assign Teacher Modal State
  const [isAssignModalOpen, setIsAssignModalOpen] = useState(false);
  const [assigningSubject, setAssigningSubject] = useState<Subject | null>(null);
  const [selectedTeacherId, setSelectedTeacherId] = useState('');
  const [isAssigning, setIsAssigning] = useState(false);

  // Delete Confirm State
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingSubjectId, setDeletingSubjectId] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const [subjRes, classRes, teachRes] = await Promise.allSettled([
        api.get<ApiResponse<Subject[]>>('/subjects'),
        api.get<ApiResponse<ClassEntity[]>>('/classes'),
        api.get<ApiResponse<User[]>>('/user?role=2'), // Role 2 = Teacher
      ]);

      if (subjRes.status === 'fulfilled' && subjRes.value.data.data) {
        setSubjects(subjRes.value.data.data);
      }
      if (classRes.status === 'fulfilled' && classRes.value.data.data) {
        setClasses(classRes.value.data.data);
      }
      if (teachRes.status === 'fulfilled' && teachRes.value.data.data) {
        setTeachers(teachRes.value.data.data);
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to fetch subject data.';
      showToast(msg, 'error');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const openAddModal = () => {
    setEditingSubject(null);
    setName('');
    setCode('');
    setClassId('');
    setNameError('');
    setCodeError('');
    setClassIdError('');
    setIsModalOpen(true);
  };

  const openEditModal = (subj: Subject) => {
    setEditingSubject(subj);
    setName(subj.name);
    setCode(subj.code);
    setClassId(subj.classId);
    setNameError('');
    setCodeError('');
    setClassIdError('');
    setIsModalOpen(true);
  };

  const handleSubjectSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    let isValid = true;
    setNameError('');
    setCodeError('');
    setClassIdError('');

    if (!name.trim()) {
      setNameError('Subject name is required.');
      isValid = false;
    }

    if (!code.trim()) {
      setCodeError('Subject code is required.');
      isValid = false;
    }

    if (!classId) {
      setClassIdError('Class selection is required.');
      isValid = false;
    }

    if (!isValid) return;

    setIsSubmitting(true);

    try {
      if (editingSubject) {
        // PUT /api/subjects/{id}
        await api.put(`/subjects/${editingSubject.id}`, {
          name: name.trim(),
          code: code.trim().toUpperCase(),
          classId: classId,
        });
        showToast('Subject updated successfully!');
      } else {
        // POST /api/subjects
        await api.post('/subjects', {
          name: name.trim(),
          code: code.trim().toUpperCase(),
          classId: classId,
        });
        showToast('Subject created successfully!');
      }

      setIsModalOpen(false);
      fetchData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to save subject.';
      showToast(msg, 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  const openAssignModal = (subj: Subject) => {
    setAssigningSubject(subj);
    setSelectedTeacherId(subj.teacherId || '');
    setIsAssignModalOpen(true);
  };

  const handleAssignTeacherSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!assigningSubject) return;

    setIsAssigning(true);

    try {
      // PATCH /api/subjects/{id}/assign-teacher
      await api.patch(`/subjects/${assigningSubject.id}/assign-teacher`, {
        teacherId: selectedTeacherId || null,
      });

      showToast('Teacher assigned successfully!');
      setIsAssignModalOpen(false);
      fetchData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to assign teacher.';
      showToast(msg, 'error');
    } finally {
      setIsAssigning(false);
    }
  };

  const openDeleteDialog = (id: string) => {
    setDeletingSubjectId(id);
    setIsDeleteOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingSubjectId) return;
    setIsDeleting(true);

    try {
      await api.delete(`/subjects/${deletingSubjectId}`);
      showToast('Subject deleted successfully.');
      setIsDeleteOpen(false);
      fetchData();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to delete subject.';
      showToast(msg, 'error');
    } finally {
      setIsDeleting(false);
    }
  };

  const classOptions = classes.map((c) => ({
    value: c.id,
    label: c.section ? `${c.name} (${c.section})` : c.name,
  }));

  const teacherOptions = teachers.map((t) => ({
    value: t.id,
    label: `${t.fullName} (${t.email})`,
  }));

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold text-white">Subjects Management</h1>
          <p className="text-sm text-slate-400">Manage curriculum subjects and assign teachers.</p>
        </div>
        <Button onClick={openAddModal} icon={<Plus className="h-4 w-4" />}>
          Add Subject
        </Button>
      </div>

      {/* Subjects Table Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-12 flex flex-col items-center justify-center text-slate-400 gap-3">
            <Loader2 className="h-8 w-8 animate-spin text-purple-500" />
            <p className="text-sm">Loading subjects...</p>
          </div>
        ) : subjects.length === 0 ? (
          <div className="p-12 text-center text-slate-400 space-y-3">
            <BookOpen className="h-12 w-12 mx-auto text-slate-600" />
            <p className="text-base font-semibold text-slate-300">No subjects created yet.</p>
            <p className="text-xs text-slate-500">Click "Add Subject" to register your first subject entity.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-300">
              <thead className="border-b border-white/10 bg-slate-950/60 text-xs font-semibold uppercase tracking-wider text-slate-400">
                <tr>
                  <th className="py-3.5 px-6">Subject Code</th>
                  <th className="py-3.5 px-6">Subject Name</th>
                  <th className="py-3.5 px-6">Class</th>
                  <th className="py-3.5 px-6">Assigned Teacher</th>
                  <th className="py-3.5 px-6">Status</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {subjects.map((subj) => (
                  <tr key={subj.id} className="hover:bg-white/5 transition duration-150">
                    <td className="py-4 px-6 font-mono font-bold text-indigo-400">{subj.code}</td>
                    <td className="py-4 px-6 font-bold text-white">{subj.name}</td>
                    <td className="py-4 px-6 text-slate-300">{subj.className || 'N/A'}</td>
                    <td className="py-4 px-6">
                      {subj.teacherName ? (
                        <span className="font-medium text-purple-300">{subj.teacherName}</span>
                      ) : (
                        <span className="text-xs text-slate-500 italic">Unassigned</span>
                      )}
                    </td>
                    <td className="py-4 px-6">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${
                          subj.isActive
                            ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                            : 'bg-red-500/10 border-red-500/20 text-red-400'
                        }`}
                      >
                        {subj.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="py-4 px-6 text-right space-x-2">
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => openAssignModal(subj)}
                        icon={<UserPlus className="h-3.5 w-3.5" />}
                      >
                        Assign
                      </Button>
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => openEditModal(subj)}
                        icon={<Edit2 className="h-3.5 w-3.5" />}
                      >
                        Edit
                      </Button>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => openDeleteDialog(subj.id)}
                        icon={<Trash2 className="h-3.5 w-3.5" />}
                      >
                        Delete
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Add / Edit Subject Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingSubject ? 'Edit Subject' : 'Add New Subject'}
      >
        <form onSubmit={handleSubjectSubmit} className="space-y-4">
          <Input
            label="Subject Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Mathematics or World History"
            error={nameError}
            required
          />

          <Input
            label="Subject Code"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="e.g. MATH101 or HIS201"
            error={codeError}
            required
          />

          <Select
            label="Assigned Class"
            value={classId}
            onChange={(e) => setClassId(e.target.value)}
            options={classOptions}
            placeholder="Select a class"
            error={classIdError}
            required
          />

          <div className="flex items-center justify-end gap-3 pt-3 border-t border-white/10">
            <Button
              type="button"
              variant="ghost"
              onClick={() => setIsModalOpen(false)}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={isSubmitting}>
              {editingSubject ? 'Update Subject' : 'Create Subject'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Assign Teacher Modal */}
      <Modal
        isOpen={isAssignModalOpen}
        onClose={() => setIsAssignModalOpen(false)}
        title={`Assign Teacher - ${assigningSubject?.name}`}
        maxWidth="sm"
      >
        <form onSubmit={handleAssignTeacherSubmit} className="space-y-4">
          <Select
            label="Teacher"
            value={selectedTeacherId}
            onChange={(e) => setSelectedTeacherId(e.target.value)}
            options={teacherOptions}
            placeholder="Select a teacher"
          />

          <div className="flex items-center justify-end gap-3 pt-3 border-t border-white/10">
            <Button
              type="button"
              variant="ghost"
              onClick={() => setIsAssignModalOpen(false)}
              disabled={isAssigning}
            >
              Cancel
            </Button>
            <Button type="submit" isLoading={isAssigning}>
              Save Assignment
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleDeleteConfirm}
        title="Delete Subject"
        message="Are you sure you want to delete this subject? This action will deactivate the subject."
        isLoading={isDeleting}
      />
    </div>
  );
}
