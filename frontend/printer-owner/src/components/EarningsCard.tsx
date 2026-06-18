import { Wallet } from 'lucide-react';

interface EarningsCardProps {
  title: string;
  amount: number;
  currency?: string;
  subtitle?: string;
  isHighlight?: boolean;
}

export default function EarningsCard({ title, amount, currency = 'EGP', subtitle, isHighlight = false }: EarningsCardProps) {
  return (
    <div className={`p-5 rounded-2xl ${isHighlight ? 'bg-slate-900 text-white shadow-md' : 'bg-white text-slate-900 border border-slate-100 shadow-sm'}`}>
      <div className="flex items-center justify-between mb-4">
        <h3 className={`font-medium ${isHighlight ? 'text-slate-300' : 'text-slate-500'}`}>{title}</h3>
        <div className={`p-2 rounded-lg ${isHighlight ? 'bg-white/10' : 'bg-primary/10'}`}>
          <Wallet size={20} className={isHighlight ? 'text-white' : 'text-primary'} />
        </div>
      </div>
      <div className="flex items-baseline">
        <span className="text-3xl font-bold">{amount}</span>
        <span className={`ml-2 text-sm font-medium ${isHighlight ? 'text-slate-400' : 'text-slate-500'}`}>{currency}</span>
      </div>
      {subtitle && (
        <div className={`mt-2 text-sm ${isHighlight ? 'text-slate-400' : 'text-success font-medium'}`}>
          {subtitle}
        </div>
      )}
    </div>
  );
}
