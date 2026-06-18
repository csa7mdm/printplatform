import LevelProgress from '../components/LevelProgress';
import StreakCounter from '../components/StreakCounter';
import BadgeGrid from '../components/BadgeGrid';
import LeaderboardRow from '../components/LeaderboardRow';
import { Target } from 'lucide-react';

export default function Achievements() {
  
  const badges = [
    { id: '1', name: 'First Print', description: 'Complete your first order', isUnlocked: true, icon: '🎉' },
    { id: '2', name: 'Fast Shipper', description: 'Ship 5 orders early', isUnlocked: true, icon: '⚡' },
    { id: '3', name: 'Flawless', description: '10 consecutive QC passes', isUnlocked: false, icon: '💎' },
    { id: '4', name: 'Night Owl', description: 'Accept a job after midnight', isUnlocked: true, icon: '🦉' },
    { id: '5', name: 'Volume King', description: 'Print > 5kg of material', isUnlocked: false, icon: '👑' },
    { id: '6', name: '5-Star', description: 'Get 5 perfect ratings', isUnlocked: false, icon: '⭐' },
  ];

  const leaderboard = [
    { rank: 1, name: 'Mostafa S.', score: 2450 },
    { rank: 2, name: 'Karim W.', score: 2100 },
    { rank: 3, name: 'Ahmed Hassan', score: 1850, isSelf: true },
    { rank: 4, name: 'Omar M.', score: 1720 },
    { rank: 5, name: 'Nour E.', score: 1500 },
  ];

  return (
    <div className="p-4 max-w-lg mx-auto pb-24 space-y-6">
      <div className="pt-2">
        <h1 className="text-2xl font-bold text-slate-900 mb-1">Rewards & Stats</h1>
        <p className="text-slate-500 text-sm">Level up to unlock higher payouts and priority jobs</p>
      </div>

      <LevelProgress 
        currentLevel="Certified"
        nextLevel="Expert"
        currentXP={1850}
        requiredXP={2500}
      />

      <StreakCounter currentStreak={4} longestStreak={12} />

      {/* Active Challenge */}
      <div className="bg-white rounded-2xl p-5 border border-primary/20 shadow-sm relative overflow-hidden">
        <div className="absolute top-0 right-0 p-4 opacity-10">
          <Target size={64} />
        </div>
        <h3 className="font-bold text-slate-900 flex items-center mb-2">
          <Target size={18} className="text-primary mr-2" /> Weekly Challenge
        </h3>
        <p className="text-sm text-slate-600 mb-4">Complete 5 orders this week to earn a 50 EGP bonus.</p>
        
        <div className="flex items-center justify-between text-sm font-bold text-slate-900 mb-1">
          <span>Progress</span>
          <span>3/5</span>
        </div>
        <div className="h-2 w-full bg-slate-100 rounded-full overflow-hidden">
          <div className="h-full bg-primary rounded-full w-3/5" />
        </div>
      </div>

      {/* Badges */}
      <div>
        <h3 className="font-bold text-slate-900 text-sm uppercase tracking-wider mb-3">Your Badges</h3>
        <BadgeGrid badges={badges} />
      </div>

      {/* Leaderboard */}
      <div>
        <h3 className="font-bold text-slate-900 text-sm uppercase tracking-wider mb-3">District Top 5 (Maadi)</h3>
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-2 space-y-1">
          {leaderboard.map(user => (
            <LeaderboardRow key={user.rank} {...user} />
          ))}
        </div>
      </div>
    </div>
  );
}
