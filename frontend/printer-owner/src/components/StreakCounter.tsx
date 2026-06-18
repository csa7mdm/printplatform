import { Flame } from 'lucide-react';

interface StreakCounterProps {
  currentStreak: number;
  longestStreak: number;
}

export default function StreakCounter({ currentStreak, longestStreak }: StreakCounterProps) {
  return (
    <div className="bg-gradient-to-br from-orange-50 to-orange-100 rounded-2xl p-5 border border-orange-200">
      <div className="flex items-center justify-between">
        <div className="flex items-center">
          <div className="w-12 h-12 bg-orange-500 text-white rounded-full flex items-center justify-center mr-4 shadow-sm shadow-orange-200">
            <Flame size={24} />
          </div>
          <div>
            <div className="text-sm text-orange-800 font-medium">Consecutive QC Passes</div>
            <div className="text-2xl font-black text-orange-950">{currentStreak} <span className="text-base font-normal">Prints</span></div>
          </div>
        </div>
        
        <div className="text-right">
          <div className="text-xs text-orange-700 font-medium mb-1">Best</div>
          <div className="font-bold text-orange-900 bg-white/60 px-2 py-1 rounded text-sm">{longestStreak}</div>
        </div>
      </div>
      
      {currentStreak > 0 && (
        <div className="mt-4 text-sm text-orange-800 bg-white/40 p-2 rounded-lg text-center">
          🔥 Keep it up! 10% bonus payout on next job!
        </div>
      )}
    </div>
  );
}
