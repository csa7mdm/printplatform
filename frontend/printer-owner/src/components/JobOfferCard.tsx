import { Link } from 'react-router-dom';
import { Package, Clock, Banknote, AlertCircle } from 'lucide-react';
import CountdownTimer from './CountdownTimer';

interface JobOfferProps {
  id: string;
  material: string;
  weight: number; // grams
  expiresAt: string;
  payoutAmount: number;
  currency?: string;
  onAccept: (id: string) => void;
  onDecline: (id: string) => void;
}

export default function JobOfferCard({
  id,
  material,
  weight,
  expiresAt,
  payoutAmount,
  currency = 'EGP',
  onAccept,
  onDecline
}: JobOfferProps) {
  return (
    <div className="bg-white rounded-xl shadow-md border-2 border-primary/20 overflow-hidden relative">
      {/* Top Urgent Bar */}
      <div className="bg-primary/10 px-4 py-2 flex justify-between items-center border-b border-primary/10">
        <div className="flex items-center text-primary font-medium text-sm">
          <AlertCircle size={16} className="mr-1.5" />
          New Job Offer!
        </div>
        <div className="flex items-center text-sm">
          <Clock size={14} className="mr-1 text-slate-500" />
          <CountdownTimer expiresAt={expiresAt} />
        </div>
      </div>

      <Link to={`/jobs/offered/${id}`} className="block p-4">
        <div className="flex justify-between items-start mb-4">
          <div>
            <h3 className="font-bold text-slate-900 text-lg mb-1">Print Job #{id}</h3>
            <div className="flex items-center text-slate-600 text-sm">
              <Package size={14} className="mr-1" />
              {material} • ~{weight}g
            </div>
          </div>
          <div className="text-right">
            <div className="text-xs text-slate-500 uppercase tracking-wider font-semibold mb-1">Payout</div>
            <div className="text-success font-bold text-xl flex items-center justify-end">
              <Banknote size={18} className="mr-1" />
              {payoutAmount} {currency}
            </div>
          </div>
        </div>
      </Link>

      <div className="p-4 pt-0 flex gap-3">
        <button 
          onClick={(e) => { e.preventDefault(); onDecline(id); }}
          className="flex-1 py-2.5 rounded-lg font-medium border border-slate-300 text-slate-700 bg-white hover:bg-slate-50 active:bg-slate-100 transition-colors"
        >
          Decline
        </button>
        <button 
          onClick={(e) => { e.preventDefault(); onAccept(id); }}
          className="flex-1 py-2.5 rounded-lg font-medium bg-primary text-white hover:bg-primary/90 active:bg-primary/95 transition-colors shadow-sm"
        >
          Accept Job
        </button>
      </div>
    </div>
  );
}
