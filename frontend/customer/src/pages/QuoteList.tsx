import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Clock, CheckCircle } from 'lucide-react';

export default function QuoteList() {
  const { t } = useTranslation();

  const quotes = [
    { id: '123', name: 'robot_arm_v2.stl', status: 'Pending', date: '2026-06-18', price: null },
    { id: '124', name: 'custom_case.3mf', status: 'Priced', date: '2026-06-17', price: 'EGP 320' },
  ];

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">{t('nav.quotes')}</h1>
        <Link to="/quotes/new" className="bg-brand text-white px-4 py-2 rounded-md font-medium text-sm hover:bg-blue-600 transition">
          {t('quote.newTitle')}
        </Link>
      </div>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="divide-y">
          {quotes.map(quote => (
            <Link key={quote.id} to={`/quotes/${quote.id}`} className="block p-4 hover:bg-gray-50 transition">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-medium text-lg text-brand mb-1">{quote.name}</h3>
                  <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span>ID: #{quote.id}</span>
                    <span>{quote.date}</span>
                  </div>
                </div>
                <div className="text-right">
                  {quote.status === 'Pending' ? (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-amber-100 text-amber-800 text-xs font-semibold">
                      <Clock className="w-3 h-3" />
                      {t('quote.statusPending')}
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-green-100 text-green-800 text-xs font-semibold">
                      <CheckCircle className="w-3 h-3" />
                      {t('quote.statusPriced')}
                    </span>
                  )}
                  {quote.price && <div className="mt-2 font-bold text-gray-900">{quote.price}</div>}
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
