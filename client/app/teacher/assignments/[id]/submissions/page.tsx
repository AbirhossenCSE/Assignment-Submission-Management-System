'use client';

import React from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import Button from '@/components/Button';
import { ArrowLeft, FileCheck } from 'lucide-react';

export default function SubmissionsPlaceholderPage() {
  const params = useParams();
  const id = params?.id as string;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <Link
        href={`/teacher/assignments/${id}`}
        className="inline-flex items-center gap-2 text-xs font-semibold text-slate-400 hover:text-white transition"
      >
        <ArrowLeft className="h-4 w-4" />
        <span>Back to Assignment Details</span>
      </Link>

      <div className="p-12 rounded-2xl border border-white/10 bg-slate-900/60 backdrop-blur-xl text-center space-y-4">
        <div className="p-3 rounded-2xl bg-purple-500/10 border border-purple-500/20 text-purple-400 w-14 h-14 mx-auto flex items-center justify-center">
          <FileCheck className="h-7 w-7" />
        </div>
        <h2 className="text-xl font-bold text-white">Student Submissions & Grading Portal</h2>
        <p className="text-sm text-slate-400 max-w-md mx-auto">
          Review student answers, grade submissions, and provide detailed feedback. This feature module will be fully connected in the upcoming step!
        </p>
      </div>
    </div>
  );
}
