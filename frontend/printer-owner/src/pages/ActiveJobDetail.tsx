import { useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ArrowLeft, CheckCircle2, Circle, Clock, MapPin, Download, ChevronRight } from 'lucide-react';
import PhotoUploader from '../components/PhotoUploader';

type JobStatus = 'accepted' | 'printing' | 'photos_uploaded' | 'qc_review' | 'done';

export default function ActiveJobDetail() {
  const navigate = useNavigate();
  const { id } = useParams();
  
  const [status, setStatus] = useState<JobStatus>('accepted');

  const steps: { key: JobStatus; label: string; desc: string }[] = [
    { key: 'accepted', label: 'Job Accepted', desc: 'Download file and prep printer' },
    { key: 'printing', label: 'Printing', desc: 'Job is currently in progress' },
    { key: 'photos_uploaded', label: 'Photos Uploaded', desc: 'Pending QC submission' },
    { key: 'qc_review', label: 'QC Review', desc: 'Awaiting customer/admin approval' },
    { key: 'done', label: 'Completed', desc: 'Ready for shipping/pickup' }
  ];

  const currentStepIndex = steps.findIndex(s => s.key === status);

  const handleStartPrinting = () => setStatus('printing');
  const handlePhotosUploaded = (files: File[]) => setStatus('qc_review'); // Skips photos_uploaded state visually to review

  return (
    <div className="min-h-screen bg-slate-50 pb-24">
      {/* Header */}
      <div className="bg-white px-4 py-3 flex items-center shadow-sm sticky top-0 z-10">
        <button onClick={() => navigate('/home')} className="p-2 -ml-2 text-slate-600 hover:bg-slate-100 rounded-full">
          <ArrowLeft size={20} />
        </button>
        <h1 className="text-lg font-bold text-slate-900 ml-2">Active Job #{id || '1'}</h1>
      </div>

      <div className="p-4 max-w-lg mx-auto space-y-6">
        
        {/* Status Stepper */}
        <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6">
          <div className="space-y-6">
            {steps.map((step, index) => {
              const isCompleted = index < currentStepIndex;
              const isCurrent = index === currentStepIndex;
              
              return (
                <div key={step.key} className="flex relative">
                  {/* Vertical Line */}
                  {index < steps.length - 1 && (
                    <div className={`absolute top-8 left-3 w-0.5 h-full -ml-[1px] ${isCompleted ? 'bg-primary' : 'bg-slate-200'}`} />
                  )}
                  
                  <div className="flex-shrink-0 z-10">
                    {isCompleted ? (
                      <CheckCircle2 className="text-primary bg-white" size={24} />
                    ) : isCurrent ? (
                      <div className="w-6 h-6 rounded-full border-4 border-primary bg-white flex items-center justify-center">
                        <div className="w-2 h-2 rounded-full bg-primary animate-pulse" />
                      </div>
                    ) : (
                      <Circle className="text-slate-300 bg-white" size={24} />
                    )}
                  </div>
                  
                  <div className={`ml-4 ${isCurrent ? 'opacity-100' : isCompleted ? 'opacity-70' : 'opacity-40'}`}>
                    <h3 className={`font-bold ${isCurrent ? 'text-primary' : 'text-slate-900'}`}>{step.label}</h3>
                    <p className="text-sm text-slate-500 mt-0.5">{step.desc}</p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {/* Action Area based on status */}
        {status === 'accepted' && (
          <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-4 space-y-4">
            <button className="w-full py-3 px-4 bg-primary/10 text-primary border border-primary/20 rounded-lg flex items-center justify-center font-medium hover:bg-primary/20 transition-colors">
              <Download size={18} className="mr-2" /> Download .gcode
            </button>
            <button 
              onClick={handleStartPrinting}
              className="w-full py-3.5 bg-primary text-white rounded-lg font-bold shadow-sm hover:bg-primary/90"
            >
              Mark as Started Printing
            </button>
          </div>
        )}

        {status === 'printing' && (
          <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6">
            <h3 className="font-bold text-slate-900 mb-4">Upload Completion Photos</h3>
            <p className="text-sm text-slate-500 mb-6">Take clear photos of the finished print from multiple angles to pass Quality Control.</p>
            <PhotoUploader minPhotos={3} onUpload={handlePhotosUploaded} />
          </div>
        )}

        {status === 'qc_review' && (
          <div className="bg-warning/10 border border-warning/20 rounded-xl p-6 text-center">
            <Clock size={32} className="text-warning mx-auto mb-3" />
            <h3 className="font-bold text-warning-900 mb-2">Under Quality Review</h3>
            <p className="text-sm text-warning-800">Our team and the customer are reviewing your photos. You will be notified once approved.</p>
            <button onClick={() => setStatus('done')} className="mt-4 text-xs underline text-warning-700">(Dev: Mock Approve)</button>
          </div>
        )}

        {status === 'done' && (
          <div className="bg-success/10 border border-success/20 rounded-xl p-6">
            <div className="flex items-center text-success-700 font-bold mb-4">
              <CheckCircle2 size={24} className="mr-2" /> QC Approved!
            </div>
            
            <h3 className="font-bold text-slate-900 mb-2">Customer Details Revealed</h3>
            <div className="bg-white p-4 rounded-lg border border-success/20 space-y-3">
              <div className="flex items-start">
                <MapPin size={18} className="text-slate-400 mr-2 mt-0.5" />
                <div>
                  <div className="font-medium text-slate-900">Ahmed Hassan</div>
                  <div className="text-sm text-slate-500">123 Maadi St, Degla, Cairo</div>
                </div>
              </div>
              <button className="w-full py-2 bg-slate-900 text-white rounded-lg text-sm font-medium mt-2">
                Generate Shipping Label
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  );
}
