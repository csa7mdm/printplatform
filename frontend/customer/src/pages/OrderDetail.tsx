import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import { OrderStatusTimeline, OrderStatus as TimelineStatus } from '../components/OrderStatusTimeline';
import { useOrderDetails, useCancelOrder } from '../api/hooks';
import { OrderStatus } from '../api/types';

const mapBackendStatusToTimeline = (status: OrderStatus): TimelineStatus => {
  switch (status) {
    case OrderStatus.PendingPayment:
      return 'Pending';
    case OrderStatus.Confirmed:
      return 'Slicing';
    case OrderStatus.InProduction:
      return 'Printing';
    case OrderStatus.QCPending:
      return 'QC';
    case OrderStatus.ReadyToShip:
    case OrderStatus.Shipped:
      return 'Shipped';
    case OrderStatus.Delivered:
      return 'Delivered';
    default:
      return 'Pending';
  }
};

export default function OrderDetail() {
  const { id } = useParams<{ id: string }>();
  const { t } = useTranslation();

  const { data: order, isLoading, error } = useOrderDetails(id!, { enabled: !!id });
  const cancelOrderMutation = useCancelOrder();

  const handleCancel = () => {
    const reason = prompt('Please enter a reason for cancelling this order:');
    if (reason === null) return; // cancelled prompt
    
    cancelOrderMutation.mutate({
      orderId: id!,
      reason: reason || 'Cancelled by customer',
    }, {
      onSuccess: () => {
        alert('Order cancelled successfully.');
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Failed to cancel order.');
      }
    });
  };

  if (isLoading) {
    return (
      <div className="max-w-4xl mx-auto py-12 text-center text-gray-500">
        Loading order details...
      </div>
    );
  }

  if (error || !order) {
    return (
      <div className="max-w-4xl mx-auto py-12 text-center text-red-500">
        Failed to load order.
      </div>
    );
  }

  const timelineStatus = mapBackendStatusToTimeline(order.status);
  const isCancellable = order.status === OrderStatus.PendingPayment || order.status === OrderStatus.Confirmed;

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex justify-between items-end">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Order #{order.id.slice(0, 8)}</h1>
          <p className="text-gray-500 mt-1">Placed on {new Date(order.createdAt).toLocaleDateString()}</p>
        </div>
        <div className="text-right space-y-2">
          <div className="font-bold text-xl">EGP {order.totalEgp.toFixed(2)}</div>
          {isCancellable && (
            <button 
              onClick={handleCancel}
              disabled={cancelOrderMutation.isPending}
              className="text-sm border border-red-200 text-red-600 px-3 py-1.5 rounded-md hover:bg-red-50 disabled:opacity-50"
            >
              {cancelOrderMutation.isPending ? 'Cancelling...' : 'Cancel Order'}
            </button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        <div className="md:col-span-1 bg-white p-6 rounded-xl shadow-sm border border-gray-100 h-fit">
          <h2 className="font-semibold text-lg border-b pb-4 mb-6">{t('order.statusTimeline')}</h2>
          <OrderStatusTimeline currentStatus={timelineStatus} />
        </div>

        <div className="md:col-span-2 space-y-6">
          <div className="bg-white p-6 rounded-xl shadow-sm border border-gray-100">
            <h2 className="font-semibold text-lg border-b pb-4 mb-4">Details</h2>
            <div className="space-y-4">
              <div>
                <span className="text-sm text-gray-500 block">Shipping Address Snapshot</span>
                <p className="text-gray-700 font-medium text-sm whitespace-pre-wrap">{order.shippingAddressSnapshot}</p>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="text-gray-500">Subtotal:</span>
                  <span className="font-medium ml-2">EGP {order.subtotalEgp.toFixed(2)}</span>
                </div>
                <div>
                  <span className="text-gray-500">Delivery Fee:</span>
                  <span className="font-medium ml-2">EGP {order.deliveryFeeEgp.toFixed(2)}</span>
                </div>
                {order.discountEgp > 0 && (
                  <div>
                    <span className="text-gray-500">Discount:</span>
                    <span className="font-medium ml-2 text-green-600">-EGP {order.discountEgp.toFixed(2)}</span>
                  </div>
                )}
                {order.loyaltyDiscountEgp > 0 && (
                  <div>
                    <span className="text-gray-500">Loyalty Discount:</span>
                    <span className="font-medium ml-2 text-green-600">-EGP {order.loyaltyDiscountEgp.toFixed(2)}</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

