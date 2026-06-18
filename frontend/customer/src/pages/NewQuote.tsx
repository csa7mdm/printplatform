import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FileUploader } from '../components/FileUploader';
import { useNavigate } from 'react-router-dom';

export default function NewQuote() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [file, setFile] = useState<File | null>(null);
  const [material, setMaterial] = useState('PLA');
  const [color, setColor] = useState('White');
  const [quality, setQuality] = useState('Standard');
  const [quantity, setQuantity] = useState(1);
  const [needsDesign, setNeedsDesign] = useState(false);
  const [step, setStep] = useState(1);

  const materials = ['PLA', 'PETG', 'ABS', 'TPU', 'Resin'];
  const colors = ['White', 'Black', 'Grey', 'Red', 'Blue', 'Green'];
  const qualities = ['Draft', 'Standard', 'Fine', 'Ultra'];

  const handleSubmit = () => {
    // In a real app, this submits to the API
    console.log('Submitting quote', { file, material, color, quality, quantity, needsDesign });
    // Navigate to a simulated detail page or back to list
    navigate('/quotes/123');
  };

  return (
    <div className="max-w-3xl mx-auto space-y-8">
      <h1 className="text-3xl font-bold">{t('quote.newTitle')}</h1>

      <div className="flex items-center gap-4 mb-8">
        <div className={`flex-1 h-2 rounded-full ${step >= 1 ? 'bg-brand' : 'bg-gray-200'}`} />
        <div className={`flex-1 h-2 rounded-full ${step >= 2 ? 'bg-brand' : 'bg-gray-200'}`} />
        <div className={`flex-1 h-2 rounded-full ${step >= 3 ? 'bg-brand' : 'bg-gray-200'}`} />
      </div>

      {step === 1 && (
        <div className="space-y-6 bg-white p-6 rounded-xl shadow-sm border border-gray-100">
          <h2 className="text-xl font-semibold">{t('quote.uploadStep')}</h2>
          <FileUploader onFileSelect={setFile} />
          
          <div className="flex justify-end pt-4">
            <button 
              disabled={!file}
              onClick={() => setStep(2)}
              className="bg-brand text-white px-6 py-2 rounded-md font-medium disabled:opacity-50"
            >
              Next Step
            </button>
          </div>
        </div>
      )}

      {step === 2 && (
        <div className="space-y-6 bg-white p-6 rounded-xl shadow-sm border border-gray-100">
          <h2 className="text-xl font-semibold">{t('quote.materialStep')}</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label className="block text-sm font-medium mb-2">{t('quote.material')}</label>
              <select value={material} onChange={(e) => setMaterial(e.target.value)} className="w-full border rounded-md p-2">
                {materials.map(m => <option key={m} value={m}>{m}</option>)}
              </select>
            </div>
            
            <div>
              <label className="block text-sm font-medium mb-2">{t('quote.color')}</label>
              <select value={color} onChange={(e) => setColor(e.target.value)} className="w-full border rounded-md p-2">
                {colors.map(c => <option key={c} value={c}>{c}</option>)}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">{t('quote.quality')}</label>
              <div className="grid grid-cols-2 gap-2">
                {qualities.map(q => (
                  <button 
                    key={q} 
                    onClick={() => setQuality(q)}
                    className={`p-2 border rounded-md text-sm font-medium ${quality === q ? 'bg-brand/10 border-brand text-brand' : 'hover:bg-gray-50'}`}
                  >
                    {q}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium mb-2">{t('quote.quantity')}</label>
              <input 
                type="number" 
                min="1" 
                value={quantity} 
                onChange={(e) => setQuantity(Number(e.target.value))} 
                className="w-full border rounded-md p-2"
              />
            </div>
          </div>

          <div className="flex items-center gap-2 mt-4 pt-4 border-t">
            <input 
              type="checkbox" 
              id="design" 
              checked={needsDesign} 
              onChange={(e) => setNeedsDesign(e.target.checked)} 
              className="w-4 h-4 text-brand rounded focus:ring-brand"
            />
            <label htmlFor="design" className="font-medium text-gray-700">{t('quote.designService')}</label>
          </div>

          <div className="flex justify-between pt-4">
            <button onClick={() => setStep(1)} className="text-gray-600 px-4 py-2 border rounded-md font-medium hover:bg-gray-50">Back</button>
            <button onClick={() => setStep(3)} className="bg-brand text-white px-6 py-2 rounded-md font-medium">Next Step</button>
          </div>
        </div>
      )}

      {step === 3 && (
        <div className="space-y-6 bg-white p-6 rounded-xl shadow-sm border border-gray-100">
          <h2 className="text-xl font-semibold">Review & Estimate</h2>
          
          <div className="bg-gray-50 p-4 rounded-lg space-y-3">
            <div className="flex justify-between"><span className="text-gray-500">File</span><span className="font-medium">{file?.name}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Material</span><span className="font-medium">{material} - {color}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Quality</span><span className="font-medium">{quality}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Quantity</span><span className="font-medium">{quantity}x</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Design Services</span><span className="font-medium">{needsDesign ? 'Yes' : 'No'}</span></div>
          </div>

          <div className="border-t pt-4">
            <div className="flex justify-between items-center mb-6">
              <span className="text-lg font-bold">Estimated Cost</span>
              <span className="text-2xl font-bold text-brand">~ EGP 450.00</span>
            </div>
          </div>

          <div className="flex justify-between pt-4">
            <button onClick={() => setStep(2)} className="text-gray-600 px-4 py-2 border rounded-md font-medium hover:bg-gray-50">Back</button>
            <button onClick={handleSubmit} className="bg-brand text-white px-8 py-3 rounded-lg font-bold hover:bg-blue-600">Submit Request</button>
          </div>
        </div>
      )}
    </div>
  );
}
