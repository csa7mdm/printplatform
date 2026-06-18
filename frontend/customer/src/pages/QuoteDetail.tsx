import { useTranslation } from 'react-i18next';
import { useParams, useNavigate } from 'react-router-dom';

export default function QuoteDetail() {
  const { id } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();

  // Mock data for a Priced quote
  const quote = {
    id,
    fileName: 'custom_case.3mf',
    status: 'Priced',
    date: '2026-06-17',
    specs: {
      material: 'PETG',
      color: 'Black',
      quality: 'Fine',
      quantity: 2
    },
    pricing: {
      subtotal: 300,
      tax: 42,
      total: 342,
      currency: 'EGP'
    },
    message: 'We checked the file, it is perfectly printable. Price includes minor supports removal.'
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Quote #{quote.id}</h1>
          <p className="text-gray-500 mt-1">{quote.fileName}</p>
        </div>
        <span className="px-3 py-1 rounded-full bg-green-100 text-green-800 text-sm font-semibold">
          {t('quote.statusPriced')}
        </span>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 space-y-4">
          <h2 className="font-semibold text-lg border-b pb-2">Specifications</h2>
          <div className="space-y-2 text-sm">
            <div className="flex justify-between"><span className="text-gray-500">Material</span><span className="font-medium">{quote.specs.material}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Color</span><span className="font-medium">{quote.specs.color}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Quality</span><span className="font-medium">{quote.specs.quality}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Quantity</span><span className="font-medium">{quote.specs.quantity}</span></div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 space-y-4">
          <h2 className="font-semibold text-lg border-b pb-2">Operator Note</h2>
          <p className="text-gray-700 text-sm leading-relaxed">{quote.message}</p>
        </div>
      </div>

      <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
        <h2 className="font-semibold text-lg border-b pb-4 mb-4">Pricing Breakdown</h2>
        <div className="space-y-3 max-w-sm ml-auto">
          <div className="flex justify-between text-gray-600"><span>Subtotal</span><span>{quote.pricing.currency} {quote.pricing.subtotal}</span></div>
          <div className="flex justify-between text-gray-600"><span>Taxes (14%)</span><span>{quote.pricing.currency} {quote.pricing.tax}</span></div>
          <div className="flex justify-between text-xl font-bold pt-3 border-t"><span>Total</span><span className="text-brand">{quote.pricing.currency} {quote.pricing.total}</span></div>
        </div>
      </div>

      {quote.status === 'Priced' && (
        <div className="flex justify-end gap-4 pt-4">
          <button className="px-6 py-2 border border-red-200 text-red-600 rounded-md font-medium hover:bg-red-50 transition">
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
