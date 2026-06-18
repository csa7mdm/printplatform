import { Trophy } from 'lucide-react';

interface Badge {
  id: string;
  name: string;
  description: string;
  isUnlocked: boolean;
  icon: string; // Emoji or SVG path
}

interface BadgeGridProps {
  badges: Badge[];
}

export default function BadgeGrid({ badges }: BadgeGridProps) {
  return (
    <div className="grid grid-cols-3 gap-4">
      {badges.map(badge => (
        <div 
          key={badge.id}
          className={`flex flex-col items-center p-3 rounded-xl border text-center transition-all ${
            badge.isUnlocked 
              ? 'bg-white border-primary/20 shadow-sm' 
              : 'bg-slate-50 border-slate-100 grayscale opacity-60'
          }`}
        >
          <div className={`w-12 h-12 rounded-full flex items-center justify-center text-2xl mb-2 ${
            badge.isUnlocked ? 'bg-primary/10' : 'bg-slate-200'
          }`}>
            {badge.icon}
          </div>
          <div className="font-bold text-xs text-slate-900 mb-1 leading-tight">{badge.name}</div>
          <div className="text-[10px] text-slate-500 leading-tight hidden md:block">{badge.description}</div>
        </div>
      ))}
    </div>
  );
}
