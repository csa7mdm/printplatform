import { useTranslation } from 'react-i18next';
import { Link } from 'react-router-dom';
import { Package, Truck } from 'lucide-react';

export default function OrderList() {
  const { t } = useTranslation();

  const orders = [
    { id: '999', name: 'robot_arm_v2.stl', status: 'In Production', date: '2026-06-18', price: 'EGP 392' },
    { id: '998', name: 'bracket.stl', status: 'Shipped', date: '2026-06-15', price: 'EGP 150', tracking: 'BOS-123456' },
  ];

  return (
    <div className="max-w-5xl mx-auto space-y-6">
      <h1 className="text-3xl font-bold">{t('nav.orders')}</h1>

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="divide-y">
          {orders.map(order => (
            <div key={order.id} className="p-4 hover:bg-gray-50 transition flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
              <div>
                <h3 className="font-medium text-lg text-brand mb-1">
                  <Link to={`/orders/${order.id}`} className="hover:underline">{order.name}</Link>
                </h3>
                <div className="flex items-center gap-4 text-sm text-gray-500">
                  <span>Order #{order.id}</span>
                  <span>{order.date}</span>
                </div>
              </div>
              
              <div className="flex flex-col items-end gap-2">
                <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-blue-100 text-blue-800 text-xs font-semibold">
                  <Package className="w-3 h-3" />
                  {order.status}
                </span>
                <span className="font-bold text-gray-900">{order.price}</span>
                {order.tracking && (
                  <a href="#" className="text-xs text-brand flex items-center gap-1 hover:underline">
                    <Truck className="w-3 h-3" />
                    Track: {order.tracking}
                  </a>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
