import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { CheckCircle2, ChevronRight, UploadCloud } from 'lucide-react';

type Step = 'basic' | 'printer' | 'location' | 'availability' | 'agreement' | 'pending';

export default function Register() {
  const navigate = useNavigate();
  const [step, setStep] = useState<Step>('basic');
  
  // Basic Info
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  
  // Printer Info
  const [brand, setBrand] = useState('');
  const [model, setModel] = useState('');
  const [technology, setTechnology] = useState('FDM');
  
  // Location
  const [district, setDistrict] = useState('');
  
  // Availability
  const [idleHours, setIdleHours] = useState('8');
  
  const handleNext = (nextStep: Step) => (e: React.FormEvent) => {
    e.preventDefault();
    setStep(nextStep);
  };

  const handleComplete = () => {
    // Mock save logic
    setStep('pending');
  };

  return (
    <div className="min-h-screen flex flex-col bg-slate-50 p-4 pb-10">
      <div className="max-w-md w-full mx-auto">
        {/* Progress indicator */}
        {step !== 'pending' && (
          <div className="flex justify-between items-center mb-6 px-2">
            {['basic', 'printer', 'location', 'availability', 'agreement'].map((s, i, arr) => {
              const isActive = step === s;
              const isPast = arr.indexOf(step) > i;
              return (
                <div key={s} className="flex-1 flex items-center">
                  <div className={`h-2 flex-1 rounded-full ${isPast ? 'bg-primary' : isActive ? 'bg-primary/50' : 'bg-slate-200'}`} />
                  {i < arr.length - 1 && <div className="w-1" />}
                </div>
              );
            })}
          </div>
        )}

        <div className="bg-white rounded-xl shadow-sm border border-slate-100 p-6">
          {step === 'basic' && (
            <form onSubmit={handleNext('printer')} className="space-y-4">
              <h2 className="text-xl font-bold text-slate-900 mb-4">Create your account</h2>
              <div><label className="block text-sm mb-1">Full Name (Arabic or English)</label><input required value={name} onChange={e=>setName(e.target.value)} className="w-full p-3 border rounded-lg" /></div>
              <div><label className="block text-sm mb-1">Phone Number</label><input required type="tel" value={phone} onChange={e=>setPhone(e.target.value)} className="w-full p-3 border rounded-lg" /></div>
              <div><label className="block text-sm mb-1">Email</label><input required type="email" value={email} onChange={e=>setEmail(e.target.value)} className="w-full p-3 border rounded-lg" /></div>
              <div><label className="block text-sm mb-1">Password</label><input required type="password" value={password} onChange={e=>setPassword(e.target.value)} className="w-full p-3 border rounded-lg" /></div>
              <button type="submit" className="w-full bg-primary text-white p-3 rounded-lg flex justify-center items-center">Next <ChevronRight size={18} className="ml-1"/></button>
              <div className="text-center text-sm mt-4">Already a partner? <Link to="/login" className="text-primary font-medium">Login</Link></div>
            </form>
          )}

          {step === 'printer' && (
            <form onSubmit={handleNext('location')} className="space-y-4">
              <h2 className="text-xl font-bold text-slate-900 mb-4">Add your first printer</h2>
              <div><label className="block text-sm mb-1">Technology</label>
                <select value={technology} onChange={e=>setTechnology(e.target.value)} className="w-full p-3 border rounded-lg bg-white">
                  <option value="FDM">FDM (Filament)</option>
                  <option value="SLA">SLA / Resin</option>
                </select>
              </div>
              <div><label className="block text-sm mb-1">Brand</label><input required placeholder="e.g. Creality, Prusa" value={brand} onChange={e=>setBrand(e.target.value)} className="w-full p-3 border rounded-lg" /></div>
              <div><label className="block text-sm mb-1">Model</label><input required placeholder="e.g. Ender 3 V2" value={model} onChange={e=>setModel(e.target.value)} className="w-full p-3 border rounded-lg" /></div>
              <button type="submit" className="w-full bg-primary text-white p-3 rounded-lg flex justify-center items-center">Next <ChevronRight size={18} className="ml-1"/></button>
            </form>
          )}

          {step === 'location' && (
            <form onSubmit={handleNext('availability')} className="space-y-4">
              <h2 className="text-xl font-bold text-slate-900 mb-4">Where are you located?</h2>
              <div><label className="block text-sm mb-1">District (Cairo/Giza)</label>
                <select required value={district} onChange={e=>setDistrict(e.target.value)} className="w-full p-3 border rounded-lg bg-white">
                  <option value="">Select a district...</option>
                  <option value="Maadi">Maadi</option>
                  <option value="Nasr City">Nasr City</option>
                  <option value="Heliopolis">Heliopolis</option>
                  <option value="6th of October">6th of October</option>
                  <option value="Zayed">Sheikh Zayed</option>
                  <option value="Dokki">Dokki</option>
                </select>
              </div>
              <p className="text-sm text-slate-500">This helps us route local jobs to you to save on shipping time and costs.</p>
              <button type="submit" className="w-full bg-primary text-white p-3 rounded-lg flex justify-center items-center">Next <ChevronRight size={18} className="ml-1"/></button>
            </form>
          )}

          {step === 'availability' && (
            <form onSubmit={handleNext('agreement')} className="space-y-4">
              <h2 className="text-xl font-bold text-slate-900 mb-4">Your Availability</h2>
              <div><label className="block text-sm mb-1">Estimated idle hours per day</label>
                <input required type="number" min="1" max="24" value={idleHours} onChange={e=>setIdleHours(e.target.value)} className="w-full p-3 border rounded-lg" />
              </div>
              <p className="text-sm text-slate-500">You can always pause your availability later if you go on vacation or your printer needs maintenance.</p>
              <button type="submit" className="w-full bg-primary text-white p-3 rounded-lg flex justify-center items-center">Next <ChevronRight size={18} className="ml-1"/></button>
            </form>
          )}

          {step === 'agreement' && (
            <div className="space-y-4">
              <h2 className="text-xl font-bold text-slate-900 mb-4">Partner Agreement</h2>
              <div className="h-40 overflow-y-auto border p-3 rounded-lg text-sm text-slate-600 bg-slate-50">
                <p>1. Quality Standard: You agree to maintain a high print quality standard and report failed prints.</p>
                <p className="mt-2">2. Timeliness: You agree to ship or prepare orders for pickup within the agreed timeframe.</p>
                <p className="mt-2">3. Payouts: Payouts are calculated weekly based on successful QC approved prints.</p>
              </div>
              <button onClick={handleComplete} className="w-full bg-primary text-white p-3 rounded-lg flex justify-center items-center">Accept & Submit Application</button>
            </div>
          )}

          {step === 'pending' && (
            <div className="text-center py-8 space-y-4">
              <div className="mx-auto w-16 h-16 bg-success/10 text-success rounded-full flex items-center justify-center mb-4">
                <CheckCircle2 size={32} />
              </div>
              <h2 className="text-2xl font-bold text-slate-900">Application Received!</h2>
              <p className="text-slate-600">Our team is reviewing your application. We will contact you via email or phone within 24-48 hours to verify your printer setup.</p>
              <Link to="/login" className="block mt-6 w-full bg-slate-900 text-white p-3 rounded-lg font-medium">Return to Login</Link>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
