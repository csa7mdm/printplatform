import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Package } from 'lucide-react';
import { useOrders } from '../api/hooks';
import { OrderStatus } from '../api/types';

const orderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.PendingPayment]: 'Pending Payment',
  [OrderStatus.Confirmed]: 'Confirmed',
  [OrderStatus.InProduction]: 'In Production',
  [OrderStatus.QCPending]: 'Quality Control',
  [OrderStatus.ReadyToShip]: 'Ready to Ship',
  [OrderStatus.Shipped]: 'Shipped',
  [OrderStatus.Delivered]: 'Delivered',
  [OrderStatus.Cancelled]: 'Cancelled',
  [OrderStatus.Refunded]: 'Refunded',
};

export default function OrderList() {
  const { t } = useTranslation();
  const { data: orders, isLoading, error } = useOrders();

  if (isLoading) {
    return (
      <div className="max-w-5xl mx-auto py-12 text-center text-gray-500">
        Loading orders...
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-5xl mx-auto py-12 text-center text-red-500">
        Failed to load orders. Please try again.
      </div>
    );
  }

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <h1 className="text-3xl font-bold">{t('nav.orders')}</h1>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="divide-y">
          {orders && orders.map(order => (
            <div key={order.id} className="p-4 hover:bg-gray-50 transition flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
              <div>
                <h3 className="font-medium text-lg text-brand mb-1">
                  <Link to={`/orders/${order.id}`} className="hover:underline">Order #{order.id.slice(0, 8)}</Link>
                </h3>
                <div className="flex items-center gap-4 text-sm text-gray-500">
                  <span>Method: {order.paymentMethod === 3 ? 'Card' : 'Other'}</span>
                  <span>{new Date(order.createdAt).toLocaleDateString()}</span>
                </div>
              </div>
              
              <div className="flex flex-col items-end gap-2">
                <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-xs font-semibold ${
                  order.status === OrderStatus.Delivered 
                    ? 'bg-green-100 text-green-800' 
                    : order.status === OrderStatus.Cancelled 
                    ? 'bg-red-100 text-red-800' 
                    : 'bg-blue-100 text-blue-800'
                }`}>
                  <Package className="w-3 h-3" />
                  {orderStatusLabels[order.status]}
                </span>
                <span className="font-bold text-gray-900">EGP {order.totalEgp.toFixed(2)}</span>
              </div>
            </div>
          ))}
          {(!orders || orders.length === 0) && (
            <div className="p-8 text-center text-gray-500">
              No orders found.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

