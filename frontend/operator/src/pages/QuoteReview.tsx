import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import ModelPreview3D from '../components/ModelPreview3D';
import { useConfirmQuote } from '../api/hooks';

export default function QuoteReview() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const confirmMutation = useConfirmQuote();
  
  const [formData, setFormData] = useState({
    weightGrams: '150',
    timeHours: '4.5',
    priceEGP: '450',
    deliveryDate: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    notes: ''
  });

  const handleConfirm = () => {
    if (!id) return;
    confirmMutation.mutate({
      quoteRequestId: id,
      overrideCustomerPriceEgp: Number(formData.priceEGP),
      operatorNotes: formData.notes,
      overrideValidUntil: new Date(formData.deliveryDate).toISOString(),
    }, {
      onSuccess: () => {
        alert('Quote confirmed successfully!');
        navigate('/quotes');
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Failed to confirm quote.');
      }
    });
  };

  const handleReject = () => {
    // Navigate back to queue for reject action
    navigate('/quotes');
  };


  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h2 className="text-2xl font-bold text-gray-900">Review Quote: {id}</h2>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm flex flex-col">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Model Preview</h3>
          <div className="flex-1 bg-gray-50 rounded-lg min-h-[400px]">
            <ModelPreview3D url="#" />
          </div>
          <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-gray-500">Material:</span>
              <p className="font-medium">PLA (White)</p>
            </div>
            <div>
              <span className="text-gray-500">Quantity:</span>
              <p className="font-medium">10</p>
            </div>
          </div>
        </div>

        <div className="bg-white p-6 rounded-lg border border-gray-200 shadow-sm">
          <h3 className="text-lg font-medium text-gray-900 mb-4">Confirm Details</h3>
          
          <div className="bg-blue-50 border border-blue-100 rounded-md p-4 mb-6">
            <h4 className="text-sm font-medium text-blue-800 mb-1">Auto-Estimate</h4>
            <p className="text-sm text-blue-600">The system estimated 145g and 4.2h based on the model volume.</p>
          </div>

          <form className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700">Weight (grams)</label>
                <input
                  type="number"
                  value={formData.weightGrams}
                  onChange={e => setFormData({...formData, weightGrams: e.target.value})}
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700">Time (hours)</label>
                <input
                  type="number"
                  step="0.1"
                  value={formData.timeHours}
                  onChange={e => setFormData({...formData, timeHours: e.target.value})}
                  className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Final Price (EGP)</label>
              <input
                type="number"
                value={formData.priceEGP}
                onChange={e => setFormData({...formData, priceEGP: e.target.value})}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Delivery Date</label>
              <input
                type="date"
                value={formData.deliveryDate}
                onChange={e => setFormData({...formData, deliveryDate: e.target.value})}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700">Operator Notes (Internal)</label>
              <textarea
                rows={3}
                value={formData.notes}
                onChange={e => setFormData({...formData, notes: e.target.value})}
                className="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
              />
            </div>

            <div className="pt-4 flex gap-3">
              <button
                type="button"
                disabled={confirmMutation.isPending}
                onClick={handleConfirm}
                className="flex-1 bg-primary-600 text-white py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
              >
                {confirmMutation.isPending ? 'Confirming...' : 'Confirm Quote'}
              </button>
              <button
                type="button"
                onClick={handleReject}
                className="flex-1 bg-white text-red-600 py-2 px-4 border border-red-300 rounded-md shadow-sm text-sm font-medium hover:bg-red-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
              >
                Reject
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}