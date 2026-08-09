'use client';

import React, { useEffect, useState } from 'react';
import { api } from '@/lib/api';
import { ApiResponse, ClassEntity } from '@/types';
import Modal from '@/components/Modal';
import ConfirmDialog from '@/components/ConfirmDialog';
import Button from '@/components/Button';
import Input from '@/components/Input';
import { useToast } from '@/components/Toast';
import { Plus, Edit2, Trash2, School, Loader2 } from 'lucide-react';

export default function ClassesPage() {
  const { showToast } = useToast();

  const [classes, setClasses] = useState<ClassEntity[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  // Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingClass, setEditingClass] = useState<ClassEntity | null>(null);
  const [name, setName] = useState('');
  const [section, setSection] = useState('');
  const [nameError, setNameError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Confirm Delete Dialog State
  const [isDeleteOpen, setIsDeleteOpen] = useState(false);
  const [deletingClassId, setDeletingClassId] = useState<string | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const fetchClasses = async () => {
    try {
      setIsLoading(true);
      const res = await api.get<ApiResponse<ClassEntity[]>>('/classes');
      if (res.data && res.data.data) {
        setClasses(res.data.data);
      }
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to fetch classes.';
      showToast(msg, 'error');
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    fetchClasses();
  }, []);

  const openAddModal = () => {
    setEditingClass(null);
    setName('');
    setSection('');
    setNameError('');
    setIsModalOpen(true);
  };

  const openEditModal = (cls: ClassEntity) => {
    setEditingClass(cls);
    setName(cls.name);
    setSection(cls.section || '');
    setNameError('');
    setIsModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setNameError('Class name is required.');
      return;
    }

    setIsSubmitting(true);
    setNameError('');

    try {
      if (editingClass) {
        // PUT /api/classes/{id}
        await api.put(`/classes/${editingClass.id}`, {
          name: name.trim(),
          section: section.trim() || null,
        });
        showToast('Class updated successfully!');
      } else {
        // POST /api/classes
        await api.post('/classes', {
          name: name.trim(),
          section: section.trim() || null,
        });
        showToast('Class created successfully!');
      }

      setIsModalOpen(false);
      fetchClasses();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to save class.';
      showToast(msg, 'error');
    } finally {
      setIsSubmitting(false);
    }
  };

  const openDeleteDialog = (id: string) => {
    setDeletingClassId(id);
    setIsDeleteOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!deletingClassId) return;
    setIsDeleting(true);

    try {
      await api.delete(`/classes/${deletingClassId}`);
      showToast('Class deleted successfully.');
      setIsDeleteOpen(false);
      fetchClasses();
    } catch (err: any) {
      const msg = err.response?.data?.message || 'Failed to delete class.';
      showToast(msg, 'error');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header Bar */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-extrabold text-white">Classes Management</h1>
          <p className="text-sm text-slate-400">Create, edit, and organize academic classes and sections.</p>
        </div>
        <Button onClick={openAddModal} icon={<Plus className="h-4 w-4" />}>
          Add Class
        </Button>
      </div>

      {/* Classes Table Card */}
      <div className="rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl overflow-hidden shadow-2xl">
        {isLoading ? (
          <div className="p-12 flex flex-col items-center justify-center text-slate-400 gap-3">
            <Loader2 className="h-8 w-8 animate-spin text-indigo-500" />
            <p className="text-sm">Loading classes...</p>
          </div>
        ) : classes.length === 0 ? (
          <div className="p-12 text-center text-slate-400 space-y-3">
            <School className="h-12 w-12 mx-auto text-slate-600" />
            <p className="text-base font-semibold text-slate-300">No classes created yet.</p>
            <p className="text-xs text-slate-500">Click "Add Class" to register your first class entity.</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-300">
              <thead className="border-b border-white/10 bg-slate-950/60 text-xs font-semibold uppercase tracking-wider text-slate-400">
                <tr>
                  <th className="py-3.5 px-6">Class Name</th>
                  <th className="py-3.5 px-6">Section</th>
                  <th className="py-3.5 px-6">Status</th>
                  <th className="py-3.5 px-6 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-white/5">
                {classes.map((cls) => (
                  <tr key={cls.id} className="hover:bg-white/5 transition duration-150">
                    <td className="py-4 px-6 font-bold text-white">{cls.name}</td>
                    <td className="py-4 px-6 text-slate-400">{cls.section || 'N/A'}</td>
                    <td className="py-4 px-6">
                      <span
                        className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${
                          cls.isActive
                            ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-400'
                            : 'bg-red-500/10 border-red-500/20 text-red-400'
                        }`}
                      >
                        {cls.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td className="py-4 px-6 text-right space-x-2">
                      <Button
                        variant="secondary"
                        size="sm"
                        onClick={() => openEditModal(cls)}
                        icon={<Edit2 className="h-3.5 w-3.5" />}
                      >
                        Edit
                      </Button>
                      <Button
                        variant="danger"
                        size="sm"
                        onClick={() => openDeleteDialog(cls.id)}
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

      {/* Add / Edit Class Modal */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingClass ? 'Edit Class' : 'Add New Class'}
      >
        <form onSubmit={handleSubmit} className="space-y-4">
          <Input
            label="Class Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Class 10 or BSc CSE Year 2"
            error={nameError}
            required
          />

          <Input
            label="Section (Optional)"
            value={section}
            onChange={(e) => setSection(e.target.value)}
            placeholder="e.g. A or Morning Shift"
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
              {editingClass ? 'Update Class' : 'Create Class'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* Delete Confirmation Dialog */}
      <ConfirmDialog
        isOpen={isDeleteOpen}
        onClose={() => setIsDeleteOpen(false)}
        onConfirm={handleDeleteConfirm}
        title="Delete Class"
        message="Are you sure you want to delete this class? This will deactivate the class record."
        isLoading={isDeleting}
      />
    </div>
  );
}
