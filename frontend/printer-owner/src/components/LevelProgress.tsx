interface LevelProgressProps {
  currentLevel: string;
  nextLevel: string;
  currentXP: number;
  requiredXP: number;
}

export default function LevelProgress({ currentLevel, nextLevel, currentXP, requiredXP }: LevelProgressProps) {
  const percentage = Math.min(100, Math.max(0, (currentXP / requiredXP) * 100));

  return (
    <div className="bg-white rounded-2xl p-6 shadow-sm border border-slate-100 relative overflow-hidden">
      {/* Decorative background element */}
      <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 rounded-bl-full -mr-10 -mt-10" />

      <div className="relative z-10">
        <div className="flex justify-between items-end mb-2">
          <div>
            <div className="text-xs text-slate-500 font-bold uppercase tracking-wider mb-1">Current Level</div>
            <div className="text-2xl font-black text-primary">{currentLevel}</div>
          </div>
          <div className="text-right">
            <div className="text-sm font-bold text-slate-900">{currentXP} / {requiredXP} XP</div>
          </div>
        </div>

        <div className="h-3 w-full bg-slate-100 rounded-full my-4 overflow-hidden relative">
          <div 
            className="h-full bg-primary rounded-full transition-all duration-1000 ease-out"
            style={{ width: `${percentage}%` }}
          />
        </div>

        <p className="text-sm text-slate-600">
          Earn <span className="font-bold text-slate-900">{requiredXP - currentXP} more XP</span> to reach <span className="font-bold">{nextLevel}</span> level.
        </p>
      </div>
    </div>
  );
}
