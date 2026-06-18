import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, Download, Package, Box, Scale, AlertCircle, Banknote, Clock } from 'lucide-react';
import CountdownTimer from '../components/CountdownTimer';

export default function JobOfferDetail() {
  const navigate = useNavigate();
  const { id } = useParams();

  // Mock data
  const offer = {
    id: id || 'JO-8832',
    material: 'PLA+ Black',
    weight: 120, // grams
    quantity: 2,
    dimensions: '150 x 100 x 50 mm',
    payoutAmount: 180,
    expiresAt: new Date(Date.now() + 2 * 60 * 60 * 1000).toISOString()
  };

  const handleAccept = () => {
    alert('Accepted! Moving to active jobs.');
    navigate('/jobs/active/1');
  };

  const handleDecline = () => {
    const reason = prompt('Please provide a reason for declining (e.g., Not enough material, busy):');
    if (reason) {
      alert('Declined.');
      navigate('/home');
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      {/* Header */}
      <div className="bg-white px-4 py-3 flex items-center shadow-sm sticky top-0 z-10">
        <button onClick={() => navigate(-1)} className="p-2 -ml-2 text-slate-600 hover:bg-slate-100 rounded-full">
          <ArrowLeft size={20} />
        </button>
        <h1 className="text-lg font-bold text-slate-900 ml-2">Job Offer Details</h1>
      </div>

      <div className="p-4 max-w-lg mx-auto space-y-6">
        
        {/* Urgent Alert */}
        <div className="bg-primary/10 rounded-xl p-4 flex items-center justify-between border border-primary/20">
          <div className="flex items-center text-primary font-medium">
            <AlertCircle size={20} className="mr-2" />
            Time to respond
          </div>
          <div className="text-lg text-primary font-bold">
            <CountdownTimer expiresAt={offer.expiresAt} />
          </div>
        </div>

        {/* Payout Summary */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6 text-center">
          <div className="text-sm text-slate-500 font-medium uppercase tracking-wider mb-1">Expected Payout</div>
          <div className="text-4xl font-bold text-success flex items-center justify-center">
            {offer.payoutAmount} <span className="text-2xl ml-1">EGP</span>
          </div>
        </div>

        {/* Details List */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden">
          <div className="p-4 border-b border-slate-100 font-bold text-slate-900">Print Specifications</div>
          
          <div className="p-4 space-y-4">
            <div className="flex items-center">
              <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 mr-4">
                <Package size={20} />
              </div>
              <div className="flex-1">
                <div className="text-sm text-slate-500">Material</div>
                <div className="font-medium text-slate-900">{offer.material}</div>
              </div>
            </div>

            <div className="flex items-center">
              <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 mr-4">
                <Scale size={20} />
              </div>
              <div className="flex-1">
                <div className="text-sm text-slate-500">Estimated Weight</div>
                <div className="font-medium text-slate-900">~{offer.weight} grams</div>
              </div>
            </div>

            <div className="flex items-center">
              <div className="w-10 h-10 rounded-full bg-slate-100 flex items-center justify-center text-slate-500 mr-4">
                <Box size={20} />
              </div>
              <div className="flex-1 flex justify-between items-center">
                <div>
                  <div className="text-sm text-slate-500">Quantity</div>
                  <div className="font-medium text-slate-900">{offer.quantity}x Pieces</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Files Area */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-4">
          <h3 className="font-bold text-slate-900 mb-3">Slicer File</h3>
          <button className="w-full py-3 px-4 bg-slate-50 border border-slate-200 rounded-lg flex items-center justify-between text-slate-700 hover:bg-slate-100 transition-colors">
            <span className="flex items-center font-medium">
              <Download size={18} className="mr-2 text-primary" />
              Download .gcode
            </span>
            <span className="text-sm text-slate-500">12.4 MB</span>
          </button>
        </div>

        {/* Actions Fixed Bottom */}
        <div className="fixed bottom-0 left-0 right-0 bg-white border-t border-slate-200 p-4 pb-safe z-20">
          <div className="max-w-lg mx-auto flex gap-3">
            <button 
              onClick={handleDecline}
              className="flex-1 py-3.5 rounded-xl font-medium border-2 border-slate-200 text-slate-700 bg-white hover:bg-slate-50 active:bg-slate-100 transition-colors"
            >
              Decline
            </button>
            <button 
              onClick={handleAccept}
              className="flex-[2] py-3.5 rounded-xl font-bold bg-primary text-white hover:bg-primary/90 active:bg-primary/95 transition-colors shadow-sm"
            >
              Accept Job Offer
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
