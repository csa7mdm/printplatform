import { useTranslation } from 'react-i18next';
import { Award } from 'lucide-react';
import clsx from 'clsx';

export const LoyaltyTierBadge = ({ tier }: { tier: 'Bronze' | 'Silver' | 'Gold' | 'Platinum' }) => {
  const colors = {
    Bronze: 'bg-[#cd7f32] text-white',
    Silver: 'bg-gray-300 text-gray-800',
    Gold: 'bg-yellow-400 text-yellow-900',
    Platinum: 'bg-slate-800 text-white',
  };

  return (
    <div className={clsx('inline-flex items-center gap-1 px-3 py-1 rounded-full text-sm font-bold shadow-sm', colors[tier])}>
      <Award className="w-4 h-4" />
      <span>{tier}</span>
    </div>
  );
};

export default function Loyalty() {
  const { t } = useTranslation();

  return (
    <div className="max-w-4xl mx-auto space-y-8">
      <h1 className="text-3xl font-bold">{t('loyalty.title')}</h1>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col items-center justify-center space-y-4 text-center">
          <p className="text-gray-500 font-medium">{t('loyalty.tier')}</p>
          <LoyaltyTierBadge tier="Gold" />
          <p className="text-sm text-gray-400">1,200 points to Platinum</p>
        </div>
        
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 flex flex-col items-center justify-center space-y-2 text-center">
          <p className="text-gray-500 font-medium">{t('loyalty.points')}</p>
          <p className="text-5xl font-extrabold text-brand">4,850</p>
          <button className="mt-4 text-brand font-medium hover:underline">
            Redeem Rewards
          </button>
        </div>
      </div>

      <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <h2 className="text-xl font-bold mb-4">{t('loyalty.history')}</h2>
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <div key={i} className="flex justify-between items-center py-3 border-b last:border-0">
              <div>
                <p className="font-medium">Order #100{i}</p>
                <p className="text-sm text-gray-500">2 days ago</p>
              </div>
              <p className="text-green-600 font-bold">+150 pts</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
