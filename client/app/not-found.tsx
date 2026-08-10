'use client';

import React from 'react';
import Link from 'next/link';
import Button from '@/components/Button';
import { Home, FileQuestion } from 'lucide-react';

export default function NotFoundPage() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col items-center justify-center p-6 text-center">
      <div className="max-w-md w-full p-8 sm:p-10 rounded-3xl border border-white/10 bg-slate-900/80 backdrop-blur-xl shadow-2xl space-y-6">
        <div className="p-4 rounded-2xl bg-purple-500/10 border border-purple-500/20 text-purple-400 w-16 h-16 mx-auto flex items-center justify-center">
          <FileQuestion className="h-8 w-8" />
        </div>

        <div className="space-y-2">
          <span className="text-xs font-mono font-bold uppercase tracking-widest text-purple-400">404 Error</span>
          <h1 className="text-3xl font-extrabold text-white">Page Not Found</h1>
          <p className="text-sm text-slate-400 leading-relaxed">
            The page or resource you are looking for does not exist, has been moved, or is temporarily unavailable.
          </p>
        </div>

        <div className="pt-4 border-t border-white/10 flex justify-center">
          <Link href="/">
            <Button icon={<Home className="h-4 w-4" />}>
              Return to Home
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
