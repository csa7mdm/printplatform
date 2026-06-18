interface LeaderboardRowProps {
  rank: number;
  name: string;
  score: number;
  isSelf?: boolean;
}

export default function LeaderboardRow({ rank, name, score, isSelf = false }: LeaderboardRowProps) {
  return (
    <div className={`flex items-center p-3 rounded-xl ${
      isSelf ? 'bg-primary/5 border border-primary/20' : 'bg-white border border-slate-50'
    }`}>
      <div className={`w-8 text-center font-bold mr-3 ${
        rank === 1 ? 'text-yellow-500 text-xl' :
        rank === 2 ? 'text-slate-400 text-lg' :
        rank === 3 ? 'text-amber-600 text-lg' :
        'text-slate-400'
      }`}>
        #{rank}
      </div>
      <div className="flex-1">
        <div className={`font-medium ${isSelf ? 'text-primary' : 'text-slate-900'}`}>
          {name} {isSelf && '(You)'}
        </div>
      </div>
      <div className="font-bold text-slate-900">
        {score} <span className="text-xs text-slate-500 font-normal">pts</span>
      </div>
    </div>
  );
}
