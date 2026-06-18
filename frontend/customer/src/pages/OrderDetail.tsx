import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { OrderStatusTimeline, OrderStatus } from '../components/OrderStatusTimeline';

export default function OrderDetail() {
  const { id } = useParams();
  const { t } = useTranslation();

  // Mock data
  const order = {
    id,
    status: 'QC' as OrderStatus,
    date: '2026-06-18',
    total: 'EGP 392',
    trackingLink: null,
    qcPhotos: [
      'https://via.placeholder.com/400x300?text=QC+Photo+1',
      'https://via.placeholder.com/400x300?text=QC+Photo+2'
    ]
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Order #{order.id}</h1>
          <p className="text-gray-500 mt-1">Placed on {order.date}</p>
        </div>
        <span className="font-bold text-xl">{order.total}</span>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div className="md:col-span-1 bg-white p-6 rounded-xl shadow-sm border border-gray-100 h-fit">
          <h2 className="font-semibold text-lg border-b pb-4 mb-6">{t('order.statusTimeline')}</h2>
          <OrderStatusTimeline currentStatus={order.status} />
        </div>

        <div className="md:col-span-2 space-y-6">
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <h2 className="font-semibold text-lg border-b pb-4 mb-4">Details</h2>
            <p className="text-gray-600">Your model is currently undergoing Quality Control inspection. Our engineers ensure all dimensions and surface qualities meet our strict standards.</p>
          </div>

          {(order.status === 'QC' || order.status === 'Shipped' || order.status === 'Delivered') && order.qcPhotos && (
            <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
              <h2 className="font-semibold text-lg border-b pb-4 mb-4">{t('order.qcPhotos')}</h2>
              <div className="grid grid-cols-2 gap-4">
                {order.qcPhotos.map((src, idx) => (
                  <img key={idx} src={src} alt="QC Photo" className="w-full h-auto rounded-lg border" />
                ))}
              </div>
            </div>
          )}

          {order.status === 'Delivered' && (
            <div className="bg-brand/10 p-6 rounded-xl border border-brand/20 text-center space-y-4">
              <h2 className="font-bold text-lg text-brand">{t('order.rateReview')}</h2>
              <p className="text-gray-600 text-sm">How was your experience? Your feedback helps us improve.</p>
              <button className="bg-brand text-white px-6 py-2 rounded-md font-medium">Leave a Review</button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
