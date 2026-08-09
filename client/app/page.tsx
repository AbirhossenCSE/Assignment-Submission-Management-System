'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { getUser, isAuthenticated } from '@/lib/auth';
import { getRoleName } from '@/types';
import { Loader2 } from 'lucide-react';

export default function RootHomePage() {
  const router = useRouter();

  useEffect(() => {
    if (isAuthenticated()) {
      const user = getUser();
      if (user) {
        const roleName = getRoleName(user.role);
        if (roleName === 'Admin') {
          router.replace('/admin');
        } else if (roleName === 'Teacher') {
          router.replace('/teacher');
        } else {
          router.replace('/student');
        }
      } else {
        router.replace('/login');
      }
    } else {
      router.replace('/login');
    }
  }, [router]);

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-950 text-white">
      <div className="flex flex-col items-center gap-3 text-slate-400">
        <Loader2 className="h-8 w-8 animate-spin text-indigo-500" />
        <p className="text-sm font-medium">Redirecting to portal...</p>
      </div>
    </main>
  );
}
