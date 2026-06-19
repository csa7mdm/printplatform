import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams, useNavigate } from 'react-router-dom';
import { CreditCard, Smartphone, Banknote, Landmark } from 'lucide-react';
import { useQuoteDetails, useAcceptQuote, useInitiatePayment } from '../api/hooks';
import { PaymentMethod } from '../api/types';

export default function Checkout() {
  const { orderId } = useParams<{ orderId: string }>(); // orderId is actually the quoteId
  const { t } = useTranslation();
  const navigate = useNavigate();
  
  const [method, setMethod] = useState<'cod' | 'instapay' | 'vodafone' | 'card'>('card');
  const [recipientName, setRecipientName] = useState('');
  const [phone, setPhone] = useState('');
  const [street, setStreet] = useState('');
  const [city, setCity] = useState('Cairo');
  const [governorate, setGovernorate] = useState('Cairo');

  const { data: quote, isLoading } = useQuoteDetails(orderId!, { enabled: !!orderId });
  const acceptQuoteMutation = useAcceptQuote();
  const initiatePaymentMutation = useInitiatePayment();

  const handleConfirm = async () => {
    if (!orderId || !recipientName || !phone || !street) {
      alert('Please fill in all shipping address fields.');
      return;
    }

    const paymentMethodMap = {
      cod: PaymentMethod.COD,
      instapay: PaymentMethod.InstaPay,
      vodafone: PaymentMethod.VodafoneCash,
      card: PaymentMethod.Card,
    };

    const shippingAddress = {
      recipientName,
      phone,
      street,
      city,
      governorate,
      country: 'Egypt',
    };

    try {
      const order = await acceptQuoteMutation.mutateAsync({
        quoteId: orderId,
        paymentMethod: paymentMethodMap[method],
        shippingAddress,
      });

      if (method === 'card') {
        const nameParts = recipientName.split(' ');
        const firstName = nameParts[0] || 'Customer';
        const lastName = nameParts.slice(1).join(' ') || 'User';

        const paymentRes = await initiatePaymentMutation.mutateAsync({
          orderId: order.id,
          customerFirstName: firstName,
          customerLastName: lastName,
          customerEmail: 'customer@example.com', // fallback
          customerPhone: phone,
        });

        if (paymentRes.iframeUrl) {
          alert('Redirecting to Paymob Gateway...');
          window.location.href = paymentRes.iframeUrl;
        } else {
          navigate(`/orders/${order.id}`);
        }
      } else {
        alert('Order created successfully!');
        navigate(`/orders/${order.id}`);
      }
    } catch (err: any) {
      alert(err.response?.data?.detail || 'An error occurred during checkout.');
    }
  };

  if (isLoading) {
    return (
      <div className="max-w-4xl mx-auto py-12 text-center text-gray-500">
        Loading checkout details...
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto grid grid-cols-1 lg:grid-cols-3 gap-8">
      <div className="lg:col-span-2 space-y-6">
        <h1 className="text-3xl font-bold">{t('order.checkoutTitle')}</h1>
        
        {/* Shipping Address Form */}
        <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 space-y-4">
          <h2 className="text-xl font-semibold border-b pb-2">Shipping Address</h2>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="col-span-2">
              <label className="block text-sm font-medium mb-1">Recipient Full Name</label>
              <input 
                type="text" 
                value={recipientName}
                onChange={(e) => setRecipientName(e.target.value)}
                className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
                placeholder="Mahmoud Ahmed"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Phone Number</label>
              <input 
                type="text" 
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
                placeholder="01012345678"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Street Details</label>
              <input 
                type="text" 
                value={street}
                onChange={(e) => setStreet(e.target.value)}
                className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
                placeholder="12 El Horreya St."
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">City</label>
              <input 
                type="text" 
                value={city}
                onChange={(e) => setCity(e.target.value)}
                className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Governorate</label>
              <input 
                type="text" 
                value={governorate}
                onChange={(e) => setGovernorate(e.target.value)}
                className="w-full border rounded-md px-3 py-2 outline-none focus:ring-2 focus:ring-brand" 
              />
            </div>
          </div>
        </div>

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
          {quote && (
            <div className="space-y-3 mb-6 pb-6 border-b">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Printing Cost</span>
                <span className="font-medium">EGP {quote.customerPriceEgp.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Design Services</span>
                <span className="font-medium">EGP {quote.designServicePriceEgp.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Delivery Fee</span>
                <span className="font-medium">EGP {quote.deliveryEstimateEgp.toFixed(2)}</span>
              </div>
            </div>
          )}
          <div className="flex justify-between text-xl font-bold mb-6">
            <span>Total</span>
            <span className="text-brand">EGP {quote ? quote.totalPriceEgp.toFixed(2) : '0.00'}</span>
          </div>
          <button 
            onClick={handleConfirm}
            disabled={acceptQuoteMutation.isPending || initiatePaymentMutation.isPending}
            className="w-full bg-brand text-white py-3 rounded-lg font-bold hover:bg-blue-600 transition disabled:opacity-50"
          >
            {acceptQuoteMutation.isPending || initiatePaymentMutation.isPending
              ? 'Processing...'
              : t('order.confirmOrder')}
          </button>
        </div>
      </div>
    </div>
  );
}

