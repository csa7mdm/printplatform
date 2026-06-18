import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams, useNavigate } from 'react-router-dom';
import { CreditCard, Smartphone, Banknote, Landmark } from 'lucide-react';

export default function Checkout() {
  const { orderId } = useParams();
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [method, setMethod] = useState<'cod' | 'instapay' | 'vodafone' | 'card'>('card');

  const handleConfirm = () => {
    console.log(`Processing ${method} payment for order ${orderId}`);
    if (method === 'card') {
      // In real life, redirect to Paymob iframe URL or load it here
      alert('Redirecting to Paymob Gateway...');
    }
    navigate('/orders/999');
  };

  return (
    <div className="max-w-4xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div className="lg:col-span-2 space-y-6">
        <h1 className="text-3xl font-bold">{t('order.checkoutTitle')}</h1>
        
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
          <h2 className="text-xl font-semibold mb-4">{t('order.paymentMethod')}</h2>
          
          <div className="space-y-3">
            {[
              { id: 'card', icon: CreditCard, label: t('order.payCard') },
              { id: 'instapay', icon: Landmark, label: t('order.payInstaPay') },
              { id: 'vodafone', icon: Smartphone, label: t('order.payVodafone') },
              { id: 'cod', icon: Banknote, label: t('order.payCOD') },
            ].map(m => (
              <label 
                key={m.id} 
                className={`flex items-center p-4 border rounded-lg cursor-pointer transition ${method === m.id ? 'border-brand bg-brand/5 ring-1 ring-brand' : 'hover:bg-gray-50'}`}
              >
                <input 
                  type="radio" 
                  name="payment" 
                  value={m.id} 
                  checked={method === m.id} 
                  onChange={() => setMethod(m.id as any)} 
                  className="w-4 h-4 text-brand focus:ring-brand"
                />
                <m.icon className="w-6 h-6 text-gray-500 mx-4" />
                <span className="font-medium text-gray-900">{m.label}</span>
              </label>
            ))}
          </div>

          {method === 'instapay' && (
            <div className="mt-4 p-4 bg-blue-50 text-blue-800 rounded-md text-sm">
              Please transfer to instapay address: <strong>printplatform@instapay</strong> and upload the receipt on the next screen.
            </div>
          )}
          {method === 'vodafone' && (
            <div className="mt-4 p-4 bg-red-50 text-red-800 rounded-md text-sm">
              Please transfer to Vodafone Cash number: <strong>01001234567</strong>.
            </div>
          )}
        </div>
      </div>

      <div className="lg:col-span-1">
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 sticky top-24">
          <h2 className="text-xl font-semibold mb-4">Order Summary</h2>
          <div className="space-y-3 mb-6 pb-6 border-b">
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">custom_case.3mf</span>
              <span className="font-medium">EGP 300</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Shipping (Cairo)</span>
              <span className="font-medium">EGP 50</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-gray-500">Taxes</span>
              <span className="font-medium">EGP 42</span>
            </div>
          </div>
          <div className="flex justify-between text-xl font-bold mb-6">
            <span>Total</span>
            <span className="text-brand">EGP 392</span>
          </div>
          <button 
            onClick={handleConfirm}
            className="w-full bg-brand text-white py-3 rounded-lg font-bold hover:bg-blue-600 transition"
          >
            {t('order.confirmOrder')}
          </button>
        </div>
      </div>
    </div>
  );
}
