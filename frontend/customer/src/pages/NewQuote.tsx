import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { FileUploader } from '../components/FileUploader';
import { useNavigate } from 'react-router-dom';
import { useUploadModel, useMaterials, useCreateQuoteRequest } from '../api/hooks';
import { QualityPreset, ModelFileDto, MaterialType } from '../api/types';

const qualityMap: Record<string, QualityPreset> = {
  'Draft': QualityPreset.Draft,
  'Standard': QualityPreset.Standard,
  'Fine': QualityPreset.Fine,
  'Ultra': QualityPreset.Ultra,
};

const materialTypeLabels: Record<MaterialType, string> = {
  [MaterialType.PLA]: 'PLA',
  [MaterialType.PETG]: 'PETG',
  [MaterialType.ABS]: 'ABS',
  [MaterialType.TPU]: 'TPU',
  [MaterialType.Nylon]: 'Nylon',
  [MaterialType.PLA_CF]: 'PLA-CF',
  [MaterialType.Resin_Standard]: 'Standard Resin',
  [MaterialType.Resin_Dental]: 'Dental Resin',
  [MaterialType.Resin_Engineering]: 'Engineering Resin',
};

export default function NewQuote() {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const [file, setFile] = useState<File | null>(null);
  const [uploadedModel, setUploadedModel] = useState<ModelFileDto | null>(null);
  const [selectedMaterialOptionId, setSelectedMaterialOptionId] = useState('');
  const [quality, setQuality] = useState('Standard');
  const [quantity, setQuantity] = useState(1);
  const [needsDesign, setNeedsDesign] = useState(false);
  const [step, setStep] = useState(1);

  const uploadMutation = useUploadModel();
  const createQuoteMutation = useCreateQuoteRequest();
  const { data: materialsList } = useMaterials();

  const qualities = ['Draft', 'Standard', 'Fine', 'Ultra'];

  useEffect(() => {
    if (materialsList && materialsList.length > 0 && !selectedMaterialOptionId) {
      setSelectedMaterialOptionId(materialsList[0].id);
    }
  }, [materialsList, selectedMaterialOptionId]);

  const handleNextStep1 = () => {
    if (!file) return;
    uploadMutation.mutate(file, {
      onSuccess: (data) => {
        setUploadedModel(data);
        setStep(2);
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Failed to upload model file. Please try again.');
      }
    });
  };

  const handleSubmit = () => {
    if (!uploadedModel || !selectedMaterialOptionId) {
      alert('Missing uploaded model or selected material.');
      return;
    }

    createQuoteMutation.mutate({
      modelFileId: uploadedModel.id,
      materialOptionId: selectedMaterialOptionId,
      qualityPreset: qualityMap[quality] ?? QualityPreset.Standard,
      layerHeightMm: quality === 'Fine' || quality === 'Ultra' ? 0.12 : 0.2,
      infillPercent: 20,
      quantity,
      includeDesignService: needsDesign,
    }, {
      onSuccess: (quoteRequestId) => {
        alert('Quote request submitted successfully!');
        navigate(`/quotes/${quoteRequestId}`);
      },
      onError: (err: any) => {
        alert(err.response?.data?.detail || 'Failed to submit quote request. The model analysis might still be pending.');
      }
    });
  };

  const selectedMaterialOption = materialsList?.find(m => m.id === selectedMaterialOptionId);

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
              disabled={!file || uploadMutation.isPending}
              onClick={handleNextStep1}
              className="bg-brand text-white px-6 py-2 rounded-md font-medium disabled:opacity-50"
            >
              {uploadMutation.isPending ? 'Uploading...' : 'Next Step'}
            </button>
          </div>
        </div>
      )}

      {step === 2 && (
        <div className="space-y-6 bg-white p-6 rounded-xl shadow-sm border border-gray-100">
          <h2 className="text-xl font-semibold">{t('quote.materialStep')}</h2>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="col-span-2">
              <label className="block text-sm font-medium mb-2">{t('quote.material')}</label>
              <select 
                value={selectedMaterialOptionId} 
                onChange={(e) => setSelectedMaterialOptionId(e.target.value)} 
                className="w-full border rounded-md p-2 outline-none focus:ring-2 focus:ring-brand"
              >
                {materialsList?.map(m => (
                  <option key={m.id} value={m.id}>
                    {materialTypeLabels[m.materialType]} - {m.colorName} (EGP {m.pricePerGram}/gram)
                  </option>
                ))}
                {(!materialsList || materialsList.length === 0) && (
                  <option value="">No active materials available</option>
                )}
              </select>
            </div>
            
            <div>
              <label className="block text-sm font-medium mb-2">{t('quote.quality')}</label>
              <div className="grid grid-cols-2 gap-2">
                {qualities.map(q => (
                  <button 
                    type="button"
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
                className="w-full border rounded-md p-2 outline-none focus:ring-2 focus:ring-brand"
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
            <button type="button" onClick={() => setStep(1)} className="text-gray-600 px-4 py-2 border rounded-md font-medium hover:bg-gray-50">Back</button>
            <button type="button" onClick={() => setStep(3)} className="bg-brand text-white px-6 py-2 rounded-md font-medium">Next Step</button>
          </div>
        </div>
      )}

      {step === 3 && (
        <div className="space-y-6 bg-white p-6 rounded-xl shadow-sm border border-gray-100">
          <h2 className="text-xl font-semibold">Review & Estimate</h2>
          
          <div className="bg-gray-50 p-4 rounded-lg space-y-3">
            <div className="flex justify-between"><span className="text-gray-500">File</span><span className="font-medium">{file?.name}</span></div>
            {selectedMaterialOption && (
              <div className="flex justify-between">
                <span className="text-gray-500">Material</span>
                <span className="font-medium">{materialTypeLabels[selectedMaterialOption.materialType]} - {selectedMaterialOption.colorName}</span>
              </div>
            )}
            <div className="flex justify-between"><span className="text-gray-500">Quality</span><span className="font-medium">{quality}</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Quantity</span><span className="font-medium">{quantity}x</span></div>
            <div className="flex justify-between"><span className="text-gray-500">Design Services</span><span className="font-medium">{needsDesign ? 'Yes' : 'No'}</span></div>
          </div>

          <div className="border-t pt-4">
            <div className="flex justify-between items-center mb-6">
              <span className="text-lg font-bold">Estimated Cost</span>
              <span className="text-2xl font-bold text-brand">
                {selectedMaterialOption && uploadedModel?.estimatedWeightGrams 
                  ? `~ EGP ${(selectedMaterialOption.pricePerGram * uploadedModel.estimatedWeightGrams * quantity).toFixed(2)}`
                  : 'Pending geometry analysis'}
              </span>
            </div>
          </div>

          <div className="flex justify-between pt-4">
            <button type="button" onClick={() => setStep(2)} className="text-gray-600 px-4 py-2 border rounded-md font-medium hover:bg-gray-50">Back</button>
            <button 
              type="button" 
              disabled={createQuoteMutation.isPending}
              onClick={handleSubmit} 
              className="bg-brand text-white px-8 py-3 rounded-lg font-bold hover:bg-blue-600 disabled:opacity-50"
            >
              {createQuoteMutation.isPending ? 'Submitting...' : 'Submit Request'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

