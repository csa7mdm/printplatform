import { Link } from 'react-router-dom';
import StatusChip from '../components/StatusChip';
import { usePendingQuotes } from '../api/hooks';
import { QuoteRequestStatus } from '../api/types';

export default function Quotes() {
  const { data: quotes, isLoading, error } = usePendingQuotes();

  if (isLoading) {
    return (
      <div className="py-12 text-center text-gray-500">
        Loading pending quotes...
      </div>
    );
  }

  if (error) {
    return (
      <div className="py-12 text-center text-red-500">
        Failed to load pending quotes. Please try again.
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Quotes Queue</h2>
      </div>

      <div className="bg-white shadow-sm rounded-lg border border-gray-200 overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Request ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Customer ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Material Option ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Quantity</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Action</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {quotes && quotes.map((quote) => (
              <tr key={quote.quoteRequestId} className="hover:bg-gray-50">
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                  {quote.quoteRequestId.slice(0, 8)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {quote.customerUserId.slice(0, 8)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {quote.materialOptionId.slice(0, 8)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{quote.quantity}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {new Date(quote.createdAt).toLocaleDateString()}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <StatusChip status={quote.status === QuoteRequestStatus.PendingReview ? 'pending' : 'reviewed'} />
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                  <Link to={`/quotes/${quote.quoteRequestId}/review`} className="text-primary-600 hover:text-primary-900">
                    Open & Confirm
                  </Link>
                </td>
              </tr>
            ))}
            {(!quotes || quotes.length === 0) && (
              <tr>
                <td colSpan={7} className="px-6 py-8 text-center text-gray-500">
                  No pending quotes to review.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}