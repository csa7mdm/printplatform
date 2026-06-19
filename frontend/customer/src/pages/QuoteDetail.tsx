import { useTranslation } from 'react-i18next';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuoteDetails } from '../api/hooks';
import { QuoteStatus } from '../api/types';

export default function QuoteDetail() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();
  const navigate = useNavigate();

  const { data: quote, isLoading, error } = useQuoteDetails(id!, {
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="max-w-3xl mx-auto py-12 text-center text-gray-500">
        Loading quote details...
      </div>
    );
  }

  if (error || !quote) {
    return (
      <div className="max-w-3xl mx-auto p-6 bg-white rounded-xl shadow-sm border border-gray-100 text-center space-y-4">
        <h2 className="text-xl font-semibold text-amber-700">Awaiting Pricing</h2>
        <p className="text-gray-600 text-sm max-w-md mx-auto">
          Our team is currently reviewing your 3D model and print settings. You will receive a notification on WhatsApp once the final price is set.
        </p>
        <button 
          onClick={() => navigate('/quotes')}
          className="bg-brand text-white px-6 py-2 rounded-md font-medium"
        >
          Back to Quotes
        </button>
      </div>
    );
  }

  const isPriced = quote.status === QuoteStatus.Sent || quote.status === QuoteStatus.Draft;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quote Details</h1>
          <p className="text-gray-500 mt-1">Request #{quote.quoteRequestId.slice(0, 8)}</p>
        </div>
        <span className={`px-3 py-1 rounded-full text-sm font-semibold ${
          quote.status === QuoteStatus.Accepted 
            ? 'bg-blue-100 text-blue-800' 
            : 'bg-green-100 text-green-800'
        }`}>
          {quote.status === QuoteStatus.Accepted ? 'Accepted' : t('quote.statusPriced')}
        </span>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 space-y-4">
          <h2 className="font-semibold text-lg border-b pb-2">Specifications</h2>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-gray-500">Estimated Weight</span><span className="font-medium">{quote.estimatedWeightGrams}g</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Estimated Print Hours</span><span className="font-medium">{quote.estimatedPrintHours}h</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Valid Until</span><span className="font-medium">{new Date(quote.validUntil).toLocaleDateString()}</span></div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 space-y-4">
          <h2 className="font-semibold text-lg border-b pb-2">Operator Note</h2>
          <p className="text-gray-700 text-sm leading-relaxed">{quote.operatorNotes || 'No notes provided by operator.'}</p>
        </div>
      </div>

      <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <h2 className="font-semibold text-lg border-b pb-4 mb-4">Pricing Breakdown</h2>
        <div className="space-y-3 max-w-sm ml-auto">
          <div className="flex justify-between text-gray-600"><span>Model Printing Price</span><span>EGP {quote.customerPriceEgp.toFixed(2)}</span></div>
          <div className="flex justify-between text-gray-600"><span>Design Service Fee</span><span>EGP {quote.designServicePriceEgp.toFixed(2)}</span></div>
          <div className="flex justify-between text-gray-600"><span>Delivery Fee Estimate</span><span>EGP {quote.deliveryEstimateEgp.toFixed(2)}</span></div>
          <div className="flex justify-between text-xl font-bold pt-3 border-t"><span>Total</span><span className="text-brand">EGP {quote.totalPriceEgp.toFixed(2)}</span></div>
        </div>
      </div>

      {isPriced && (
        <div className="flex justify-end gap-4 pt-4">
          <button 
            onClick={() => navigate('/quotes')}
            className="px-6 py-2 border border-red-200 text-red-600 rounded-md font-medium hover:bg-red-50 transition"
          >
            {t('quote.decline')}
          </button>
          <button 
            onClick={() => navigate(`/checkout/${quote.id}`)}
            className="bg-brand text-white px-8 py-2 rounded-md font-bold hover:bg-blue-600 transition"
          >
            {t('quote.accept')}
          </button>
        </div>
      )}
    </div>
  );
}

