export default function HomePage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center bg-gradient-to-br from-slate-900 via-indigo-950 to-slate-900 p-6 text-white">
      <div className="w-full max-w-2xl rounded-2xl border border-white/10 bg-white/5 p-8 backdrop-blur-xl shadow-2xl text-center space-y-4">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-indigo-500/20 text-indigo-400 border border-indigo-500/30 text-2xl font-bold mb-2">
          📚
        </div>
        <h1 className="text-3xl sm:text-4xl font-extrabold tracking-tight text-transparent bg-clip-text bg-gradient-to-r from-indigo-300 via-white to-purple-300">
          Assignment Management System
        </h1>
        <p className="text-slate-300 text-sm sm:text-base leading-relaxed max-w-lg mx-auto">
          Role-based portal for Admin, Teachers, and Students. Features course management, assignment distribution, student submissions, and teacher grading workflows.
        </p>
        <div className="pt-4 flex items-center justify-center gap-3 text-xs text-indigo-300/80 font-mono">
          <span className="px-3 py-1.5 rounded-full bg-indigo-500/10 border border-indigo-500/20">Next.js 14+ App Router</span>
          <span className="px-3 py-1.5 rounded-full bg-purple-500/10 border border-purple-500/20">TypeScript</span>
          <span className="px-3 py-1.5 rounded-full bg-emerald-500/10 border border-emerald-500/20">ASP.NET Core 8 API</span>
        </div>
      </div>
    </main>
  );
}
