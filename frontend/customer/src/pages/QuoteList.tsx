import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Clock, CheckCircle, AlertCircle } from 'lucide-react';
import { useQuotes } from '../api/hooks';
import { QuoteRequestStatus } from '../api/types';

export default function QuoteList() {
  const { t } = useTranslation();
  const { data: quotes, isLoading, error } = useQuotes();

  if (isLoading) {
    return (
      <div className="max-w-5xl mx-auto py-12 text-center text-gray-500">
        Loading quotes...
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-5xl mx-auto py-12 text-center text-red-500">
        Failed to load quotes. Please try again.
      </div>
    );
  }

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
          {quotes && quotes.map(quote => (
            <Link key={quote.quoteRequestId} to={`/quotes/${quote.quoteRequestId}`} className="block p-4 hover:bg-gray-50 transition">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-medium text-lg text-brand mb-1">
                    Model Request #{quote.modelFileId.slice(0, 8)}
                  </h3>
                  <div className="flex items-center gap-4 text-sm text-gray-500">
                    <span>ID: #{quote.quoteRequestId.slice(0, 8)}</span>
                    <span>{new Date(quote.createdAt).toLocaleDateString()}</span>
                  </div>
                </div>
                <div className="text-right">
                  {quote.status === QuoteRequestStatus.PendingReview ? (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-amber-100 text-amber-800 text-xs font-semibold">
                      <Clock className="w-3 h-3" />
                      {t('quote.statusPending')}
                    </span>
                  ) : quote.status === QuoteRequestStatus.Quoted ? (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-green-100 text-green-800 text-xs font-semibold">
                      <CheckCircle className="w-3 h-3" />
                      {t('quote.statusPriced')}
                    </span>
                  ) : quote.status === QuoteRequestStatus.Accepted ? (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-blue-100 text-blue-800 text-xs font-semibold">
                      <CheckCircle className="w-3 h-3" />
                      Accepted
                    </span>
                  ) : (
                    <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-gray-100 text-gray-800 text-xs font-semibold">
                      <AlertCircle className="w-3 h-3" />
                      {QuoteRequestStatus[quote.status] || 'Expired'}
                    </span>
                  )}
                </div>
              </div>
            </Link>
          ))}
          {(!quotes || quotes.length === 0) && (
            <div className="p-8 text-center text-gray-500">
              No quotes found.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

